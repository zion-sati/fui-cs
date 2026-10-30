import { expect, test, type CDPSession, type Page } from '@playwright/test';

import { boundsOf, hostNode, labelOf, openTestPage, pointIn, screenshotPath, serveApp } from './support.js';

const baseUrl = serveApp();

async function longPress(page: Page, cdp: CDPSession, nodeId: string, fx: number, fy = 0.5): Promise<void> {
  const point = await pointIn(page, nodeId, fx, fy);
  await cdp.send('Input.dispatchTouchEvent', { type: 'touchStart', touchPoints: [{ x: point.x, y: point.y, id: 1 }] });
  await page.waitForTimeout(800);
  await cdp.send('Input.dispatchTouchEvent', { type: 'touchEnd', touchPoints: [] });
}

async function tap(cdp: CDPSession, x: number, y: number): Promise<void> {
  await cdp.send('Input.dispatchTouchEvent', { type: 'touchStart', touchPoints: [{ x, y, id: 1 }] });
  await cdp.send('Input.dispatchTouchEvent', { type: 'touchEnd', touchPoints: [] });
}

async function visible(page: Page, nodeId: string): Promise<boolean> {
  const node = await hostNode(page, nodeId);
  return node !== null && node.visibleBounds.width > 0 && node.visibleBounds.height > 0;
}

test.describe('Touch text selection in the host', () => {
  test('a long press selects a word and shows its handles and toolbar; Copy copies it', async ({ page, context }) => {
    await context.grantPermissions(['clipboard-read', 'clipboard-write']);
    const errors = await openTestPage(page, baseUrl(), 'selection');
    const cdp = await page.context().newCDPSession(page);

    // "word" sits about a third of the way into the sentence.
    await longPress(page, cdp, 'paragraph', 0.36);

    await expect.poll(async () => await labelOf(page, 'text-selection-log')).toMatch(/^\w+$/);
    const word = (await labelOf(page, 'text-selection-log')) ?? '';
    await expect.poll(async () => await visible(page, 'fui-selection-start-handle')).toBe(true);
    expect(await visible(page, 'fui-selection-end-handle')).toBe(true);
    await expect(page.getByRole('button', { name: 'Copy', exact: true })).toHaveCount(1);
    const toolbar = await boundsOf(page, 'fui-text-toolbar');
    const paragraph = await boundsOf(page, 'paragraph');
    expect(toolbar.y + toolbar.height).toBeLessThanOrEqual(paragraph.y);
    await page.screenshot({ path: screenshotPath('fui-cs-selection-toolbar.png') });

    const copy = await page.getByRole('button', { name: 'Copy', exact: true }).boundingBox();
    if (copy === null) throw new Error('Expected the Copy button.');
    await tap(cdp, copy.x + (copy.width / 2), copy.y + (copy.height / 2));

    await expect.poll(async () => await page.evaluate(async () => await navigator.clipboard.readText())).toBe(word);
    await expect.poll(async () => await visible(page, 'fui-text-toolbar')).toBe(false);
    expect(errors).toEqual([]);
  });

  test('dragging the end handle extends the selection and the toolbar returns after the drag', async ({ page }) => {
    await openTestPage(page, baseUrl(), 'selection');
    const cdp = await page.context().newCDPSession(page);
    await longPress(page, cdp, 'paragraph', 0.36);
    await expect.poll(async () => await visible(page, 'fui-selection-end-handle')).toBe(true);
    const word = (await labelOf(page, 'text-selection-log')) ?? '';
    const canvas = await page.locator('#fui-canvas').boundingBox();
    const handle = await boundsOf(page, 'fui-selection-end-handle');
    // The knob sits 25 px down the hit target, 18 px in from its left edge.
    const start = { x: (canvas?.x ?? 0) + handle.x + 27, y: (canvas?.y ?? 0) + handle.y + 34 };
    const paragraph = await boundsOf(page, 'paragraph');

    await cdp.send('Input.dispatchTouchEvent', { type: 'touchStart', touchPoints: [{ x: start.x, y: start.y, id: 1 }] });
    for (let step = 1; step <= 8; ++step) {
      await cdp.send('Input.dispatchTouchEvent', { type: 'touchMove', touchPoints: [{ x: start.x + (step * 15), y: start.y, id: 1 }] });
    }
    await expect.poll(async () => await visible(page, 'fui-text-toolbar')).toBe(false);
    await cdp.send('Input.dispatchTouchEvent', { type: 'touchEnd', touchPoints: [] });

    await expect.poll(async () => ((await labelOf(page, 'text-selection-log')) ?? '').length).toBeGreaterThan(word.length);
    expect(await labelOf(page, 'text-selection-log')).toContain(word);
    await expect.poll(async () => await visible(page, 'fui-text-toolbar')).toBe(true);
    expect(paragraph.width).toBeGreaterThan(0);
  });

  test('a tap outside closes the toolbar', async ({ page }) => {
    await openTestPage(page, baseUrl(), 'selection');
    const cdp = await page.context().newCDPSession(page);
    await longPress(page, cdp, 'paragraph', 0.36);
    await expect.poll(async () => await visible(page, 'fui-text-toolbar')).toBe(true);

    const canvas = await page.locator('#fui-canvas').boundingBox();
    await tap(cdp, (canvas?.x ?? 0) + 700, (canvas?.y ?? 0) + 550);

    await expect.poll(async () => await visible(page, 'fui-text-toolbar')).toBe(false);
  });

  test('a long press in a selection area selects across its text with Copy and Select all', async ({ page }) => {
    await openTestPage(page, baseUrl(), 'selection');
    const cdp = await page.context().newCDPSession(page);

    await longPress(page, cdp, 'area-first', 0.5);

    await expect.poll(async () => await labelOf(page, 'area-selection-log')).not.toBe('none');
    await expect(page.getByRole('button', { name: 'Select all', exact: true })).toHaveCount(1);
    const selectAll = await page.getByRole('button', { name: 'Select all', exact: true }).boundingBox();
    if (selectAll === null) throw new Error('Expected the Select all button.');
    await tap(cdp, selectAll.x + (selectAll.width / 2), selectAll.y + (selectAll.height / 2));
    await expect.poll(async () => await labelOf(page, 'area-selection-log')).toContain('Alpha bravo charlie');
  });

  test('a long press on an image link puts the items past three behind More', async ({ page }) => {
    await openTestPage(page, baseUrl(), 'selection');
    const cdp = await page.context().newCDPSession(page);
    await expect.poll(async () => (await boundsOf(page, 'selection-image')).height).toBeGreaterThan(0);

    await longPress(page, cdp, 'selection-image-link', 0.5);

    await expect.poll(async () => await visible(page, 'fui-text-toolbar')).toBe(true);
    await expect(page.getByRole('button', { name: 'More', exact: true })).toHaveCount(1);
    const row = await boundsOf(page, 'fui-text-toolbar');
    // The link's New Tab and Open, then the image's New Tab; the image's Open waits behind More.
    await expect(page.getByRole('button', { name: 'New Tab', exact: true })).toHaveCount(2);
    await expect(page.getByRole('button', { name: 'Open', exact: true })).toHaveCount(1);
    const more = await page.getByRole('button', { name: 'More', exact: true }).boundingBox();
    if (more === null) throw new Error('Expected the More button.');
    await tap(cdp, more.x + (more.width / 2), more.y + (more.height / 2));

    await expect.poll(async () => await visible(page, 'fui-text-toolbar-overflow')).toBe(true);
    expect(await visible(page, 'fui-text-toolbar')).toBe(false);
    // The short panel fits below where the row was, so it opens just under it.
    const overflow = await boundsOf(page, 'fui-text-toolbar-overflow');
    expect(Math.abs(overflow.y - (row.y + row.height + 4))).toBeLessThanOrEqual(1);
    await expect(page.getByRole('button', { name: 'Open', exact: true })).toHaveCount(1);
    const back = await page.getByRole('button', { name: '<', exact: true }).boundingBox();
    if (back === null) throw new Error('Expected the back button.');
    await page.screenshot({ path: screenshotPath('fui-cs-selection-overflow.png') });
    await tap(cdp, back.x + (back.width / 2), back.y + (back.height / 2));

    await expect.poll(async () => await visible(page, 'fui-text-toolbar')).toBe(true);
    expect(await visible(page, 'fui-text-toolbar-overflow')).toBe(false);
  });

  test('a long press on a link offers it in the toolbar', async ({ page }) => {
    await openTestPage(page, baseUrl(), 'selection');
    const cdp = await page.context().newCDPSession(page);

    await longPress(page, cdp, 'selection-link', 0.5);

    await expect(page.getByRole('button', { name: 'New Tab', exact: true })).toHaveCount(1);
    await expect(page.getByRole('button', { name: 'Open', exact: true })).toHaveCount(1);
  });
});
