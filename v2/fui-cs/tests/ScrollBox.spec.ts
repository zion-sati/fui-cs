import { expect, test, type Page } from '@playwright/test';

import { boundsOf, clickNode, labelOf, openTestPage, pointIn, screenshotPath, serveApp } from './support.js';

const baseUrl = serveApp();

/** A readout's offsets as [x, y]. */
async function offsets(page: Page, nodeId: string): Promise<[number, number]> {
  const label = (await labelOf(page, nodeId)) ?? '0,0';
  const [x = 0, y = 0] = label.replace('page ', '').split(',').map(Number);
  return [x, y];
}

async function wheelOver(page: Page, nodeId: string, deltaX: number, deltaY: number, times = 1, fy = 0.5): Promise<void> {
  const point = await pointIn(page, nodeId, 0.4, fy);
  await page.mouse.move(point.x, point.y);
  for (let index = 0; index < times; ++index) {
    await page.mouse.wheel(deltaX, deltaY);
  }
}

test.describe('ScrollBox in the host', () => {
  test('the wheel scrolls the list to its bottom and the thumb follows', async ({ page }) => {
    const errors = await openTestPage(page, baseUrl(), 'scrollbox');
    const startList = await boundsOf(page, 'list-box');
    const thumbTop = (await boundsOf(page, 'list-thumb')).y - startList.y;

    await wheelOver(page, 'list-box', 0, 400, 3);

    // Wheel input past the list's end chains to the page box, so positions are compared within the list.
    await expect.poll(async () => (await offsets(page, 'list-log'))[1]).toBe(1000);
    const list = await boundsOf(page, 'list-box');
    const last = await boundsOf(page, 'row-40');
    expect(last.y + last.height).toBeLessThanOrEqual(list.y + list.height + 1);
    const thumb = await boundsOf(page, 'list-thumb');
    expect(thumb.y - list.y).toBeGreaterThan(thumbTop + 100);
    expect(thumb.y + thumb.height).toBeGreaterThan(list.y + list.height - 10);
    await page.screenshot({ path: screenshotPath('fui-cs-scrollbox.png') });
    expect(errors).toEqual([]);
  });

  test('the wheel scrolls a horizontal strip to its right edge', async ({ page }) => {
    await openTestPage(page, baseUrl(), 'scrollbox');

    await wheelOver(page, 'wide-box', 400, 0, 4);

    await expect.poll(async () => (await offsets(page, 'wide-log'))[0]).toBe(700);
    const box = await boundsOf(page, 'wide-box');
    const thumb = await boundsOf(page, 'wide-thumb');
    expect(thumb.x + thumb.width).toBeGreaterThan(box.x + box.width - 4);
  });

  test('dragging the thumb scrolls in proportion and keeps going outside the track', async ({ page }) => {
    await openTestPage(page, baseUrl(), 'scrollbox');
    const thumb = await pointIn(page, 'list-thumb');
    const thumbBefore = await boundsOf(page, 'list-thumb');

    await page.mouse.move(thumb.x, thumb.y);
    await page.mouse.down();
    await page.mouse.move(thumb.x, thumb.y + 40, { steps: 4 });
    // The thumb is 200 * 200 / 1200 = 33.3 px long and travels 166.7 px over 1000 px of offset.
    await expect.poll(async () => (await offsets(page, 'list-log'))[1]).toBe(240);
    expect((await boundsOf(page, 'list-thumb')).y).toBeCloseTo(thumbBefore.y + 40, 0);

    await page.mouse.move(thumb.x + 300, thumb.y + 80, { steps: 4 });
    await expect.poll(async () => (await offsets(page, 'list-log'))[1]).toBe(480);
    await page.mouse.up();
  });

  test('outside the list the wheel scrolls the page box it is nested in', async ({ page }) => {
    await openTestPage(page, baseUrl(), 'scrollbox');

    await wheelOver(page, 'page-filler', 0, 200, 1, 0.05);

    await expect.poll(async () => (await offsets(page, 'page-log'))[1]).toBeGreaterThan(0);
    expect((await offsets(page, 'list-log'))[1]).toBe(0);
  });

  test('after a click in the list, the keyboard scrolls it', async ({ page }) => {
    await openTestPage(page, baseUrl(), 'scrollbox');

    await clickNode(page, 'row-2');
    await page.keyboard.press('PageDown');
    await expect.poll(async () => (await offsets(page, 'list-log'))[1]).toBe(160);
    await page.keyboard.press('End');
    await expect.poll(async () => (await offsets(page, 'list-log'))[1]).toBe(1000);
    await page.keyboard.press('Home');
    await expect.poll(async () => (await offsets(page, 'list-log'))[1]).toBe(0);
  });

  test('a drag across the selection area selects both lines but not the barrier', async ({ page }) => {
    await openTestPage(page, baseUrl(), 'scrollbox');
    const start = await pointIn(page, 'first-line', 0.02, 0.5);
    const end = await pointIn(page, 'shielded-line', 0.9, 0.5);

    await page.mouse.move(start.x, start.y);
    await page.mouse.down();
    await page.mouse.move(end.x, end.y, { steps: 8 });
    await page.mouse.up();

    // One selection across both text nodes, without the barrier's text.
    await expect.poll(async () => await labelOf(page, 'selected-log')).toContain('selectable line|Second selectable line');
    expect(await labelOf(page, 'selected-log')).not.toContain('Outside');
  });
});
