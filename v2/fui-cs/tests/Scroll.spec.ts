import { expect, test, type Page } from '@playwright/test';

import { boundsOf, clickNode, labelOf, nextFrames, openTestPage, pointIn, screenshotPath, serveApp } from './support.js';

const baseUrl = serveApp();

async function itemTop(page: Page, index: number): Promise<number> {
  return (await boundsOf(page, `item-${String(index)}`)).y - (await boundsOf(page, 'scroll')).y;
}

test.describe('ScrollView in the host', () => {
  test('the host reports the content extent and viewport', async ({ page }) => {
    const errors = await openTestPage(page, baseUrl(), 'scroll');
    const point = await pointIn(page, 'scroll');
    await page.mouse.move(point.x, point.y);
    await nextFrames(page);

    await page.mouse.wheel(0, 40);

    // 30 items of 40 px in a 200 px tall viewport.
    await expect.poll(async () => await labelOf(page, 'extent-log')).toBe('1200/200');
    expect(errors).toEqual([]);
  });

  test('wheel scrolling moves the content and reports the offset', async ({ page }) => {
    await openTestPage(page, baseUrl(), 'scroll');
    const point = await pointIn(page, 'scroll');
    await page.mouse.move(point.x, point.y);
    await nextFrames(page);

    await page.mouse.wheel(0, 120);

    // Smooth scrolling animates; once it settles, the reported offset matches where the host laid the content.
    await expect.poll(async () => {
      const offset = Number(await labelOf(page, 'offset-log'));
      return offset > 0 && (await itemTop(page, 0)) === -offset;
    }).toBe(true);
  });

  test('ScrollTo moves the view to the requested offset', async ({ page }) => {
    await openTestPage(page, baseUrl(), 'scroll');

    await clickNode(page, 'jump');

    await expect.poll(async () => await labelOf(page, 'offset-log')).toBe('400');
    // Item 10 starts at 400 px, so it is now at the top of the viewport.
    await expect.poll(async () => await itemTop(page, 10)).toBe(0);
    await page.screenshot({ path: screenshotPath('fui-cs-scroll.png') });
  });

  test('the offset stops at the end of the content', async ({ page }) => {
    await openTestPage(page, baseUrl(), 'scroll');
    const point = await pointIn(page, 'scroll');
    await page.mouse.move(point.x, point.y);
    await nextFrames(page);

    for (let step = 0; step < 10; ++step) {
      await page.mouse.wheel(0, 400);
    }

    // 1200 px of content in a 200 px viewport scrolls at most 1000 px.
    await expect.poll(async () => await labelOf(page, 'offset-log')).toBe('1000');
    await expect.poll(async () => await itemTop(page, 29)).toBe(160);
  });
});
