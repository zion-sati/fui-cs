import { expect, test, type Page } from '@playwright/test';

import { boundsOf, labelOf, labelStartingWith, nextFrames, pointIn, screenshotPath, serveApp, waitForReady } from './support.js';

const baseUrl = serveApp();

async function openWorkbench(page: Page): Promise<string[]> {
  const errors: string[] = [];
  page.on('pageerror', (error) => errors.push(error.message));
  page.on('console', (message) => {
    // The asset showcase loads two assets that do not exist on purpose.
    const expectedFailure = message.text().includes('assets/missing.') || message.location().url.includes('/assets/missing.');
    if (message.type() === 'error' && !expectedFailure) errors.push(message.text());
  });
  await page.setViewportSize({ width: 1024, height: 700 });
  await page.goto(`${baseUrl()}/v2/fui-cs/demo/workbench/`);
  await waitForReady(page);
  await expect.poll(async () => await labelOf(page, 'text-input-value')).toBe('Value: <empty>');
  await scrollIntoView(page, 'text-input-colored');
  return errors;
}

/** Wheels the page until the node sits inside the window. */
async function scrollIntoView(page: Page, nodeId: string): Promise<void> {
  const viewportHeight = page.viewportSize()?.height ?? 700;
  await page.mouse.move(980, 300);
  await expect.poll(async () => {
    const bounds = await boundsOf(page, nodeId);
    if (bounds.y + bounds.height <= viewportHeight - 16) return true;
    await page.mouse.wheel(0, 240);
    return false;
  }, { timeout: 10_000 }).toBe(true);
  await expect.poll(async () => {
    const before = (await boundsOf(page, nodeId)).y;
    await nextFrames(page);
    return (await boundsOf(page, nodeId)).y === before;
  }).toBe(true);
}

/** Clicks inside a text input, `fx` of the way across it. */
async function clickInput(page: Page, nodeId: string, fx = 0.5, clickCount = 1): Promise<void> {
  const point = await pointIn(page, nodeId, fx);
  await page.mouse.move(point.x, point.y);
  await nextFrames(page);
  await page.mouse.click(point.x, point.y, { clickCount });
}

const value = async (page: Page): Promise<string | null> => await labelOf(page, 'text-input-value');
const selection = async (page: Page): Promise<string | null> => await labelOf(page, 'text-input-selection');

test.describe('TextInput in the host', () => {
  test('typing, deleting and caret keys edit the value, counted in characters and UTF-8 bytes', async ({ page }) => {
    const errors = await openWorkbench(page);
    await clickInput(page, 'text-input-plain');
    await expect.poll(async () => await labelOf(page, 'text-input-focus')).toBe('Focus: focused');

    await page.keyboard.type('héllo😀');
    await expect.poll(async () => await value(page)).toBe('Value: "héllo😀"');
    // Six characters, ten UTF-8 bytes.
    await expect.poll(async () => await selection(page)).toBe('Selection: 6-6 (bytes 10-10)');

    await page.keyboard.press('ArrowLeft');
    await page.keyboard.press('Backspace');
    await expect.poll(async () => await value(page)).toBe('Value: "héll😀"');
    await page.keyboard.press('Home');
    await page.keyboard.press('Delete');
    await expect.poll(async () => await value(page)).toBe('Value: "éll😀"');
    await page.keyboard.press('End');
    await page.keyboard.press('Shift+ArrowLeft');
    await expect.poll(async () => await selection(page)).toBe('Selection: 4-3 (bytes 8-4)');
    await page.keyboard.type('!');
    await expect.poll(async () => await value(page)).toBe('Value: "éll!"');
    await page.screenshot({ path: screenshotPath('fui-cs-text-input.png') });
    expect(errors).toEqual([]);
  });

  test('pasting inserts the clipboard text, and Select All then Backspace clears it', async ({ page, context }) => {
    await context.grantPermissions(['clipboard-read', 'clipboard-write']);
    const errors = await openWorkbench(page);
    await clickInput(page, 'text-input-plain');
    await page.evaluate(async () => { await navigator.clipboard.writeText('pasted 文字'); });

    await page.keyboard.press('Control+V');
    await expect.poll(async () => await value(page)).toBe('Value: "pasted 文字"');
    await page.keyboard.press('Control+A');
    await expect.poll(async () => await selection(page)).toBe('Selection: 0-9 (bytes 0-13)');
    await page.keyboard.press('Backspace');
    await expect.poll(async () => await value(page)).toBe('Value: <empty>');
    expect(errors).toEqual([]);
  });

  test('a click places the caret and a drag selects, and typing replaces the selection', async ({ page }) => {
    const errors = await openWorkbench(page);
    await clickInput(page, 'text-input-plain');
    await page.keyboard.type('alpha beta');
    await expect.poll(async () => await value(page)).toBe('Value: "alpha beta"');

    // A click near the field's left edge puts the caret before the first character.
    const field = await boundsOf(page, 'text-input-plain');
    const canvas = await page.locator('#fui-canvas').boundingBox();
    const left = (canvas?.x ?? 0) + field.x + 17;
    const middle = (canvas?.y ?? 0) + field.y + (field.height / 2);
    await page.mouse.click(left, middle);
    await expect.poll(async () => await selection(page)).toBe('Selection: 0-0 (bytes 0-0)');

    // Drag from the start to past the end, then type over the selection.
    await page.mouse.move(left, middle);
    await page.mouse.down();
    await page.mouse.move(left + 60, middle, { steps: 4 });
    await page.mouse.move(left + 200, middle, { steps: 4 });
    await page.mouse.up();
    await expect.poll(async () => await selection(page)).toBe('Selection: 0-10 (bytes 0-10)');
    await page.keyboard.type('x');
    await expect.poll(async () => await value(page)).toBe('Value: "x"');
    expect(errors).toEqual([]);
  });

  // The UI runtime stores a text node's character limit but never applies it to typed text (in every SDK), so the
  // limit only holds for text the control inserts itself. Open host finding.
  test.fixme('the limit stops typing at 24 characters', async ({ page }) => {
    const errors = await openWorkbench(page);
    await clickInput(page, 'text-input-plain');

    await page.keyboard.type('abcdefghijklmnopqrstuvwxyz0123');
    await expect.poll(async () => await value(page)).toBe('Value: "abcdefghijklmnopqrstuvwx"');
    expect(errors).toEqual([]);
  });

  test('Tab moves focus between inputs and back to the caret where it was', async ({ page }) => {
    const errors = await openWorkbench(page);
    await clickInput(page, 'text-input-plain');
    await page.keyboard.type('abc');
    await page.keyboard.press('ArrowLeft');

    await page.keyboard.press('Tab');
    await expect.poll(async () => await labelOf(page, 'text-input-focus')).toBe('Focus: blurred');
    await page.keyboard.press('Shift+Tab');
    await expect.poll(async () => await labelOf(page, 'text-input-focus')).toBe('Focus: focused');
    await page.keyboard.type('X');
    await expect.poll(async () => await value(page)).toBe('Value: "abXc"');
    expect(errors).toEqual([]);
  });

  test('a password input is obscured, tells the browser it is a password, and blocks copying', async ({ page, context }) => {
    await context.grantPermissions(['clipboard-read', 'clipboard-write']);
    const errors = await openWorkbench(page);
    await page.evaluate(async () => { await navigator.clipboard.writeText('before'); });

    await clickInput(page, 'text-input-password', 0.2, 2);
    await expect.poll(async () => await page.evaluate(() => {
      const editor = document.querySelector('input[data-effindom-hidden-editor]');
      return editor instanceof HTMLInputElement ? `${editor.type} ${editor.autocomplete}` : null;
    })).toBe('password current-password');
    await page.keyboard.press('Control+A');
    await page.keyboard.press('Control+C');
    await nextFrames(page);
    expect(await page.evaluate(async () => await navigator.clipboard.readText())).toBe('before');
    expect(errors).toEqual([]);
  });

  test('a read-only input selects but does not change, and a disabled one ignores input', async ({ page }) => {
    const errors = await openWorkbench(page);
    await clickInput(page, 'text-input-plain');
    await page.keyboard.type('kept');

    await clickInput(page, 'text-input-readonly');
    await expect.poll(async () => await labelOf(page, 'text-input-focus')).toBe('Focus: blurred');
    await page.keyboard.type('zz');
    await page.keyboard.press('Backspace');
    await clickInput(page, 'text-input-disabled');
    await page.keyboard.type('yy');
    await nextFrames(page);
    expect(await value(page)).toBe('Value: "kept"');

    // Back in the first input, typing continues where it left off.
    await clickInput(page, 'text-input-plain', 0.9);
    await page.keyboard.type('!');
    await expect.poll(async () => await value(page)).toBe('Value: "kept!"');
    expect(errors).toEqual([]);
  });
});

test.describe('TextInput on the dashboard', () => {
  test('typing updates the dashboard\'s text input status line', async ({ page }) => {
    const errors: string[] = [];
    page.on('pageerror', (error) => errors.push(error.message));
    await page.setViewportSize({ width: 1280, height: 900 });
    await page.goto(`${baseUrl()}/v2/fui-cs/demo/`);
    await waitForReady(page);
    await expect.poll(async () => await labelStartingWith(page, 'TextInput: ')).toBe('TextInput: <empty> • Selection: 0-0 • Focus: blurred');
    await scrollIntoView(page, 'demo-dashboard:text-input');

    await clickInput(page, 'demo-dashboard:text-input');
    await page.keyboard.type('hi');
    await expect.poll(async () => await labelStartingWith(page, 'TextInput: ')).toBe('TextInput: "hi" • Selection: 2-2 • Focus: focused');
    expect(errors).toEqual([]);
  });
});
