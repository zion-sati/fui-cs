import { expect, test, type Page } from '@playwright/test';

import { boundsOf, clickNode, openTestPage, screenshotPath, serveApp, type Bounds } from './support.js';

const baseUrl = serveApp();

async function inGrid(page: Page, childId: string, gridId = 'grid-main'): Promise<Bounds> {
  const child = await boundsOf(page, childId);
  const grid = await boundsOf(page, gridId);
  return { x: child.x - grid.x, y: child.y - grid.y, width: child.width, height: child.height };
}

test.describe('Grid layout in the host', () => {
  test('pixel and star tracks size the cells, spans cover tracks and children fill their cells', async ({ page }) => {
    const errors = await openTestPage(page, baseUrl(), 'grid');

    // Columns 100 | 1* | 2* over 400 px; rows 50 | * | 30 over 200 px.
    expect(await inGrid(page, 'g-a')).toEqual({ x: 0, y: 0, width: 100, height: 50 });
    expect(await inGrid(page, 'g-b')).toEqual({ x: 200, y: 0, width: 200, height: 50 });
    expect(await inGrid(page, 'g-span')).toEqual({ x: 0, y: 50, width: 200, height: 120 });
    expect(await inGrid(page, 'g-last')).toEqual({ x: 100, y: 170, width: 100, height: 30 });
    expect(errors).toEqual([]);
    await page.screenshot({ path: screenshotPath('fui-cs-grid.png') });
  });

  test('moving a built child to another column re-lays it out', async ({ page }) => {
    await openTestPage(page, baseUrl(), 'grid');

    await clickNode(page, 'move-button');

    await expect.poll(async () => (await inGrid(page, 'g-a')).x).toBe(100);
    expect((await inGrid(page, 'g-a')).width).toBe(100);
  });

  test('replacing the column tracks of a built grid re-lays every cell', async ({ page }) => {
    await openTestPage(page, baseUrl(), 'grid');

    await clickNode(page, 'tracks-button');

    // Columns become 200 | 1* | 1* over 400 px.
    await expect.poll(async () => await inGrid(page, 'g-b')).toEqual({ x: 300, y: 0, width: 100, height: 50 });
    expect(await inGrid(page, 'g-span')).toMatchObject({ x: 0, width: 300 });
  });

  test('auto tracks size to text content', async ({ page }) => {
    await openTestPage(page, baseUrl(), 'grid');

    const text = await inGrid(page, 'auto-text', 'auto-grid');
    const value = await inGrid(page, 'auto-text-value', 'auto-grid');
    expect(text.width).toBeGreaterThan(60);
    expect(text.height).toBeGreaterThan(16);
    expect(value).toMatchObject({ x: text.width, y: 0, height: text.height, width: 100 });
  });

  test('shared size groups give auto columns in one scope the widest content', async ({ page }) => {
    await openTestPage(page, baseUrl(), 'grid');

    const short = await inGrid(page, 's1-value', 's1-grid');
    const long = await inGrid(page, 's2-value', 's2-grid');
    const alone = await inGrid(page, 's3-value', 's3-grid');
    // "Name" alone needs a narrower column than "Email address"; in the shared group it gets the wider one.
    expect(alone.x).toBeGreaterThan(20);
    expect(long.x).toBeGreaterThan(alone.x + 20);
    expect(short.x).toBe(long.x);
    // Each grid sizes itself to its tracks: the 100 px value column and the 28 px row lie within it.
    for (const id of ['s1', 's2', 's3']) {
      const [gridBounds, value] = [await boundsOf(page, `${id}-grid`), await boundsOf(page, `${id}-value`)];
      expect(gridBounds.x + gridBounds.width).toBe(value.x + value.width);
      expect(gridBounds.height).toBe(28);
    }
  });

  test('auto tracks size to a child with an explicit width and height', async ({ page }) => {
    // As in WPF, a child's explicit Width/Height is its desired size, so an Auto track holds it even without content.
    await openTestPage(page, baseUrl(), 'grid');

    const sized = await inGrid(page, 'auto-sized', 'auto-grid');
    const value = await inGrid(page, 'auto-sized-value', 'auto-grid');
    expect(sized).toMatchObject({ width: 180, height: 30 });
    expect(value.x).toBe(180);
    expect(value.height).toBe(30);
  });
});
