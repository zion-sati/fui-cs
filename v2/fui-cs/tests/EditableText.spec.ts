import { expect, test, type CDPSession, type Page } from '@playwright/test';

import { boundsOf, debugNodes, labelOf, nextFrames, pointIn, serveApp, waitForReady, type HostNode } from './support.js';

const baseUrl = serveApp();

async function openShowcase(page: Page): Promise<string[]> {
  const errors: string[] = [];
  page.on('pageerror', (error) => errors.push(error.message));
  page.on('console', (message) => {
    // The asset showcase loads two assets that do not exist on purpose.
    const expectedFailure = message.text().includes('assets/missing.') || message.location().url.includes('/assets/missing.');
    if (message.type() === 'error' && !expectedFailure) errors.push(message.text());
  });
  await page.goto(`${baseUrl()}/v2/fui-cs/demo/workbench/`);
  await waitForReady(page);
  await expect.poll(async () => await labelOf(page, 'text-input-value')).toBe('Value: <empty>');
  await page.mouse.move(980, 300);
  await expect.poll(async () => {
    const bounds = await boundsOf(page, 'text-input-colored');
    if (bounds.y + bounds.height <= (page.viewportSize()?.height ?? 700) - 16) return true;
    await page.mouse.wheel(0, 240);
    return false;
  }, { timeout: 10_000 }).toBe(true);
  await expect.poll(async () => {
    const before = (await boundsOf(page, 'text-input-plain')).y;
    await nextFrames(page);
    return (await boundsOf(page, 'text-input-plain')).y === before;
  }).toBe(true);
  return errors;
}

const value = async (page: Page): Promise<string | null> => await labelOf(page, 'text-input-value');

/**
 * Opens the context menu over a point of an input and clicks the item named `item`. The menu must not open over another
 * editable field: the bridge sends a press on a popup lying over an editor to that editor (an open host finding).
 */
async function chooseFromMenu(page: Page, nodeId: string, fx: number, item: string): Promise<void> {
  const point = await pointIn(page, nodeId, fx);
  await page.mouse.click(point.x, point.y, { button: 'right' });
  let entry: HostNode | undefined;
  await expect.poll(async () => {
    entry = [...(await debugNodes(page))].reverse().find((node) => node.semanticLabel === item && node.bounds.height > 0);
    return entry !== undefined;
  }).toBe(true);
  const canvas = await page.locator('#fui-canvas').boundingBox();
  await page.mouse.move((canvas?.x ?? 0) + (entry?.bounds.x ?? 0) + ((entry?.bounds.width ?? 0) / 2), (canvas?.y ?? 0) + (entry?.bounds.y ?? 0) + ((entry?.bounds.height ?? 0) / 2));
  await nextFrames(page);
  await page.mouse.down();
  await page.mouse.up();
}

/** The focused input's whole text, read by selecting and copying it (an empty selection leaves the clipboard as it was). */
async function copiedText(page: Page): Promise<string> {
  await page.evaluate(async () => { await navigator.clipboard.writeText('<unchanged>'); });
  await page.keyboard.press('Control+A');
  await page.keyboard.press('Control+C');
  await nextFrames(page);
  return await page.evaluate(async () => await navigator.clipboard.readText());
}

test.describe('Editable text integration', () => {
  test('the context menu cuts, pastes and undoes in a text input', async ({ page, context }) => {
    await context.grantPermissions(['clipboard-read', 'clipboard-write']);
    await page.setViewportSize({ width: 1024, height: 700 });
    const errors = await openShowcase(page);
    // The custom-colored input is the last in its column, with no editable field below it for the menu to cover.
    const input = 'text-input-colored';
    const point = await pointIn(page, input, 0.15);
    await page.mouse.click(point.x, point.y);
    await page.keyboard.type('alpha beta');
    await page.keyboard.press('Shift+Home');

    await chooseFromMenu(page, input, 0.15, 'Cut');
    await expect.poll(async () => await page.evaluate(async () => await navigator.clipboard.readText())).toBe('alpha beta');

    await page.evaluate(async () => { await navigator.clipboard.writeText('XY'); });
    await chooseFromMenu(page, input, 0.15, 'Paste');
    await expect.poll(async () => await copiedText(page)).toBe('XY');
    await page.keyboard.press('End');
    // Undo reverts the paste (the runtime may group it with the cut before it).
    await chooseFromMenu(page, input, 0.15, 'Undo');
    await expect.poll(async () => await copiedText(page)).not.toBe('XY');
    expect(errors).toEqual([]);
  });

  test('function keys insert nothing, and the editor keeps focus after a menu action', async ({ page, context }) => {
    await context.grantPermissions(['clipboard-read', 'clipboard-write']);
    await page.setViewportSize({ width: 1024, height: 700 });
    const errors = await openShowcase(page);
    const point = await pointIn(page, 'text-input-plain', 0.5);
    await page.mouse.click(point.x, point.y);
    await page.keyboard.type('ab');

    for (const key of ['F2', 'F7', 'F9', 'Insert']) {
      await page.keyboard.press(key);
    }
    await nextFrames(page);
    expect(await value(page)).toBe('Value: "ab"');

    await chooseFromMenu(page, 'text-input-plain', 0.05, 'Select All');
    await expect.poll(async () => await labelOf(page, 'text-input-selection')).toBe('Selection: 0-2 (bytes 0-2)');
    await page.keyboard.type('z');
    await expect.poll(async () => await value(page)).toBe('Value: "z"');
    expect(errors).toEqual([]);
  });
});

test.describe('Composite editor keys', () => {
  test('a combo box editor keeps Left and Right for its caret while Up and Down drive its list', async ({ page }) => {
    await page.setViewportSize({ width: 1024, height: 700 });
    const errors = await openShowcase(page);
    await page.mouse.move(980, 300);
    await expect.poll(async () => {
      const bounds = await boundsOf(page, 'combo-custom');
      if (bounds.y + bounds.height <= 480) return true;
      await page.mouse.wheel(0, 240);
      return false;
    }, { timeout: 10_000 }).toBe(true);
    await expect.poll(async () => {
      const before = (await boundsOf(page, 'combo-custom')).y;
      await nextFrames(page);
      return (await boundsOf(page, 'combo-custom')).y === before;
    }).toBe(true);
    const point = await pointIn(page, 'combo-custom', 0.3);
    await page.mouse.click(point.x, point.y);
    await expect.poll(async () => await page.evaluate(() => document.activeElement?.getAttribute('data-effindom-hidden-editor'))).toBe('true');

    await page.keyboard.type('pars');
    await page.keyboard.press('ArrowLeft');
    await page.keyboard.press('Shift+ArrowLeft');
    await page.keyboard.type('i');
    await expect.poll(async () => await labelOf(page, 'combo-readout')).toBe('Text: "pais" • Selected: none (-1) • Filtered: 0');
    await page.keyboard.press('ArrowLeft');
    await page.keyboard.type('r');
    await expect.poll(async () => await labelOf(page, 'combo-readout')).toBe('Text: "paris" • Selected: Paris (8) • Filtered: 1');
    await page.keyboard.press('Control+A');
    await page.keyboard.type('Pari');
    await page.keyboard.press('ArrowDown');
    await page.keyboard.press('Enter');
    await expect.poll(async () => await labelOf(page, 'combo-event')).toBe('Last event: changed Paris (8)');
    expect(errors).toEqual([]);
  });
});

test.describe('Editable text under touch', () => {
  test('a long press in a text input shows Cut, Copy and Paste, and Paste inserts the clipboard', async ({ browser }) => {
    const context = await browser.newContext({ hasTouch: true, isMobile: true, viewport: { width: 1024, height: 700 } });
    await context.grantPermissions(['clipboard-read', 'clipboard-write'], { origin: baseUrl() });
    const page = await context.newPage();
    const errors = await openShowcase(page);
    const cdp: CDPSession = await context.newCDPSession(page);
    const tap = async (x: number, y: number): Promise<void> => {
      await cdp.send('Input.dispatchTouchEvent', { type: 'touchStart', touchPoints: [{ x, y, id: 1 }] });
      await cdp.send('Input.dispatchTouchEvent', { type: 'touchEnd', touchPoints: [] });
    };

    const field = await pointIn(page, 'text-input-plain', 0.5);
    await tap(field.x, field.y);
    await page.keyboard.type('hello world');
    await expect.poll(async () => await value(page)).toBe('Value: "hello world"');
    await page.evaluate(async () => { await navigator.clipboard.writeText('X'); });

    // A long press on the first word selects it and shows the editable toolbar.
    const word = await pointIn(page, 'text-input-plain', 0.12);
    await cdp.send('Input.dispatchTouchEvent', { type: 'touchStart', touchPoints: [{ x: word.x, y: word.y, id: 1 }] });
    await page.waitForTimeout(800);
    await cdp.send('Input.dispatchTouchEvent', { type: 'touchEnd', touchPoints: [] });
    for (const name of ['Cut', 'Copy', 'Paste']) {
      await expect(page.getByRole('button', { name, exact: true })).toHaveCount(1);
    }

    const paste = await page.getByRole('button', { name: 'Paste', exact: true }).boundingBox();
    if (paste === null) throw new Error('Expected the Paste button.');
    await tap(paste.x + (paste.width / 2), paste.y + (paste.height / 2));
    await expect.poll(async () => await value(page)).toBe('Value: "X world"');
    expect(errors).toEqual([]);
    await context.close();
  });
});
