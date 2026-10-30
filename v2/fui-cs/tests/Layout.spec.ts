import { expect, test, type Page } from '@playwright/test';

import { boundsOf, clickNode, hostNode, openTestPage, pixelAt, pixelIn, rgb, screenshotPath, serveApp, type Bounds } from './support.js';

const baseUrl = serveApp();

async function relative(page: Page, childId: string, parentId: string): Promise<Bounds> {
  const child = await boundsOf(page, childId);
  const parent = await boundsOf(page, parentId);
  return { x: child.x - parent.x, y: child.y - parent.y, width: child.width, height: child.height };
}

test.describe('FlexBox layout in the host', () => {
  test('fixed sizes and margins place a row left to right', async ({ page }) => {
    const errors = await openTestPage(page, baseUrl(), 'layout');

    expect(await relative(page, 'fixed-a', 'fixed-row')).toEqual({ x: 0, y: 0, width: 100, height: 30 });
    expect(await relative(page, 'fixed-b', 'fixed-row')).toEqual({ x: 110, y: 0, width: 60, height: 30 });
    expect(await relative(page, 'fixed-c', 'fixed-row')).toEqual({ x: 170, y: 0, width: 80, height: 30 });
    expect(errors).toEqual([]);
    await page.screenshot({ path: screenshotPath('fui-cs-layout.png') });
  });

  test('fill and percent children share the remaining main axis', async ({ page }) => {
    await openTestPage(page, baseUrl(), 'layout');

    expect(await relative(page, 'fill', 'fill-row')).toEqual({ x: 100, y: 0, width: 250, height: 30 });
    expect(await relative(page, 'percent-25', 'percent-row')).toMatchObject({ x: 0, width: 100 });
    expect(await relative(page, 'percent-75', 'percent-row')).toMatchObject({ x: 100, width: 300 });
  });

  test('justify and align position children on both axes', async ({ page }) => {
    await openTestPage(page, baseUrl(), 'layout');

    expect(await relative(page, 'centered', 'center-row')).toMatchObject({ x: 150, width: 100 });
    expect(await relative(page, 'end-aligned', 'end-column')).toMatchObject({ x: 150, width: 50 });
  });

  test('min and max sizes clamp the requested size', async ({ page }) => {
    await openTestPage(page, baseUrl(), 'layout');

    expect((await boundsOf(page, 'min-clamped')).width).toBe(80);
    expect((await boundsOf(page, 'max-clamped')).width).toBe(120);
  });

  test('wrapping moves overflowing children to the next line', async ({ page }) => {
    await openTestPage(page, baseUrl(), 'layout');

    const first = await relative(page, 'wrap-1', 'wrap-row');
    const third = await relative(page, 'wrap-3', 'wrap-row');
    const fourth = await relative(page, 'wrap-4', 'wrap-row');
    expect(first).toMatchObject({ x: 0, y: 0 });
    expect(third).toMatchObject({ x: 120, y: 0 });
    expect(fourth).toMatchObject({ x: 0, y: 20 });
    expect((await boundsOf(page, 'wrap-row')).height).toBe(40);
  });

  test('collapsed children take no space', async ({ page }) => {
    await openTestPage(page, baseUrl(), 'layout');

    expect(await relative(page, 'collapse-c', 'collapse-row')).toMatchObject({ x: 50 });
  });

  test('absolute children use their offsets, clipping limits what is visible, padding insets content', async ({ page }) => {
    await openTestPage(page, baseUrl(), 'layout');

    expect(await relative(page, 'absolute-child', 'absolute-host')).toMatchObject({ x: 30, y: 25, width: 20, height: 20 });
    const clipped = await hostNode(page, 'clipped');
    expect(clipped?.bounds.width).toBe(100);
    expect(clipped?.visibleBounds.width).toBe(60);
    expect(clipped?.visibleBounds.height).toBe(60);
    expect(await relative(page, 'padded-child', 'padded')).toEqual({ x: 12, y: 6, width: 30, height: 30 });
    expect(await boundsOf(page, 'padded')).toMatchObject({ width: 54, height: 42 });
    // Children without a height stretch to the row's cross size, set by its 60 px tall siblings.
    expect((await boundsOf(page, 'stretched')).height).toBe(60);
  });

  test('background, border and corner radius are painted', async ({ page }) => {
    await openTestPage(page, baseUrl(), 'layout');

    expect(rgb(await pixelIn(page, 'styled', 0.5, 0.5))).toBe(0xe11d48);
    expect(rgb(await pixelIn(page, 'styled', 3 / 80, 0.5))).toBe(0x2563eb);
    // The rounded corner leaves the page background showing at the box's very corner.
    expect(rgb(await pixelIn(page, 'styled', 0, 0))).not.toBe(0x2563eb);
  });

  test('opacity fades a node together with its children, once', async ({ page }) => {
    await openTestPage(page, baseUrl(), 'layout');
    // The faded box is 40 px square at half opacity: a blue child in its top-left quarter over its own grey.
    const background = await pixelAt(page, 'faded', 50, 20);
    const halfway = (color: number, pixel: readonly [number, number, number, number]): void => {
      [(color >> 16) & 0xff, (color >> 8) & 0xff, color & 0xff].forEach((channel, index) => {
        expect(Math.abs(pixel[index] - ((channel + background[index]) / 2))).toBeLessThanOrEqual(8);
      });
    };

    halfway(0x2563eb, await pixelIn(page, 'faded-child'));
    halfway(0xcbd5e1, await pixelAt(page, 'faded', 32, 32));
  });

  test('a click handler resizes a built box and the row reflows', async ({ page }) => {
    await openTestPage(page, baseUrl(), 'layout');
    const before = await relative(page, 'toggle', 'toggle-row');

    await clickNode(page, 'toggle');

    await expect.poll(async () => (await boundsOf(page, 'toggle-target')).width).toBe(200);
    expect((await relative(page, 'toggle', 'toggle-row')).x).toBe(before.x + 100);
  });
});
