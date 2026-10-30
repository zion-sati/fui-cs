import { expect, test, type Page } from '@playwright/test';

import { boundsOf, debugNodes, labelOf, nextFrames, pointIn, serveApp, waitForReady } from './support.js';

const baseUrl = serveApp();
const area = 'demo-advanced-controls:text-area';
const initialLength = 80;

async function openTextArea(page: Page): Promise<string[]> {
  const errors: string[] = [];
  page.on('pageerror', (error) => errors.push(error.message));
  page.on('console', (message) => {
    if (message.type() === 'error') errors.push(message.text());
  });
  await page.setViewportSize({ width: 1280, height: 900 });
  await page.goto(`${baseUrl()}/v2/fui-cs/demo/advanced-controls/`);
  await waitForReady(page);
  await expect.poll(async () => await labelOf(page, 'text-area-focus')).toBe(`Focus: blurred • Text length: ${String(initialLength)}`);
  // Bring the text area and its toggles into the window.
  await page.mouse.move(640, 450);
  await expect.poll(async () => {
    const bounds = await boundsOf(page, area);
    if (bounds.y <= 120) return true;
    await page.mouse.wheel(0, 200);
    return false;
  }, { timeout: 10_000 }).toBe(true);
  await expect.poll(async () => {
    const before = (await boundsOf(page, area)).y;
    await nextFrames(page);
    return (await boundsOf(page, area)).y === before;
  }).toBe(true);
  return errors;
}

async function clickArea(page: Page, fx = 0.5, fy = 0.5): Promise<void> {
  const point = await pointIn(page, area, fx, fy);
  await page.mouse.click(point.x, point.y);
  await expect.poll(async () => (await labelOf(page, 'text-area-focus'))?.startsWith('Focus: focused')).toBe(true);
}

async function clickNode(page: Page, nodeId: string): Promise<void> {
  const point = await pointIn(page, nodeId, 0.05, 0.5);
  await page.mouse.click(point.x, point.y);
}

const textLength = async (page: Page): Promise<number> =>
  Number(/Text length: (\d+)/.exec((await labelOf(page, 'text-area-focus')) ?? '')?.[1] ?? Number.NaN);
const settings = async (page: Page): Promise<string> => (await labelOf(page, 'text-area-settings')) ?? '';

test.describe('TextArea on the advanced controls route', () => {
  test('Enter adds lines and Tab inserts a tab, until Accepts Tab is off and Tab moves on', async ({ page }) => {
    const errors = await openTextArea(page);
    await clickArea(page);
    await page.keyboard.press('Control+End');

    await page.keyboard.press('Enter');
    await page.keyboard.type('Added');
    await page.keyboard.press('Tab');
    await page.keyboard.type('x');
    await expect.poll(async () => await textLength(page)).toBe(initialLength + 8);
    await expect.poll(async () => await labelOf(page, 'text-area-selection')).toBe(`Selection: ${String(initialLength + 8)}-${String(initialLength + 8)}`);

    await clickNode(page, 'demo-advanced-controls:accepts-tab-toggle');
    await expect.poll(async () => await settings(page)).toContain('Tabs: traverse');
    await clickArea(page);
    await page.keyboard.press('Tab');
    await expect.poll(async () => (await labelOf(page, 'text-area-focus'))?.startsWith('Focus: blurred')).toBe(true);
    expect(await textLength(page)).toBe(initialLength + 8);
    expect(errors).toEqual([]);
  });

  test('PageUp, PageDown and the arrows edit without inserting text or scrolling the route', async ({ page }) => {
    const errors = await openTextArea(page);
    await clickArea(page);
    const before = (await boundsOf(page, area)).y;

    for (const key of ['PageUp', 'PageDown', 'ArrowDown', 'ArrowDown', 'ArrowUp']) {
      await page.keyboard.press(key);
    }
    await nextFrames(page);
    expect(await textLength(page)).toBe(initialLength);
    expect((await boundsOf(page, area)).y).toBe(before);
    expect(errors).toEqual([]);
  });

  test('read-only keeps the text, and clearing the text shows the placeholder again', async ({ page }) => {
    const errors = await openTextArea(page);
    await clickNode(page, 'demo-advanced-controls:read-only-toggle');
    await expect.poll(async () => await settings(page)).toContain('Read-only: on');
    await clickArea(page);
    await page.keyboard.type('blocked');
    await page.keyboard.press('Backspace');
    await nextFrames(page);
    expect(await textLength(page)).toBe(initialLength);

    await clickNode(page, 'demo-advanced-controls:read-only-toggle');
    await clickArea(page);
    await page.keyboard.press('Control+A');
    await page.keyboard.press('Backspace');
    await expect.poll(async () => await textLength(page)).toBe(0);
    await expect.poll(async () => (await debugNodes(page)).some((node) => node.semanticLabel.startsWith('Type notes here'))).toBe(true);
    expect(errors).toEqual([]);
  });

  test('the scroll bar toggles and policy radios mirror each other, and turning wrapping off scrolls sideways', async ({ page }) => {
    const errors = await openTextArea(page);

    await clickNode(page, 'demo-advanced-controls:always-vertical-toggle');
    await expect.poll(async () => await settings(page)).toContain('Vertical: always');
    await clickNode(page, 'demo-advanced-controls:never-vertical-toggle');
    await expect.poll(async () => await settings(page)).toContain('Vertical: never');
    expect((await debugNodes(page)).find((node) => node.semanticLabel === 'Always show vertical scrollbar')).toBeDefined();

    await clickNode(page, 'demo-advanced-controls:wrapping-toggle');
    await expect.poll(async () => await settings(page)).toContain('Wrapping: off');
    await clickArea(page, 0.5, 0.2);
    await page.keyboard.press('Home');
    // One input event: typing each key waits about a frame, which on a CI runner outlasts the test's timeout.
    await page.keyboard.insertText('x'.repeat(220));
    // The long line overflows the area's viewport sideways.
    await expect.poll(async () => {
      const view = (await debugNodes(page)).find((node) => node.scroll.contentWidth > node.scroll.viewportWidth + 100 && node.scroll.viewportHeight > 150);
      return view !== undefined;
    }).toBe(true);
    expect(errors).toEqual([]);
  });

  test('line height, font and visibility settings apply', async ({ page }) => {
    const errors = await openTextArea(page);

    await clickNode(page, 'demo-advanced-controls:line-height-group');
    const fixed = (await debugNodes(page)).find((node) => node.semanticLabel === 'Line height: Fixed 28 px');
    const mono = (await debugNodes(page)).find((node) => node.semanticLabel === 'Text font: Monospace');
    const canvas = await page.locator('#fui-canvas').boundingBox();
    for (const node of [fixed, mono]) {
      if (node === undefined) throw new Error('Expected the line height and font radios.');
      await page.mouse.click((canvas?.x ?? 0) + node.bounds.x + 10, (canvas?.y ?? 0) + node.bounds.y + (node.bounds.height / 2));
    }
    await expect.poll(async () => await settings(page)).toContain('Line height: fixed 28px');
    await expect.poll(async () => await settings(page)).toContain('Font: monospace');

    const height = (await boundsOf(page, area)).height;
    await clickNode(page, 'demo-advanced-controls:visibility-dropdown');
    const collapsed = 'Visibility: Collapsed - remove layout space and hide the content';
    await expect.poll(async () => (await debugNodes(page)).some((node) => node.semanticRole === 7 && node.semanticLabel === collapsed)).toBe(true);
    await page.keyboard.press('End');
    await page.keyboard.press('Enter');
    await expect.poll(async () => await settings(page)).toContain('Visibility: collapsed');
    expect(height).toBe(220);
    await expect.poll(async () => (await boundsOf(page, area)).height).toBe(0);
    expect(errors).toEqual([]);
  });
});
