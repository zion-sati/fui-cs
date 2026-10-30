import { expect, test, type Page } from '@playwright/test';

import { boundsOf, debugNodes, hostNode, rgb, pixelAt, screenshotPath, serveApp, waitForReady, type Bounds } from './support.js';

const baseUrl = serveApp();

const PAGE = 0x141428;
const NEEDLE = 0xdc3232;

async function openRoute(page: Page): Promise<string[]> {
  const errors: string[] = [];
  page.on('pageerror', (error) => errors.push(error.message));
  page.on('console', (message) => {
    if (message.type() === 'error') errors.push(message.text());
  });
  await page.setViewportSize({ width: 1280, height: 900 });
  await page.goto(`${baseUrl()}/v2/fui-cs/demo/immediate-drawing/`);
  await waitForReady(page);
  await expect.poll(async () => (await hostNode(page, 'widget-paint')) !== null).toBe(true);
  return errors;
}

/** The custom-drawn cards, in reading order. */
async function cards(page: Page): Promise<Bounds[]> {
  const nodes = await debugNodes(page);
  return nodes
    .filter((node) => (node as unknown as { behavior: { customDrawable: boolean } }).behavior.customDrawable)
    .map((node) => node.bounds)
    .sort((a, b) => (a.y - b.y) || (a.x - b.x));
}

/** The RGBA bytes of a canvas region, in page pixels. */
async function region(page: Page, area: Bounds): Promise<number[]> {
  const canvas = await page.locator('#fui-canvas').boundingBox();
  if (canvas === null) throw new Error('Expected the fui-cs canvas.');
  const png = await page.screenshot({ clip: { x: canvas.x + area.x, y: canvas.y + area.y, width: area.width, height: area.height } });
  return await page.evaluate(async ([base64, width, height]) => {
    const bytes = Uint8Array.from(atob(base64), (character) => character.charCodeAt(0));
    const bitmap = await createImageBitmap(new Blob([bytes], { type: 'image/png' }));
    const context = new OffscreenCanvas(width, height).getContext('2d');
    if (context === null) throw new Error('Expected a 2D context.');
    context.drawImage(bitmap, 0, 0);
    return Array.from(context.getImageData(0, 0, width, height).data);
  }, [png.toString('base64'), area.width, area.height] as const);
}

/** Pixels brighter than the dark cards in a region: drawn text and strokes. */
async function lightPixels(page: Page, area: Bounds): Promise<number> {
  const data = await region(page, area);
  let count = 0;
  for (let index = 0; index < data.length; index += 4) {
    if ((data[index] ?? 0) + (data[index + 1] ?? 0) + (data[index + 2] ?? 0) > 450) count += 1;
  }
  return count;
}

test.describe('Immediate drawing route', () => {
  test('eight 300 px cards wrap across the dark page, as in the reference', async ({ page }) => {
    const errors = await openRoute(page);

    await expect.poll(async () => await page.title()).toBe('EffinDOM FUI-CS Demo • Immediate Drawing');
    const positions = (await cards(page)).map((card) => `${String(card.x)},${String(card.y)} ${String(card.width)}x${String(card.height)}`);
    expect(positions).toEqual([
      '24,24 300x300', '340,24 300x300', '656,24 300x300',
      '24,340 300x300', '340,340 300x300', '656,340 300x300',
      '24,656 300x300', '340,656 300x300',
    ]);
    expect((await hostNode(page, 'widget-yarn'))?.semanticLabel).toBe('Dancing yarn interactive noise panel');
    expect((await hostNode(page, 'widget-paint'))?.semanticLabel).toBe('Paint canvas — drag to draw');
    await expect.poll(async () => rgb((await region(page, { x: 8, y: 8, width: 1, height: 1 })).slice(0, 4) as unknown as readonly [number, number, number, number])).toBe(PAGE);
    expect(errors).toEqual([]);
  });

  test('the plots move on their own tick, and their titles and labels are drawn', async ({ page }) => {
    await openRoute(page);
    const [gauge, chart] = await cards(page);

    // The gauge's title, then its needle sweeping between two frames.
    await expect.poll(async () => await lightPixels(page, { x: gauge.x + 16, y: gauge.y + 20, width: 100, height: 24 })).toBeGreaterThan(20);
    const before = await region(page, gauge);
    await page.waitForTimeout(300);
    const after = await region(page, gauge);
    expect(after).not.toEqual(before);
    // The bar chart's live numeric labels sit on dark backings above the bars.
    await expect.poll(async () => await lightPixels(page, { x: chart.x, y: chart.y + 44, width: chart.width, height: chart.height - 44 })).toBeGreaterThan(20);
    await page.screenshot({ path: screenshotPath('fui-cs-immediate-drawing.png') });
  });

  test('the paint canvas shows its rich-text hint and paints where it is dragged', async ({ page }) => {
    await openRoute(page);
    const paint = await boundsOf(page, 'widget-paint');

    // The hint is rendered into the canvas's bitmap once its fonts are ready.
    await expect.poll(async () => await lightPixels(page, { x: paint.x + 16, y: paint.y + 32, width: 260, height: 90 })).toBeGreaterThan(100);

    const canvas = await page.locator('#fui-canvas').boundingBox();
    if (canvas === null) throw new Error('Expected the fui-cs canvas.');
    const y = canvas.y + paint.y + 240;
    await page.mouse.move(canvas.x + paint.x + 60, y);
    await page.mouse.down();
    await page.mouse.move(canvas.x + paint.x + 240, y, { steps: 12 });
    await page.mouse.up();

    await expect.poll(async () => rgb(await pixelAt(page, 'widget-paint', 150, 240))).toBe(NEEDLE);
    expect(rgb(await pixelAt(page, 'widget-paint', 150, 200))).not.toBe(NEEDLE);
  });
});
