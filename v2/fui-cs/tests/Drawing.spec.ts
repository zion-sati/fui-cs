import { expect, test, type Page } from '@playwright/test';

import { clickNode, openTestPage, pixelAt, pixelIn, rgb, screenshotPath, serveApp } from './support.js';

const baseUrl = serveApp();

const RED = 0xdc2626;
const PURPLE = 0x7c3aed;
const GREEN = 0x16a34a;
const BLUE = 0x2563eb;
const ORANGE = 0xf97316;
const BLACK = 0x000000;
const WHITE = 0xffffff;
const HALVES_RED = 0xe11d48;
const HALVES_BLUE = 0x2563eb;
const YELLOW = 0xfde047;
const ROSE = 0xe11d48;

async function color(page: Page, x: number, y: number): Promise<number> {
  return rgb(await pixelAt(page, 'sketch', x, y));
}

/** Counts the host draw batches from now on: every custom drawable's repaint sends one. */
async function countDrawBatches(page: Page): Promise<void> {
  await page.evaluate(() => {
    type Imports = Record<string, Record<string, (...args: unknown[]) => unknown>>;
    const bridge = (globalThis as unknown as { __fuiCsTier3?: { imports: Imports } }).__fuiCsTier3;
    const host = bridge?.imports.fui_host;
    const draw = host?.fui_canvas_draw_batch;
    if (host === undefined || draw === undefined) throw new Error('The canvas import is missing.');
    const counter = globalThis as unknown as { __drawBatches: number };
    counter.__drawBatches = 0;
    host.fui_canvas_draw_batch = (...args: unknown[]) => {
      counter.__drawBatches += 1;
      return draw(...args);
    };
  });
}

async function drawBatches(page: Page): Promise<number> {
  return await page.evaluate(() => (globalThis as unknown as { __drawBatches: number }).__drawBatches);
}

test.describe('Custom drawing in the host', () => {
  test('shapes, paths and transforms are drawn where the C# drawing put them', async ({ page }) => {
    const errors = await openTestPage(page, baseUrl(), 'drawing');

    await expect.poll(async () => await color(page, 60, 40)).toBe(RED);
    expect(await color(page, 170, 40)).toBe(GREEN);
    expect(await color(page, 270, 40)).toBe(BLUE);
    expect(await color(page, 60, 120)).toBe(ORANGE);
    expect(await color(page, 11, 120)).toBe(BLACK);
    expect(await color(page, 190, 135)).toBe(RED);
    // A square rotated 45 degrees about (320, 120): its center and diagonal tips are blue, its old corners are not.
    expect(await color(page, 320, 120)).toBe(BLUE);
    expect(await color(page, 320, 100)).toBe(BLUE);
    expect(await color(page, 334, 106)).toBe(WHITE);
    expect(await color(page, 395, 295)).toBe(WHITE);
    expect(errors).toEqual([]);
    await page.screenshot({ path: screenshotPath('fui-cs-drawing.png') });
  });

  test('a rounded clip cuts what is drawn inside it', async ({ page }) => {
    await openTestPage(page, baseUrl(), 'drawing');
    await expect.poll(async () => await color(page, 260, 210)).toBe(YELLOW);

    // Inside the clip: the square, and the circle where it reaches in. Outside it, past an edge or a rounded corner,
    // nothing of either is drawn.
    expect(await color(page, 290, 240)).toBe(ROSE);
    expect(await color(page, 215, 165)).toBe(WHITE);
    expect(await color(page, 222, 172)).toBe(WHITE);
    expect(await color(page, 305, 255)).toBe(WHITE);
  });

  test('a bitmap drawn through its canvas shows as an image', async ({ page }) => {
    await openTestPage(page, baseUrl(), 'drawing');

    await expect.poll(async () => rgb(await pixelIn(page, 'pattern', 0.5, 0.5))).toBe(PURPLE);
    expect(rgb(await pixelIn(page, 'pattern', 0.05, 0.05))).toBe(YELLOW);
  });

  test('nothing repaints on its own, and painting stops again after a change', async ({ page }) => {
    await openTestPage(page, baseUrl(), 'drawing');
    await expect.poll(async () => await color(page, 60, 40)).toBe(RED);
    await page.waitForTimeout(500);
    await countDrawBatches(page);

    await page.waitForTimeout(1_000);
    const idle = await drawBatches(page);
    // A change repaints the drawing; then painting stops again.
    await clickNode(page, 'sketch', 0.15, 0.13);
    await expect.poll(async () => await color(page, 60, 40)).toBe(PURPLE);
    await page.waitForTimeout(500);
    const afterChange = await drawBatches(page);
    await page.waitForTimeout(1_000);

    expect(idle).toBe(0);
    expect(afterChange).toBeGreaterThan(0);
    expect(await drawBatches(page)).toBe(afterChange);
  });

  test('a TextLayout, a texture and an SVG are drawn once their resources are ready', async ({ page }) => {
    await openTestPage(page, baseUrl(), 'drawing');

    await expect.poll(async () => await color(page, 30, 240)).toBe(HALVES_RED);
    expect(await color(page, 70, 240)).toBe(HALVES_BLUE);
    await expect.poll(async () => await color(page, 150, 235)).toBe(BLACK);
    let inked = 0;
    for (let x = 10; x < 110; x += 2) {
      if ((await color(page, x, 182)) !== WHITE) inked += 1;
    }
    expect(inked).toBeGreaterThan(5);
  });

  test('InvalidateVisual repaints the drawing with the new state', async ({ page }) => {
    await openTestPage(page, baseUrl(), 'drawing');
    await expect.poll(async () => await color(page, 60, 40)).toBe(RED);

    await clickNode(page, 'sketch', 0.15, 0.13);

    await expect.poll(async () => await color(page, 60, 40)).toBe(PURPLE);
    expect(await color(page, 170, 40)).toBe(GREEN);
  });
});
