import { expect, test, type Page } from '@playwright/test';

import { boundsOf, clickNode, labelOf, openTestPage, pixelIn, rgb, screenshotPath, serveApp } from './support.js';

const baseUrl = serveApp();

async function expectLabel(page: Page, nodeId: string, text: string): Promise<void> {
  await expect.poll(async () => await labelOf(page, nodeId)).toBe(text);
}

/** The page point over the slider's track at `fraction` of its range, from the thumb's center at either end. */
async function trackPoint(page: Page, nodeId: string, fraction: number): Promise<{ x: number; y: number }> {
  const canvas = await page.locator('#fui-canvas').boundingBox();
  if (canvas === null) throw new Error('Expected the fui-cs canvas.');
  const bounds = await boundsOf(page, nodeId);
  const inset = 5 + 9;
  const vertical = bounds.height > bounds.width;
  const available = (vertical ? bounds.height : bounds.width) - (inset * 2);
  return vertical
    ? { x: canvas.x + bounds.x + (bounds.width / 2), y: canvas.y + bounds.y + inset + (available * (1 - fraction)) }
    : { x: canvas.x + bounds.x + inset + (available * fraction), y: canvas.y + bounds.y + (bounds.height / 2) };
}

test.describe('Slider and ProgressBar in the host', () => {
  test('pressing the track sets the value and the progress bar follows', async ({ page }) => {
    const errors = await openTestPage(page, baseUrl(), 'range');
    const accent = rgb(await pixelIn(page, 'progress', 0.05, 0.5));
    expect(rgb(await pixelIn(page, 'progress', 0.7, 0.5))).not.toBe(accent);

    const point = await trackPoint(page, 'volume', 0.75);
    await page.mouse.move(point.x, point.y);
    await page.mouse.down();
    await page.mouse.up();

    await expectLabel(page, 'volume-state', '75');
    await expect(page.getByRole('slider', { name: 'Slider' }).first()).toHaveAttribute('aria-valuenow', '75');
    await expect.poll(async () => rgb(await pixelIn(page, 'progress', 0.7, 0.5))).toBe(accent);
    expect(rgb(await pixelIn(page, 'progress', 0.8, 0.5))).not.toBe(accent);
    expect(errors).toEqual([]);
  });

  test('dragging the thumb follows the pointer past the ends', async ({ page }) => {
    await openTestPage(page, baseUrl(), 'range');
    const start = await trackPoint(page, 'volume', 0.2);
    const quarter = await trackPoint(page, 'volume', 0.5);

    await page.mouse.move(start.x, start.y);
    await page.mouse.down();
    await page.mouse.move(quarter.x, quarter.y, { steps: 5 });
    await expectLabel(page, 'volume-state', '50');
    await page.mouse.move(790, start.y + 200, { steps: 5 });
    await expectLabel(page, 'volume-state', '100');
    await page.mouse.up();
    await page.mouse.move(2, 2);

    const thumb = await trackPoint(page, 'volume', 1);
    const canvas = await page.locator('#fui-canvas').boundingBox();
    const bounds = await boundsOf(page, 'volume');
    // The thumb sits at the end; the slider's own background shows at its old place.
    const end = await pixelIn(page, 'volume', (thumb.x - (canvas?.x ?? 0) - bounds.x) / bounds.width, 0.5);
    expect(rgb(end)).toBe(rgb(await pixelIn(page, 'progress', 0.05, 0.5)));
    await page.screenshot({ path: screenshotPath('fui-cs-range.png') });
  });

  test('the arrow keys, Home and End step the focused slider', async ({ page }) => {
    await openTestPage(page, baseUrl(), 'range');
    const point = await trackPoint(page, 'steps', 0);
    await page.mouse.move(point.x, point.y);
    await page.mouse.down();
    await page.mouse.up();
    await expectLabel(page, 'steps-state', '0');

    await page.keyboard.press('ArrowRight');
    await expectLabel(page, 'steps-state', '2');
    await page.keyboard.press('ArrowRight');
    await expectLabel(page, 'steps-state', '4');
    await page.keyboard.press('ArrowLeft');
    await expectLabel(page, 'steps-state', '2');
    await page.keyboard.press('End');
    await expectLabel(page, 'steps-state', '10');
    await page.keyboard.press('Home');
    await expectLabel(page, 'steps-state', '0');
  });

  test('a vertical slider increases upwards and a vertical progress bar fills from the bottom', async ({ page }) => {
    await openTestPage(page, baseUrl(), 'range');
    const accent = rgb(await pixelIn(page, 'level-bar', 0.5, 0.95));
    expect(rgb(await pixelIn(page, 'level-bar', 0.5, 0.2))).not.toBe(accent);

    const point = await trackPoint(page, 'level', 0.75);
    await page.mouse.move(point.x, point.y);
    await page.mouse.down();
    await page.mouse.up();
    await expectLabel(page, 'level-state', '75');
    await page.keyboard.press('ArrowUp');
    await expectLabel(page, 'level-state', '76');
    await expect(page.getByRole('slider', { name: 'Vertical slider' })).toHaveAttribute('aria-orientation', 'vertical');
  });

  test('a disabled slider ignores the pointer and keys', async ({ page }) => {
    await openTestPage(page, baseUrl(), 'range');

    await clickNode(page, 'locked', 0.9, 0.5);
    await page.keyboard.press('End');

    await page.waitForTimeout(100);
    expect(await labelOf(page, 'locked-state')).toBe('50');
    await expect(page.getByRole('slider').last()).toHaveAttribute('aria-disabled', 'true');
  });
});
