import { expect, test, type Page } from '@playwright/test';

import { clickNode, labelOf, nextFrames, openTestPage, pointIn, serveApp } from './support.js';

const baseUrl = serveApp();

async function scrollList(page: Page, deltaY: number): Promise<void> {
  const list = await pointIn(page, 'persist-list');
  await page.mouse.move(list.x, list.y);
  await nextFrames(page);
  await page.mouse.wheel(0, deltaY);
}

async function settledOffset(page: Page): Promise<number> {
  let previous = -1;
  await expect.poll(async () => {
    const current = Number(await labelOf(page, 'offset-log'));
    const settled = current === previous;
    previous = current;
    return settled;
  }, { intervals: [200] }).toBe(true);
  return previous;
}

test.describe('Persisted UI state in the host', () => {
  test('going back restores the persisted value and scroll offset of the history entry', async ({ page }) => {
    const errors = await openTestPage(page, baseUrl(), 'persistence');
    for (let click = 0; click < 3; ++click) {
      await clickNode(page, 'increment');
    }
    await expect.poll(async () => await labelOf(page, 'counter')).toBe('3');
    await scrollList(page, 300);
    const saved = await settledOffset(page);
    expect(saved).toBeGreaterThan(200);

    await page.evaluate(async () => {
      await window.__fui_debug?.navigateTo('/v2/fui-cs/index.html?page=persistence#next');
    });
    await clickNode(page, 'increment');
    await clickNode(page, 'increment');
    await scrollList(page, -1000);
    await expect.poll(async () => await labelOf(page, 'offset-log')).toBe('0');
    await expect.poll(async () => await labelOf(page, 'counter')).toBe('5');

    await page.goBack();

    await expect.poll(async () => await labelOf(page, 'counter')).toBe('3');
    await expect.poll(async () => Number(await labelOf(page, 'offset-log'))).toBe(saved);
    expect(errors).toEqual([]);
  });
});
