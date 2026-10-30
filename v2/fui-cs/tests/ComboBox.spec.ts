import { expect, test, type Page } from '@playwright/test';

import { boundsOf, debugNodes, labelOf, labelStartingWith, nextFrames, pointIn, screenshotPath, serveApp, waitForReady, type HostNode } from './support.js';

const baseUrl = serveApp();

const LIST_ITEM_ROLE = 7;

async function openWorkbench(page: Page): Promise<string[]> {
  const errors: string[] = [];
  page.on('pageerror', (error) => errors.push(error.message));
  page.on('console', (message) => {
    // The asset showcase loads two assets that do not exist on purpose.
    const expectedFailure = message.text().includes('assets/missing.') || message.location().url.includes('/assets/missing.');
    if (message.type() === 'error' && !expectedFailure) errors.push(message.text());
  });
  await page.setViewportSize({ width: 1024, height: 700 });
  await page.goto(`${baseUrl()}/v2/fui-cs/demo/workbench/`);
  await waitForReady(page);
  await expect.poll(async () => await labelOf(page, 'combo-readout')).toBe('Text: "" • Selected: none (-1) • Filtered: 10');
  // The custom combo box's five rows fit below it.
  await scrollIntoView(page, 'combo-custom', 200);
  return errors;
}

/** Wheels the page until the node sits inside the window with `roomBelow` pixels under it. */
async function scrollIntoView(page: Page, nodeId: string, roomBelow: number): Promise<void> {
  const viewportHeight = page.viewportSize()?.height ?? 700;
  await page.mouse.move(980, 300);
  await expect.poll(async () => {
    const bounds = await boundsOf(page, nodeId);
    if (bounds.y + bounds.height <= viewportHeight - roomBelow) return true;
    await page.mouse.wheel(0, 240);
    return false;
  }, { timeout: 10_000 }).toBe(true);
  await expect.poll(async () => {
    const before = (await boundsOf(page, nodeId)).y;
    await nextFrames(page);
    return (await boundsOf(page, nodeId)).y === before;
  }).toBe(true);
}

async function rows(page: Page): Promise<HostNode[]> {
  const listed = (await debugNodes(page)).filter((node) => node.semanticRole === LIST_ITEM_ROLE && node.bounds.height > 0);
  return [...listed].sort((a, b) => a.bounds.y - b.bounds.y);
}

const rowLabels = async (page: Page): Promise<string[]> => (await rows(page)).map((row) => row.semanticLabel);

/**
 * Clicks the combo box's text, away from its chevron, and waits for the browser's editor to take focus. A press on a
 * placeholder focuses the editor through the app, which the host applies a moment later; keys sent before then are lost.
 */
async function clickField(page: Page, nodeId: string): Promise<void> {
  const point = await pointIn(page, nodeId, 0.3);
  await page.mouse.move(point.x, point.y);
  await nextFrames(page);
  await page.mouse.click(point.x, point.y);
  await expect.poll(async () => await page.evaluate(() => document.activeElement?.getAttribute('data-effindom-hidden-editor'))).toBe('true');
}

async function clickRow(page: Page, label: string): Promise<void> {
  const row = (await rows(page)).find((candidate) => candidate.semanticLabel === label);
  if (row === undefined) throw new Error(`No open row is labelled "${label}".`);
  const canvas = await page.locator('#fui-canvas').boundingBox();
  await page.mouse.move((canvas?.x ?? 0) + row.bounds.x + 30, (canvas?.y ?? 0) + row.bounds.y + (row.bounds.height / 2));
  await nextFrames(page);
  await page.mouse.down();
  await page.mouse.up();
}

test.describe('ComboBox in the host', () => {
  test('typing filters and opens the list, and Backspace widens it again', async ({ page }) => {
    const errors = await openWorkbench(page);
    await clickField(page, 'combo-custom');

    await page.keyboard.type('sa');
    await expect.poll(async () => await rowLabels(page)).toEqual(['San Francisco', 'Osaka']);
    await expect.poll(async () => await labelOf(page, 'combo-readout')).toBe('Text: "sa" • Selected: none (-1) • Filtered: 2');
    await page.keyboard.press('Backspace');
    await expect.poll(async () => await labelOf(page, 'combo-readout')).toBe('Text: "s" • Selected: none (-1) • Filtered: 6');
    await page.screenshot({ path: screenshotPath('fui-cs-combobox-open.png') });
    expect(errors).toEqual([]);
  });

  test('the arrows move the highlight, Enter selects, and Escape closes', async ({ page }) => {
    const errors = await openWorkbench(page);
    await clickField(page, 'combo-custom');

    await page.keyboard.press('ArrowDown');
    await expect.poll(async () => (await rows(page)).length).toBe(10);
    await page.keyboard.press('ArrowDown');
    await page.keyboard.press('ArrowDown');
    await page.keyboard.press('Enter');
    await expect.poll(async () => await labelOf(page, 'combo-event')).toBe('Last event: changed Singapore (2)');
    await expect.poll(async () => (await rows(page)).length).toBe(0);

    // The selection's text filters the reopened list to itself.
    await page.keyboard.press('ArrowDown');
    await expect.poll(async () => await rowLabels(page)).toEqual(['Singapore']);
    await page.keyboard.press('Escape');
    await expect.poll(async () => (await rows(page)).length).toBe(0);
    expect(errors).toEqual([]);
  });

  test('a click on a row selects it, and a click outside closes the list without choosing', async ({ page }) => {
    const errors = await openWorkbench(page);
    await clickField(page, 'combo-custom');
    await page.keyboard.type('o');
    await expect.poll(async () => (await rows(page)).length).toBeGreaterThan(3);

    // The first row lies over the readouts, clear of the combo boxes below (see the fixme below).
    await clickRow(page, 'Melbourne');
    await expect.poll(async () => await labelOf(page, 'combo-event')).toBe('Last event: changed Melbourne (0)');
    await expect.poll(async () => (await rows(page)).length).toBe(0);

    await clickField(page, 'combo-custom');
    await page.keyboard.press('ArrowDown');
    await expect.poll(async () => (await rows(page)).length).toBe(1);
    const canvas = await page.locator('#fui-canvas').boundingBox();
    await page.mouse.click((canvas?.x ?? 0) + 900, (canvas?.y ?? 0) + 20);
    await expect.poll(async () => (await rows(page)).length).toBe(0);
    expect(await labelOf(page, 'combo-event')).toBe('Last event: changed Melbourne (0)');
    expect(errors).toEqual([]);
  });

  test('Tab moves on and closes the list; open-on-focus opens when focused', async ({ page }) => {
    const errors = await openWorkbench(page);
    await clickField(page, 'combo-custom');
    await page.keyboard.type('l');
    await expect.poll(async () => (await rows(page)).length).toBeGreaterThan(0);

    await page.keyboard.press('Tab');
    await expect.poll(async () => (await rows(page)).length).toBe(0);
    await clickField(page, 'combo-open-on-focus');
    await expect.poll(async () => (await rows(page)).length).toBe(10);
    expect(errors).toEqual([]);
  });

  test('an exact-match combo box selects the typed item on blur, autocomplete fills the rest, and templated rows are taller', async ({ page }) => {
    const errors = await openWorkbench(page);
    await scrollIntoView(page, 'combo-templated', 16);

    // The combo box's accessible label is its text.
    await clickField(page, 'combo-exact');
    await page.keyboard.type('paris');
    await page.keyboard.press('Tab');
    await expect.poll(async () => await labelOf(page, 'combo-exact')).toBe('Paris');

    await clickField(page, 'combo-autocomplete');
    await page.keyboard.type('lo');
    await expect.poll(async () => await labelOf(page, 'combo-autocomplete')).toBe('London');
    await expect.poll(async () => await rowLabels(page)).toEqual(['London']);
    await page.keyboard.press('Enter');
    await expect.poll(async () => (await rows(page)).length).toBe(0);

    await clickField(page, 'combo-templated');
    await expect.poll(async () => (await rows(page)).length).toBe(0);
    await page.keyboard.press('ArrowDown');
    await expect.poll(async () => (await rows(page)).map((row) => Math.round(row.bounds.height))).toContain(40);
    expect(errors).toEqual([]);
  });

  // The bridge sends a press to a combo box editor under the pointer even when another combo box's list is painted
  // over it, so a row lying over a combo box below cannot be clicked (in every SDK). Open host finding.
  test.fixme('a row over another combo box takes the click', async ({ page }) => {
    await openWorkbench(page);
    await clickField(page, 'combo-custom');
    await page.keyboard.type('o');
    await expect.poll(async () => (await rows(page)).length).toBeGreaterThan(3);
    await clickRow(page, 'Tokyo');
    await expect.poll(async () => await labelOf(page, 'combo-event')).toBe('Last event: changed Tokyo (5)');
  });

  // The UI runtime hands a held button's release to the text node whose selection is active, and pressing the
  // chevron focuses the editor and sets its caret, so the chevron never sees its click (in every SDK). Open host finding.
  test.fixme('the chevron opens and closes the list', async ({ page }) => {
    await openWorkbench(page);
    const field = await boundsOf(page, 'combo-custom');
    const canvas = await page.locator('#fui-canvas').boundingBox();
    await page.mouse.click((canvas?.x ?? 0) + field.x + field.width - 24, (canvas?.y ?? 0) + field.y + (field.height / 2));
    await expect.poll(async () => (await rows(page)).length).toBe(10);
  });
});

test.describe('ComboBox on the dashboard', () => {
  test('choosing a city updates the dashboard\'s status line', async ({ page }) => {
    const errors: string[] = [];
    page.on('pageerror', (error) => errors.push(error.message));
    await page.setViewportSize({ width: 1280, height: 900 });
    await page.goto(`${baseUrl()}/v2/fui-cs/demo/`);
    await waitForReady(page);
    await expect.poll(async () => await labelStartingWith(page, 'ComboBox: ')).toBe('ComboBox: Melbourne • Text: "Melbourne"');
    await scrollIntoView(page, 'demo-dashboard:city-combobox', 300);

    await clickField(page, 'demo-dashboard:city-combobox');
    await page.keyboard.press('Control+A');
    await page.keyboard.type('tor');
    await expect.poll(async () => await rowLabels(page)).toEqual(['Toronto']);
    await page.keyboard.press('Enter');
    await expect.poll(async () => await labelStartingWith(page, 'ComboBox: ')).toBe('ComboBox: Toronto • Text: "tor"');
    expect(errors).toEqual([]);
  });
});
