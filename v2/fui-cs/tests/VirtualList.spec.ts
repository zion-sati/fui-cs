import { expect, test, type Page } from '@playwright/test';

import { debugNodes, labelOf, openTestPage, pointIn, screenshotPath, serveApp } from './support.js';

const baseUrl = serveApp();

/** The item labels the list's rows show, top to bottom. */
async function shownItems(page: Page): Promise<string[]> {
  const rows = (await debugNodes(page)).filter((node) => /^Item \d+$/.test(node.semanticLabel) && node.bounds.height > 0);
  return [...rows].sort((a, b) => a.bounds.y - b.bounds.y).map((node) => node.semanticLabel);
}

async function itemTextCount(page: Page): Promise<number> {
  return (await debugNodes(page)).filter((node) => node.nodeTypeName.toLowerCase().includes('text') && /^(Item \d+)?$/.test(node.semanticLabel)).length;
}

test.describe('VirtualList in the host', () => {
  test('only the rows in view are bound, and scrolling rebinds the same rows to later items', async ({ page }) => {
    const errors = await openTestPage(page, baseUrl(), 'virtual-list');
    await expect.poll(async () => (await shownItems(page))[0]).toBe('Item 0');
    const first = await shownItems(page);
    expect(first.length).toBeGreaterThanOrEqual(10);
    expect(first.length).toBeLessThanOrEqual(12);
    const texts = await itemTextCount(page);

    const point = await pointIn(page, 'vlist');
    await page.mouse.move(point.x, point.y);
    for (let step = 0; step < 16; ++step) {
      await page.mouse.wheel(0, 400);
    }

    // The wheel scrolls smoothly, so the window and its rows are read together until they agree.
    await expect.poll(async () => {
      const firstIndex = Number((await labelOf(page, 'vlist-first'))?.split(' ')[1]);
      return firstIndex > 100 && (await shownItems(page)).includes(`Item ${String(firstIndex + 2)}`);
    }).toBe(true);
    // No rows were created to show them: the pool is the same size.
    expect(await itemTextCount(page)).toBe(texts);
    expect((await shownItems(page)).includes('Item 0')).toBe(false);
    await page.screenshot({ path: screenshotPath('fui-cs-virtual-list.png') });
    expect(errors).toEqual([]);
  });
});
