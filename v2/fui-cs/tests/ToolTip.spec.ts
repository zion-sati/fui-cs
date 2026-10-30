import { expect, test, type Page } from '@playwright/test';

import { boundsOf, hostNode, labelOf, openTestPage, serveApp } from './support.js';

const baseUrl = serveApp();

async function toolTipShown(page: Page): Promise<boolean> {
  const node = await hostNode(page, 'fui-tooltip');
  return node !== null && node.visibleBounds.width > 0 && node.visibleBounds.height > 0;
}

interface HarnessDebugApi {
  pointerEvent(type: number, handle: string, x: number, y: number, modifiers?: number): Promise<void>;
  focusChanged(handle: string, focused: boolean): Promise<void>;
  keyEvent(type: number, key: string, modifiers?: number): Promise<void>;
  scroll(handle: string, offsetX: number, offsetY: number, contentWidth: number, contentHeight: number, viewportWidth: number, viewportHeight: number): Promise<void>;
}

const POINTER_ENTER = 4;
const POINTER_LEAVE = 5;

type DebugCall =
  | { readonly kind: 'pointer'; readonly type: number; readonly handle: string; readonly x: number; readonly y: number }
  | { readonly kind: 'focus'; readonly handle: string; readonly focused: boolean }
  | { readonly kind: 'key'; readonly key: string }
  | { readonly kind: 'scroll'; readonly handle: string; readonly offsetY: number };

/** Sends input through the harness debug API, which reaches the app through its debug exports. */
async function debugInput(page: Page, call: DebugCall): Promise<void> {
  await page.evaluate(async (input) => {
    const debug = (window as Window & { __fui_debug?: HarnessDebugApi }).__fui_debug;
    if (debug === undefined) throw new Error('The harness debug API is missing.');
    if (input.kind === 'pointer') await debug.pointerEvent(input.type, input.handle, input.x, input.y);
    else if (input.kind === 'focus') await debug.focusChanged(input.handle, input.focused);
    else if (input.kind === 'key') await debug.keyEvent(1, input.key);
    else await debug.scroll(input.handle, 0, input.offsetY, 200, 300, 200, 96);
  }, call);
}

test.describe('Tooltips through the harness debug hooks', () => {
  test('the harness debug API drives tooltips through the app\'s debug pointer, focus, key and scroll exports', async ({ page }) => {
    const errors = await openTestPage(page, baseUrl(), 'tooltip');
    await page.mouse.move(5, 5);
    const press = await hostNode(page, 'tip-button');
    const line = await hostNode(page, 'tip-line-1');
    const box = await hostNode(page, 'tip-scroll');
    if (press === null || line === null || box === null) throw new Error('Expected the tooltip owners and the scroll box.');
    const center = (node: typeof press): { x: number; y: number } =>
      ({ x: node.bounds.x + (node.bounds.width / 2), y: node.bounds.y + (node.bounds.height / 2) });

    await debugInput(page, { kind: 'pointer', type: POINTER_ENTER, handle: press.handle, ...center(press) });
    await expect.poll(async () => await toolTipShown(page)).toBe(true);
    expect(await labelOf(page, 'fui-tooltip-text')).toBe('Save the file');
    await debugInput(page, { kind: 'pointer', type: POINTER_LEAVE, handle: press.handle, ...center(press) });
    await expect.poll(async () => await toolTipShown(page)).toBe(false);

    // A key makes focus visible, so focusing the button shows its tooltip above it.
    await debugInput(page, { kind: 'key', key: 'Tab' });
    await debugInput(page, { kind: 'focus', handle: press.handle, focused: true });
    await expect.poll(async () => await toolTipShown(page)).toBe(true);
    expect((await boundsOf(page, 'fui-tooltip')).y).toBeLessThan(press.bounds.y);
    await debugInput(page, { kind: 'focus', handle: press.handle, focused: false });
    await expect.poll(async () => await toolTipShown(page), { timeout: 1_500 }).toBe(false);

    await debugInput(page, { kind: 'pointer', type: POINTER_ENTER, handle: line.handle, ...center(line) });
    await expect.poll(async () => await labelOf(page, 'fui-tooltip-text')).toBe('The first line');
    await expect.poll(async () => await toolTipShown(page)).toBe(true);
    await debugInput(page, { kind: 'scroll', handle: box.handle, offsetY: 24 });
    await expect.poll(async () => await toolTipShown(page), { timeout: 1_500 }).toBe(false);
    expect(errors).toEqual([]);
  });

});
