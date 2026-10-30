import { expect, test } from '@playwright/test';

import { clickNode, hostNode, labelOf, openTestPage, serveApp } from './support.js';

const baseUrl = serveApp();

test.describe('Platform services in the host', () => {
  test('the browser host reports its environment, platform and capabilities', async ({ page }) => {
    const errors = await openTestPage(page, baseUrl(), 'platform');

    expect(await labelOf(page, 'environment-log')).toBe('browser');
    expect(await labelOf(page, 'family-log')).toBe(process.platform === 'darwin' ? 'apple' : process.platform === 'win32' ? 'windows' : 'linux');
    expect(await labelOf(page, 'history-log')).toBe('history');
    expect(await labelOf(page, 'undo-log')).toBe(process.platform === 'darwin' ? '⌘Z' : 'Ctrl+Z');
    expect(errors).toEqual([]);
  });

  test('BCL timers continue on the page and their UI changes reach the screen', async ({ page }) => {
    await openTestPage(page, baseUrl(), 'platform');

    await expect.poll(async () => await labelOf(page, 'timer-log')).toBe('ticked 3');
  });

  test('navigation reaches the host and the new route comes back to C#', async ({ page }) => {
    await openTestPage(page, baseUrl(), 'platform');
    expect(await labelOf(page, 'route-log')).toBe('/v2/fui-cs/index.html?page=platform');

    await clickNode(page, 'navigate');

    await expect.poll(async () => await labelOf(page, 'route-log')).toBe('/v2/fui-cs/index.html?page=platform#second');
    expect(page.url()).toContain('#second');
  });

  test('the harness debug API injects input through the debug exports', async ({ page }) => {
    await openTestPage(page, baseUrl(), 'platform');
    const target = await hostNode(page, 'debug-target');
    if (target === null) throw new Error('Expected the debug target.');

    await page.evaluate(async ({ handle, x, y }) => {
      const debug = window.__fui_debug;
      if (debug === undefined) throw new Error('The harness debug API is not installed.');
      await debug.pointerEvent(1, handle, x, y);
      await debug.pointerEvent(2, handle, x, y);
    }, { handle: target.handle, x: target.bounds.x + 5, y: target.bounds.y + 5 });

    await expect.poll(async () => await labelOf(page, 'debug-log')).toBe('clicked');
  });
});
