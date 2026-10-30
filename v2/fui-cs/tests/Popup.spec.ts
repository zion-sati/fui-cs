import { expect, test } from '@playwright/test';

import { boundsOf, clickNode, hostNode, labelOf, nextFrames, openTestPage, pointIn, screenshotPath, serveApp } from './support.js';

const baseUrl = serveApp();

const GAP = 4;
const EDGE = 8;

test.describe('Popup in the host', () => {
  test('an anchored popup opens below its anchor with its content', async ({ page }) => {
    const errors = await openTestPage(page, baseUrl(), 'popup');
    const anchor = await boundsOf(page, 'anchor-top');

    await clickNode(page, 'anchor-top');

    await expect.poll(async () => await hostNode(page, 'popup-surface')).not.toBeNull();
    expect(await boundsOf(page, 'popup-surface')).toEqual({
      x: anchor.x,
      y: anchor.y + anchor.height + GAP,
      width: 200,
      height: 100,
    });
    expect(await labelOf(page, 'popup-text')).toBe('Popup content');
    expect(errors).toEqual([]);
    await page.screenshot({ path: screenshotPath('fui-cs-popup.png') });
  });

  test('without room below, an anchored popup opens above its anchor', async ({ page }) => {
    await openTestPage(page, baseUrl(), 'popup');
    const anchor = await boundsOf(page, 'anchor-bottom');

    await clickNode(page, 'anchor-bottom');

    await expect.poll(async () => await hostNode(page, 'popup-surface')).not.toBeNull();
    expect(await boundsOf(page, 'popup-surface')).toMatchObject({ x: anchor.x, y: anchor.y - 100 - GAP });
  });

  test('a popup shown past the viewport edge is kept inside it', async ({ page }) => {
    await openTestPage(page, baseUrl(), 'popup');

    await clickNode(page, 'point-button');

    await expect.poll(async () => await hostNode(page, 'popup-surface')).not.toBeNull();
    expect(await boundsOf(page, 'popup-surface')).toMatchObject({ x: 800 - 200 - EDGE, y: 600 - 100 - EDGE });
  });

  test('clicks on the panel and its content keep the popup open and a backdrop click dismisses it', async ({ page }) => {
    await openTestPage(page, baseUrl(), 'popup');
    await clickNode(page, 'anchor-top');
    await expect.poll(async () => await hostNode(page, 'popup-surface')).not.toBeNull();

    await clickNode(page, 'popup-surface', 0.9, 0.9);
    await clickNode(page, 'popup-text');
    await nextFrames(page);
    expect(await hostNode(page, 'popup-surface')).not.toBeNull();
    await clickNode(page, 'popup-page', 0.9, 0.5);

    await expect.poll(async () => await hostNode(page, 'popup-surface')).toBeNull();
  });

  test('a backdrop click in the same frame as the move off the content still dismisses', async ({ page }) => {
    // The host coalesces moves to the next frame; it must deliver the move's Leave/Enter before a same-frame press,
    // or the late Leave from the content disarms the backdrop's click.
    await openTestPage(page, baseUrl(), 'popup');
    await clickNode(page, 'anchor-top');
    await expect.poll(async () => await hostNode(page, 'popup-surface')).not.toBeNull();
    await clickNode(page, 'popup-text');
    await nextFrames(page);

    const backdrop = await pointIn(page, 'popup-page', 0.9, 0.5);
    await page.mouse.click(backdrop.x, backdrop.y);

    await expect.poll(async () => await hostNode(page, 'popup-surface'), { timeout: 2000 }).toBeNull();
  });
});
