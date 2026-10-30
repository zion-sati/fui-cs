import { expect, test, type Page } from '@playwright/test';

import { clickNode, labelOf, openTestPage, pixelIn, pointIn, rgb, screenshotPath, serveApp } from './support.js';

const baseUrl = serveApp();

const BLUE = 0x2563eb;
const ORANGE = 0xf97316;
const GREEN = 0x16a34a;

async function expectLabel(page: Page, nodeId: string, expected: string | RegExp): Promise<void> {
  await expect.poll(async () => await labelOf(page, nodeId)).toMatch(expected);
}

/** Drags with the mouse from one node to another in small steps, as a person would. */
async function dragBetween(page: Page, fromId: string, toId: string): Promise<void> {
  const from = await pointIn(page, fromId);
  const to = await pointIn(page, toId);
  await page.mouse.move(from.x, from.y);
  await page.mouse.down();
  await page.mouse.move(from.x + 8, from.y, { steps: 4 });
  await page.mouse.move(to.x, to.y, { steps: 20 });
  await page.mouse.up();
}

test.describe('Input from the host reaches C# handlers', () => {
  test('clicks and double clicks are counted separately', async ({ page }) => {
    const errors = await openTestPage(page, baseUrl(), 'input');

    await clickNode(page, 'click-target');
    await expectLabel(page, 'click-log', '1/0');
    const point = await pointIn(page, 'click-target');
    await page.mouse.dblclick(point.x, point.y);

    await expectLabel(page, 'click-log', /^[23]\/1$/);
    expect(errors).toEqual([]);
    await page.screenshot({ path: screenshotPath('fui-cs-input.png') });
  });

  test('pointer enter and leave repaint the hovered box', async ({ page }) => {
    await openTestPage(page, baseUrl(), 'input');
    const inside = await pointIn(page, 'hover-target');

    await page.mouse.move(inside.x, inside.y);
    await expectLabel(page, 'hover-log', 'in');
    await page.mouse.move(inside.x + 10, inside.y + 10);
    expect(rgb(await pixelIn(page, 'hover-target', 0.1, 0.1))).toBe(ORANGE);

    await page.mouse.move(2, 2);
    await expectLabel(page, 'hover-log', 'out');
    expect(rgb(await pixelIn(page, 'hover-target', 0.1, 0.1))).toBe(BLUE);
  });

  test('a disabled node ignores clicks', async ({ page }) => {
    await openTestPage(page, baseUrl(), 'input');

    await clickNode(page, 'disabled-target');
    await clickNode(page, 'click-target');

    await expectLabel(page, 'click-log', '1/0');
    expect(await labelOf(page, 'disabled-log')).toBe('idle');
  });

  test('clicks bubble to ancestors unless a handler marks them handled', async ({ page }) => {
    await openTestPage(page, baseUrl(), 'input');

    await clickNode(page, 'bubble-child');
    await clickNode(page, 'stop-child');

    await expectLabel(page, 'bubble-log', 'child>parent');
    await expectLabel(page, 'stop-log', 'child');
  });

  test('the wheel reports its direction', async ({ page }) => {
    await openTestPage(page, baseUrl(), 'input');
    const point = await pointIn(page, 'wheel-target');
    await page.mouse.move(point.x, point.y);

    await page.mouse.wheel(0, 100);
    await expectLabel(page, 'wheel-log', 'down');
    await page.mouse.wheel(0, -100);
    await expectLabel(page, 'wheel-log', 'up');
  });

  test('a focusable node takes focus and receives keys', async ({ page }) => {
    await openTestPage(page, baseUrl(), 'input');

    await clickNode(page, 'key-target');
    await expectLabel(page, 'focus-log', 'focused');
    await page.keyboard.press('x');
    await expectLabel(page, 'key-log', 'x');
    await page.keyboard.press('ArrowDown');
    await expectLabel(page, 'key-log', 'ArrowDown');

    await clickNode(page, 'click-target');
    await expectLabel(page, 'focus-log', 'blurred');
  });

  test('a held touch is recognized as a long press', async ({ page }) => {
    // The host recognizes long presses for touch and pen; a held mouse button is not one.
    await openTestPage(page, baseUrl(), 'input');
    const point = await pointIn(page, 'long-press-target');
    const cdp = await page.context().newCDPSession(page);

    await cdp.send('Input.dispatchTouchEvent', { type: 'touchStart', touchPoints: [{ x: point.x, y: point.y }] });
    await expectLabel(page, 'long-press-log', 'pressed');
    await cdp.send('Input.dispatchTouchEvent', { type: 'touchEnd', touchPoints: [] });
  });

  test('a two-finger touch drag is a pan gesture', async ({ page }) => {
    // One finger scrolls; the host's control gestures (pan, pinch) take two.
    await openTestPage(page, baseUrl(), 'input');
    const start = await pointIn(page, 'pan-target', 0.25, 0.5);
    const cdp = await page.context().newCDPSession(page);
    const fingers = (dx: number): { x: number; y: number; id: number }[] => [
      { x: start.x + dx, y: start.y - 10, id: 1 },
      { x: start.x + dx, y: start.y + 10, id: 2 },
    ];

    await cdp.send('Input.dispatchTouchEvent', { type: 'touchStart', touchPoints: fingers(0) });
    for (let step = 1; step <= 10; ++step) {
      await cdp.send('Input.dispatchTouchEvent', { type: 'touchMove', touchPoints: fingers(step * 5) });
    }
    await cdp.send('Input.dispatchTouchEvent', { type: 'touchEnd', touchPoints: [] });

    await expectLabel(page, 'pan-log', 'right-ended');
  });

  test('two fingers spreading apart are a pinch gesture', async ({ page }) => {
    await openTestPage(page, baseUrl(), 'input');
    const center = await pointIn(page, 'pinch-target');
    const cdp = await page.context().newCDPSession(page);
    const fingers = (spread: number): { x: number; y: number; id: number }[] => [
      { x: center.x - 10 - spread, y: center.y, id: 1 },
      { x: center.x + 10 + spread, y: center.y, id: 2 },
    ];

    await cdp.send('Input.dispatchTouchEvent', { type: 'touchStart', touchPoints: fingers(0) });
    for (let step = 1; step <= 10; ++step) {
      await cdp.send('Input.dispatchTouchEvent', { type: 'touchMove', touchPoints: fingers(step * 4) });
    }
    await cdp.send('Input.dispatchTouchEvent', { type: 'touchEnd', touchPoints: [] });

    await expectLabel(page, 'pinch-log', 'grew-ended');
  });

  test('pulling down at the top of the page shows the host pull-to-refresh indicator', async ({ page }) => {
    await openTestPage(page, baseUrl(), 'input');
    const start = await pointIn(page, 'click-target');
    const cdp = await page.context().newCDPSession(page);
    const indicator = page.locator('#effindom-pull-to-refresh');

    await cdp.send('Input.dispatchTouchEvent', { type: 'touchStart', touchPoints: [{ x: start.x, y: start.y, id: 1 }] });
    for (let step = 1; step <= 12; ++step) {
      await cdp.send('Input.dispatchTouchEvent', { type: 'touchMove', touchPoints: [{ x: start.x, y: start.y + (step * 6), id: 1 }] });
    }
    // The pull is short of the reload distance, so releasing it springs back instead of reloading.
    await expect.poll(async () => await indicator.evaluate((element) => Number.parseFloat(getComputedStyle(element).opacity))).toBeGreaterThan(0);
    await cdp.send('Input.dispatchTouchEvent', { type: 'touchEnd', touchPoints: [] });
  });

  test('the cursor follows the node under the pointer', async ({ page }) => {
    await openTestPage(page, baseUrl(), 'input');
    const cursor = async (): Promise<string> =>
      await page.locator('#fui-canvas').evaluate((canvas) => getComputedStyle(canvas).cursor);

    const button = await pointIn(page, 'click-target');
    await page.mouse.move(button.x, button.y);
    await expect.poll(cursor).toBe('pointer');
    const text = await pointIn(page, 'click-log', 0.1, 0.5);
    await page.mouse.move(text.x, text.y);
    await expect.poll(cursor).toBe('text');
    await page.mouse.move(text.x + 300, text.y + 400);
    await expect.poll(cursor).toBe('default');
  });

  test('dragging the source onto the drop target delivers its data', async ({ page }) => {
    await openTestPage(page, baseUrl(), 'input');

    await dragBetween(page, 'drag-source', 'drop-target');

    await expectLabel(page, 'drop-log', 'payload');
    expect(rgb(await pixelIn(page, 'drop-target', 0.1, 0.1))).toBe(GREEN);
  });
});
