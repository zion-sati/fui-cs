import { expect, test, type Page } from '@playwright/test';

import { screenshotPath, serveApp, waitForReady } from './support.js';

declare global {
  interface Window {
    __fuiState?: {
      readonly commandWordCount: number;
      readonly commandWords: readonly number[];
      readonly rootHandle: string | null;
    };
  }
}

const appUrl = serveApp();

test('renders the fui-cs smoke through the managed harness', async ({ page }) => {
  const errors: string[] = [];
  page.on('pageerror', (error) => {
    errors.push(error.message);
  });

  await page.goto(`${appUrl()}/v2/fui-cs/index.html`);

  await expect.poll(async () => {
    return await page.evaluate(() => {
      if (window.__fuiError !== undefined) return `error:${window.__fuiError}`;
      return window.__fuiReady === true ? 'ready' : 'pending';
    });
  }).toBe('ready');

  if (errors.length > 0) {
    throw new Error(`Page errors: ${errors.join('; ')}`);
  }
  await expect(page.locator('#effindom-loading-overlay')).toHaveCount(0);

  const state = await page.evaluate(() => window.__fuiState);
  expect(state).toBeDefined();
  if (state === undefined) throw new Error('Expected fui-cs state.');
  expect(state.commandWordCount).toBeGreaterThan(0);
  expect(state.rootHandle).not.toBeNull();

  await page.screenshot({ path: screenshotPath('fui-cs-smoke.png') });
});

test('routes host input through the C# exports without errors', async ({ page }) => {
  const errors: string[] = [];
  page.on('pageerror', (error) => {
    errors.push(error.message);
  });
  page.on('console', (message) => {
    if (message.type() === 'error') errors.push(message.text());
  });

  await page.goto(`${appUrl()}/v2/fui-cs/index.html`);
  await waitForReady(page);

  const canvas = await page.locator('#fui-canvas').boundingBox();
  if (canvas === null) throw new Error('Expected the smoke canvas.');
  // Over the box, over the text row, then off the canvas.
  await page.mouse.move(canvas.x + 84, canvas.y + 72);
  await page.mouse.down();
  await page.mouse.up();
  await page.mouse.dblclick(canvas.x + 84, canvas.y + 72);
  await page.mouse.move(canvas.x + 40, canvas.y + 160);
  await page.mouse.wheel(0, 120);
  await page.keyboard.press('Tab');
  await page.keyboard.press('a');
  await page.mouse.move(2, 2);

  expect(errors).toEqual([]);
});

test('a click repaints the retained box without rebuilding the root', async ({ page }) => {
  await page.goto(`${appUrl()}/v2/fui-cs/index.html`);
  await waitForReady(page);
  const rootBefore = await page.evaluate(() => window.__fuiState?.rootHandle ?? null);
  const canvas = await page.locator('#fui-canvas').boundingBox();
  if (canvas === null) throw new Error('Expected the smoke canvas.');
  // The smoke page pads by 24 px and puts the 120 x 96 box first.
  const inside = { x: canvas.x + 44, y: canvas.y + 44, width: 40, height: 40 };
  const before = await page.screenshot({ clip: inside });

  await page.mouse.click(canvas.x + 84, canvas.y + 72);

  await expect.poll(async () => Buffer.compare(await page.screenshot({ clip: inside }), before) !== 0).toBe(true);
  expect(await page.evaluate(() => window.__fuiState?.rootHandle ?? null)).toBe(rootBefore);
  await page.screenshot({ path: screenshotPath('fui-cs-smoke-clicked.png') });
});

interface Extent {
  readonly left: number;
  readonly right: number;
}

/** Horizontal extents, in the clip, of the orange underline and of the dark glyphs of the first word above it. */
async function measureUnderlineAndWord(page: Page, png: Buffer): Promise<{ underline: Extent | null; word: Extent | null }> {
  return await page.evaluate(async (base64) => {
    const bytes = Uint8Array.from(atob(base64), (character) => character.charCodeAt(0));
    const bitmap = await createImageBitmap(new Blob([bytes], { type: 'image/png' }));
    const context = new OffscreenCanvas(bitmap.width, bitmap.height).getContext('2d');
    if (context === null) throw new Error('Expected a 2D context.');
    context.drawImage(bitmap, 0, 0);
    const { data, width, height } = context.getImageData(0, 0, bitmap.width, bitmap.height);
    const extent = (rows: readonly number[], test: (r: number, g: number, b: number) => boolean, maxX: number): { left: number; right: number } | null => {
      let left = Number.POSITIVE_INFINITY;
      let right = Number.NEGATIVE_INFINITY;
      for (const y of rows) {
        for (let x = 0; x < Math.min(width, maxX); ++x) {
          const offset = (y * width + x) * 4;
          if (test(data[offset] ?? 0, data[offset + 1] ?? 0, data[offset + 2] ?? 0)) {
            left = Math.min(left, x);
            right = Math.max(right, x + 1);
          }
        }
      }
      return Number.isFinite(left) ? { left, right } : null;
    };
    const isOrange = (r: number, g: number, b: number): boolean => r > 200 && g > 100 && g < 180 && b < 80;
    const isInk = (r: number, g: number, b: number): boolean => r + g + b < 300;
    const all = Array.from({ length: height }, (_, y) => y);
    const underlineRows = all.filter((y) => extent([y], isOrange, width) !== null);
    const underline = extent(underlineRows, isOrange, width);
    if (underline === null || underlineRows.length === 0) return { underline: null, word: null };
    const top = Math.min(...underlineRows);
    // The word sits in the text row above the underline; "right" starts 32 px after the measured "left".
    const textRows = all.filter((y) => y < top && y >= top - 50);
    return { underline, word: extent(textRows, isInk, underline.right + 16) };
  }, png.toString('base64'));
}

test('a TextLayout measurement sizes the underline to the rendered word', async ({ page }) => {
  await page.goto(`${appUrl()}/v2/fui-cs/index.html`);
  await waitForReady(page);
  const canvas = await page.locator('#fui-canvas').boundingBox();
  if (canvas === null) throw new Error('Expected the smoke canvas.');

  const { underline, word } = await measureUnderlineAndWord(page, await page.screenshot({ clip: canvas }));

  expect(underline).not.toBeNull();
  expect(word).not.toBeNull();
  if (underline === null || word === null) return;
  // The measured advance width spans the glyphs' ink plus side bearings.
  expect(underline.left).toBe(24);
  expect(underline.right - underline.left).toBeGreaterThanOrEqual(word.right - word.left);
  expect(underline.right - underline.left - (word.right - word.left)).toBeLessThanOrEqual(8);
  expect(Math.abs(word.left - underline.left)).toBeLessThanOrEqual(4);
});

test('the wheel scrolls the page and the C# ScrollChanged handler sees the offset', async ({ page }) => {
  await page.goto(`${appUrl()}/v2/fui-cs/index.html`);
  await waitForReady(page);
  const canvas = await page.locator('#fui-canvas').boundingBox();
  if (canvas === null) throw new Error('Expected the smoke canvas.');
  const titleBefore = await page.title();
  const boxArea = { x: canvas.x + 44, y: canvas.y + 44, width: 40, height: 40 };
  const before = await page.screenshot({ clip: boxArea });

  // Over blank space to the right of the box, so the scroll view itself takes the wheel.
  await page.mouse.move(canvas.x + 260, canvas.y + 60);
  await page.mouse.wheel(0, 120);

  await expect.poll(async () => await page.title()).toBe('fui-cs smoke (scrolled)');
  await expect.poll(async () => Buffer.compare(await page.screenshot({ clip: boxArea }), before) !== 0).toBe(true);
  expect(titleBefore).not.toBe('fui-cs smoke (scrolled)');
  await page.screenshot({ path: screenshotPath('fui-cs-smoke-scrolled.png') });
});
