import { expect, test, type Page } from '@playwright/test';

import { boundsOf, clickNode, hostNode, labelOf, openTestPage, pixelIn, rgb, screenshotPath, serveApp } from './support.js';

const baseUrl = serveApp();

const RED = 0xe11d48;
const BLUE = 0x2563eb;
const GREEN = 0x16a34a;
const GREY = 0xcbd5e1;

async function waitForSize(page: Page, nodeId: string, width: number, height: number): Promise<void> {
  await expect.poll(async () => {
    const bounds = await boundsOf(page, nodeId);
    return `${String(bounds.width)}x${String(bounds.height)}`;
  }).toBe(`${String(width)}x${String(height)}`);
}

test.describe('Images and SVGs in the host', () => {
  test('a loaded image takes its intrinsic size and reports ready', async ({ page }) => {
    const errors = await openTestPage(page, baseUrl(), 'image');

    await waitForSize(page, 'natural', 40, 20);
    await expect.poll(async () => await labelOf(page, 'natural-state')).toBe('ready');
    expect(rgb(await pixelIn(page, 'natural', 0.25, 0.5))).toBe(RED);
    expect(rgb(await pixelIn(page, 'natural', 0.75, 0.5))).toBe(BLUE);
    // The page loads one missing image on purpose: Chromium logs a 404 without the URL and the host names the file.
    const notFound = 'Failed to load resource: the server responded with a status of 404 (Not Found)';
    expect(errors.filter((error) => error === notFound).length).toBeLessThanOrEqual(1);
    expect(errors.filter((error) => error !== notFound && !error.includes('assets/missing.png'))).toEqual([]);
    await page.screenshot({ path: screenshotPath('fui-cs-image.png') });
  });

  test('a fixed width keeps the aspect ratio and paints the scaled texture', async ({ page }) => {
    await openTestPage(page, baseUrl(), 'image');

    await waitForSize(page, 'scaled', 200, 100);
    expect(rgb(await pixelIn(page, 'scaled', 0.2, 0.5))).toBe(RED);
    expect(rgb(await pixelIn(page, 'scaled', 0.8, 0.5))).toBe(BLUE);
    const node = await hostNode(page, 'scaled');
    expect(node?.semanticLabel).toBe('Red and blue halves');
  });

  test('contain fits the texture inside the box with its aspect ratio', async ({ page }) => {
    await openTestPage(page, baseUrl(), 'image');
    await expect.poll(async () => rgb(await pixelIn(page, 'contain', 0.25, 0.5))).toBe(RED);

    // A 2:1 texture in a 100 x 100 box is 100 x 50, centered: the top and bottom quarters stay background.
    expect(rgb(await pixelIn(page, 'contain', 0.5, 0.1))).toBe(GREY);
    expect(rgb(await pixelIn(page, 'contain', 0.75, 0.5))).toBe(BLUE);
    expect(rgb(await pixelIn(page, 'nearest', 0.05, 0.5))).toBe(RED);
  });

  test('SVGs take their intrinsic size, keep their aspect ratio and take a tint', async ({ page }) => {
    await openTestPage(page, baseUrl(), 'image');

    await waitForSize(page, 'svg-natural', 30, 15);
    await waitForSize(page, 'svg-tinted', 60, 30);
    expect(rgb(await pixelIn(page, 'svg-natural'))).toBe(0x000000);
    expect(rgb(await pixelIn(page, 'svg-tinted'))).toBe(GREEN);
  });

  test('a missing image reports the failure', async ({ page }) => {
    await openTestPage(page, baseUrl(), 'image');

    await expect.poll(async () => await labelOf(page, 'missing-state')).toBe('failed:error');
  });

  test('clearing the source stops painting the texture', async ({ page }) => {
    await openTestPage(page, baseUrl(), 'image');
    await expect.poll(async () => rgb(await pixelIn(page, 'scaled', 0.2, 0.5))).toBe(RED);

    await clickNode(page, 'clear');

    await expect.poll(async () => rgb(await pixelIn(page, 'scaled', 0.2, 0.5))).not.toBe(RED);
  });
});
