import { expect, test, type Page } from '@playwright/test';

import { boundsOf, clickNode, hostNode, labelOf, openTestPage, pixelAt, pixelIn, pointIn, rgb, screenshotPath, serveApp } from './support.js';

const baseUrl = serveApp();

const RED = 0xe11d48;
const BLUE = 0x2563eb;
const GREEN = 0x16a34a;

async function expectLabel(page: Page, nodeId: string, text: string): Promise<void> {
  await expect.poll(async () => await labelOf(page, nodeId)).toBe(text);
}

/** A point inside the button's left padding, clear of its border and label. */
async function chrome(page: Page, nodeId: string): Promise<number> {
  return rgb(await pixelIn(page, nodeId, 0.06, 0.5));
}

test.describe('Button in the host', () => {
  test('hovering shows the hover background and a click raises Click', async ({ page }) => {
    const errors = await openTestPage(page, baseUrl(), 'controls');
    expect((await hostNode(page, 'save'))?.semanticLabel).toBe('Save');
    const rest = await themeAccent(page);

    const point = await pointIn(page, 'save');
    await page.mouse.move(point.x, point.y);
    await expectLabel(page, 'hover-log', 'in');
    await expect.poll(async () => await chrome(page, 'save')).not.toBe(rest);

    await clickNode(page, 'save');
    await expectLabel(page, 'save-count', '1');
    await page.mouse.move(2, 2);
    await expectLabel(page, 'hover-log', 'out');
    await expect.poll(async () => await chrome(page, 'save')).toBe(rest);
    expect(errors).toEqual([]);
    await page.screenshot({ path: screenshotPath('fui-cs-controls.png') });
  });

  test('a press released outside the button does not click', async ({ page }) => {
    await openTestPage(page, baseUrl(), 'controls');
    const inside = await pointIn(page, 'save');

    await page.mouse.move(inside.x, inside.y);
    await page.mouse.down();
    await page.mouse.move(2, 2, { steps: 4 });
    await page.mouse.up();

    await expectLabel(page, 'hover-log', 'out');
    expect(await labelOf(page, 'save-count')).toBe('0');

    // Dragging back over the button before releasing still clicks.
    await page.mouse.move(inside.x, inside.y);
    await page.mouse.down();
    await page.mouse.move(2, 2, { steps: 4 });
    await page.mouse.move(inside.x, inside.y, { steps: 4 });
    await page.mouse.up();
    await expectLabel(page, 'save-count', '1');
  });

  test('Tab focuses the button and Enter and Space activate it', async ({ page }) => {
    await openTestPage(page, baseUrl(), 'controls');

    await page.keyboard.press('Tab');
    await page.keyboard.press('Enter');
    await expectLabel(page, 'save-count', '1');
    await page.keyboard.press('Space');
    await expectLabel(page, 'save-count', '2');
    await page.keyboard.press('Control+Enter');
    await page.keyboard.press('Enter');
    await expectLabel(page, 'save-count', '3');
  });

  test('keyboard focus draws the focus ring and a pointer press hides it', async ({ page }) => {
    await openTestPage(page, baseUrl(), 'controls');
    const save = await boundsOf(page, 'save');
    // A point on the ring: just outside the button's left edge, halfway down.
    const ringColor = async (): Promise<number> => rgb(await pixelAt(page, 'save', -1.5, save.height / 2));
    const page0 = await ringColor();

    await page.keyboard.press('Tab');
    await expect.poll(ringColor).not.toBe(page0);
    const accent = await themeAccent(page);
    expect(await ringColor()).toBe(accent);

    await clickNode(page, 'agree');
    await expect.poll(ringColor).toBe(page0);
  });

  test('a disabled button is faded and ignores clicks', async ({ page }) => {
    await openTestPage(page, baseUrl(), 'controls');
    await themeAccent(page);
    const enabled = await pixelIn(page, 'save', 0.06, 0.5);

    await clickNode(page, 'off');

    expect(await labelOf(page, 'off-count')).toBe('0');
    // At 38% opacity the accent blends toward the page, so it is lighter or darker than the enabled accent.
    const faded = await pixelIn(page, 'off', 0.06, 0.5);
    expect(rgb(faded)).not.toBe(rgb(enabled));
  });

  test('an overridden background and custom colors paint and follow hover', async ({ page }) => {
    await openTestPage(page, baseUrl(), 'controls');

    await expect.poll(async () => await chrome(page, 'custom')).toBe(RED);
    await expect.poll(async () => await chrome(page, 'colored')).toBe(GREEN);
    const point = await pointIn(page, 'colored');
    await page.mouse.move(point.x, point.y);
    await expect.poll(async () => await chrome(page, 'colored')).toBe(BLUE);
  });
});

/**
 * The Save button's resting accent: the theme accent the indicators fill with. The host paints its first frames
 * before its fonts and the scene settle (up to a second under load), so this waits until the button shows it.
 */
async function themeAccent(page: Page): Promise<number> {
  await expect.poll(async () => await chrome(page, 'save')).toBe(BLUE);
  return BLUE;
}

async function away(page: Page): Promise<void> {
  await page.mouse.move(2, 2);
}

function isLight(color: readonly [number, number, number, number]): boolean {
  return color[0] + color[1] + color[2] > 600;
}

test.describe('Checkbox, RadioButton and Switch in the host', () => {
  test('clicking a checkbox or its label toggles it, fills the box and draws the check mark', async ({ page }) => {
    const errors = await openTestPage(page, baseUrl(), 'controls');
    const accent = await themeAccent(page);
    const box = page.getByRole('checkbox', { name: 'Agree' });
    await expect(box).toHaveAttribute('aria-checked', 'false');

    await clickNode(page, 'agree-box');
    await expectLabel(page, 'agree-state', 'True');
    await away(page);
    await expect(box).toHaveAttribute('aria-checked', 'true');
    await expect.poll(async () => rgb(await pixelIn(page, 'agree-box', 0.2, 0.2))).toBe(accent);
    await expect.poll(async () => isLight(await pixelIn(page, 'agree-box', 0.5, 0.59))).toBe(true);

    // The whole control is the target: clicking the label text unchecks it.
    await clickNode(page, 'agree', 0.8, 0.5);
    await expectLabel(page, 'agree-state', 'False');
    await away(page);
    await expect.poll(async () => rgb(await pixelIn(page, 'agree-box', 0.2, 0.2))).not.toBe(accent);
    expect(errors).toEqual([]);
  });

  test('a three-state checkbox passes through indeterminate', async ({ page }) => {
    await openTestPage(page, baseUrl(), 'controls');
    const box = page.getByRole('checkbox', { name: 'Three states' });

    await clickNode(page, 'mixed');
    await expectLabel(page, 'mixed-state', 'True');
    await clickNode(page, 'mixed');
    await expectLabel(page, 'mixed-state', 'Mixed');
    await expect(box).toHaveAttribute('aria-checked', 'mixed');
    await clickNode(page, 'mixed');
    await expectLabel(page, 'mixed-state', 'False');
  });

  test('Space toggles the focused checkbox and switch', async ({ page }) => {
    await openTestPage(page, baseUrl(), 'controls');
    await clickNode(page, 'agree');
    await expectLabel(page, 'agree-state', 'True');

    await page.keyboard.press('Space');
    await expectLabel(page, 'agree-state', 'False');
    await page.keyboard.press('Enter');
    await clickNode(page, 'wifi');
    await expectLabel(page, 'wifi-state', 'on');
    await page.keyboard.press('Space');
    await expectLabel(page, 'wifi-state', 'off');
    expect(await labelOf(page, 'agree-state')).toBe('False');
  });

  test('a disabled checkbox ignores clicks and a press released outside does not toggle', async ({ page }) => {
    await openTestPage(page, baseUrl(), 'controls');
    await clickNode(page, 'locked');
    await expect(page.getByRole('checkbox', { name: 'Locked' })).toHaveAttribute('aria-checked', 'false');
    await expect(page.getByRole('checkbox', { name: 'Locked' })).toBeDisabled();

    const inside = await pointIn(page, 'agree');
    await page.mouse.move(inside.x, inside.y);
    await page.mouse.down();
    await page.mouse.move(2, 2, { steps: 4 });
    await page.mouse.up();
    await page.waitForTimeout(100);
    expect(await labelOf(page, 'agree-state')).toBe('False');
  });

  test('radios keep one selection, show the dot and move with the arrow keys past disabled ones', async ({ page }) => {
    await openTestPage(page, baseUrl(), 'controls');
    const accent = await themeAccent(page);

    await clickNode(page, 'size-s');
    await expectLabel(page, 'size-state', 's');
    await away(page);
    await expect.poll(async () => rgb(await pixelIn(page, 'size-s-ring'))).toBe(accent);
    expect(rgb(await pixelIn(page, 'size-l-ring'))).not.toBe(accent);
    await expect(page.getByRole('radio', { name: 'Small' })).toHaveAttribute('aria-checked', 'true');

    await page.keyboard.press('ArrowDown');
    await expectLabel(page, 'size-state', 'l');
    await expect.poll(async () => rgb(await pixelIn(page, 'size-l-ring'))).toBe(accent);
    expect(rgb(await pixelIn(page, 'size-s-ring'))).not.toBe(accent);
    await expect(page.getByRole('radio', { name: 'Small' })).toHaveAttribute('aria-checked', 'false');

    await clickNode(page, 'size-m');
    expect(await labelOf(page, 'size-state')).toBe('l');
  });

  test('a switch slides its thumb and fills the track when on', async ({ page }) => {
    await openTestPage(page, baseUrl(), 'controls');
    const accent = await themeAccent(page);

    await clickNode(page, 'wifi');
    await expectLabel(page, 'wifi-state', 'on');
    await away(page);
    await expect(page.getByRole('switch', { name: 'Wi-Fi' })).toHaveAttribute('aria-checked', 'true');
    await expect.poll(async () => rgb(await pixelIn(page, 'wifi-track', 0.2, 0.5))).toBe(accent);
    expect(isLight(await pixelIn(page, 'wifi-track', 0.7, 0.5))).toBe(true);
    await page.screenshot({ path: screenshotPath('fui-cs-labeled-controls.png') });
  });
});
