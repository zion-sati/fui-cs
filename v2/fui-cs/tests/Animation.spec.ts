import { expect, test } from '@playwright/test';

import { clickNode, labelOf, openTestPage, pixelIn, serveApp } from './support.js';

const baseUrl = serveApp();

test.describe('Animations and transitions in the host', () => {
  test('the host keeps sending frames while an animation runs, then stops', async ({ page }) => {
    await openTestPage(page, baseUrl(), 'animation');

    await clickNode(page, 'count-start');

    // 300 ms at the browser's frame rate gives many samples, ending exactly on the final value.
    await expect.poll(async () => await labelOf(page, 'sample-log'), { timeout: 5000 }).toMatch(/^done \d+$/);
    const samples = Number((await labelOf(page, 'sample-log'))?.split(' ')[1]);
    expect(samples).toBeGreaterThanOrEqual(5);
  });

  test('opacity and background transitions run to their new values', async ({ page }) => {
    await openTestPage(page, baseUrl(), 'animation');
    const background = await pixelIn(page, 'fade-box', 1.5, 0.5);
    const before = await pixelIn(page, 'fade-box');

    await clickNode(page, 'fade-toggle');

    // At the end: the green background at 0.2 opacity over the page.
    const expected = [0x16, 0xa3, 0x4a].map((channel, index) => (0.2 * channel) + (0.8 * background[index]));
    await expect.poll(async () => {
      const pixel = await pixelIn(page, 'fade-box');
      return Math.max(...expected.map((channel, index) => Math.abs(pixel[index] - channel)));
    }, { timeout: 5000 }).toBeLessThanOrEqual(6);
    expect(before.slice(0, 3)).toEqual([0x25, 0x63, 0xeb]);
  });

  test('animated scrolling moves through intermediate offsets to its target', async ({ page }) => {
    await openTestPage(page, baseUrl(), 'animation');

    await clickNode(page, 'scroll-down');

    await expect.poll(async () => await labelOf(page, 'scroll-log'), { timeout: 5000 }).toMatch(/^scroll 300 steps \d+$/);
    const steps = Number((await labelOf(page, 'scroll-log'))?.split(' ')[3]);
    expect(steps).toBeGreaterThanOrEqual(3);
  });

  test('a loaded handler reads the live device pixel ratio', async ({ page }) => {
    await openTestPage(page, baseUrl(), 'animation');

    const ratio = await page.evaluate(() => window.devicePixelRatio);
    await expect.poll(async () => await labelOf(page, 'dpr-log')).toBe(`dpr ${String(ratio)}`);
  });
});
