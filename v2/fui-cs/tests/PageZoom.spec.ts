import { expect, test, type Page } from '@playwright/test';

import { boundsOf, labelOf, nextFrames, openTestPage, serveApp } from './support.js';

const baseUrl = serveApp();

interface PageZoom {
  readonly scale: number;
  readonly offsetX: number;
  readonly offsetY: number;
}

async function pageZoom(page: Page): Promise<PageZoom> {
  return await page.evaluate(() => window.EffinDomBrowserBridge?.getPageZoom() ?? { scale: Number.NaN, offsetX: 0, offsetY: 0 });
}

/** Pinches in with the trackpad gesture browsers report as a ctrl-wheel over a point. */
async function ctrlWheel(page: Page, x: number, y: number, steps: number): Promise<void> {
  await page.mouse.move(x, y);
  await page.keyboard.down('Control');
  for (let step = 0; step < steps; ++step) {
    await page.mouse.wheel(0, -60);
  }
  await page.keyboard.up('Control');
  await nextFrames(page);
}

/** The screen point of a node's center under the current page zoom: the bridge maps screen to scene as (screen - offset) / scale. */
async function zoomedCenter(page: Page, nodeId: string): Promise<{ x: number; y: number }> {
  const bounds = await boundsOf(page, nodeId);
  const zoom = await pageZoom(page);
  const canvas = await page.locator('#fui-canvas').boundingBox();
  return {
    x: (canvas?.x ?? 0) + ((bounds.x + (bounds.width / 2)) * zoom.scale) + zoom.offsetX,
    y: (canvas?.y ?? 0) + ((bounds.y + (bounds.height / 2)) * zoom.scale) + zoom.offsetY,
  };
}

test.describe('Framework page zoom', () => {
  test('a ctrl-wheel pinch zooms the page, and clicks still reach the node drawn under the pointer', async ({ page }) => {
    await page.setViewportSize({ width: 1024, height: 700 });
    const errors = await openTestPage(page, baseUrl(), 'platform');
    expect((await pageZoom(page)).scale).toBe(1);

    const target = await zoomedCenter(page, 'zoom-target');
    await ctrlWheel(page, target.x, target.y, 8);
    await expect.poll(async () => (await pageZoom(page)).scale).toBeGreaterThan(1.2);

    const zoomed = await zoomedCenter(page, 'zoom-target');
    await page.mouse.click(zoomed.x, zoomed.y);
    await expect.poll(async () => await labelOf(page, 'zoom-log')).toBe('zoom target 1');
    expect(errors).toEqual([]);
  });

  test('the application can turn page zoom off', async ({ page }) => {
    await page.setViewportSize({ width: 1024, height: 700 });
    const errors = await openTestPage(page, baseUrl(), 'platform');

    const off = await zoomedCenter(page, 'zoom-off');
    await page.mouse.click(off.x, off.y);
    await nextFrames(page);
    await ctrlWheel(page, 300, 300, 8);
    await page.waitForTimeout(200);
    expect((await pageZoom(page)).scale).toBe(1);
    expect(errors).toEqual([]);
  });
});
