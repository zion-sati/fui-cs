import { expect, test, type Page } from '@playwright/test';

import { boundsOf, clickNode, labelOf, nextFrames, openTestPage, pixelIn, pointIn, rgb, screenshotPath, serveApp } from './support.js';

const baseUrl = serveApp();

const LIGHT_BACKGROUND = 0xf8fafc;
const DARK_BACKGROUND = 0x040a14;
const MARKER = 0xfde047;

async function expectLabel(page: Page, nodeId: string, expected: string | RegExp): Promise<void> {
  await expect.poll(async () => await labelOf(page, nodeId)).toMatch(expected);
}

/** Every pixel color along a horizontal line through the node, as 0xRRGGBB values. */
async function colorsAcross(page: Page, nodeId: string, fy = 0.5, samples = 40): Promise<Set<number>> {
  const colors = new Set<number>();
  for (let index = 0; index < samples; ++index) {
    colors.add(rgb(await pixelIn(page, nodeId, (index + 0.5) / samples, fy)));
  }
  return colors;
}

test.describe('Text in the host', () => {
  test('text renders with its content as the static-text label', async ({ page }) => {
    const errors = await openTestPage(page, baseUrl(), 'text');

    const bounds = await boundsOf(page, 'selectable');
    expect(bounds.width).toBeGreaterThan(150);
    expect(bounds.height).toBeGreaterThan(20);
    expect(await labelOf(page, 'selectable')).toBe('Select this sentence');
    expect(errors).toEqual([]);
    await page.screenshot({ path: screenshotPath('fui-cs-text.png') });
  });

  test('dragging across selectable text reports the selected range', async ({ page }) => {
    await openTestPage(page, baseUrl(), 'text');
    const start = await pointIn(page, 'selectable', 0.0, 0.5);
    const end = await pointIn(page, 'selectable', 0.5, 0.5);

    await page.mouse.move(start.x + 1, start.y);
    await nextFrames(page);
    await page.mouse.down();
    await page.mouse.move(end.x, end.y, { steps: 10 });
    await page.mouse.up();

    await expectLabel(page, 'selection-log', /^0-([6-9]|1[0-2])$/);
  });

  test('typing into editable text reports each change', async ({ page }) => {
    await openTestPage(page, baseUrl(), 'text');

    await clickNode(page, 'editor');
    await page.keyboard.type('abc');
    await expectLabel(page, 'edit-log', 'abc');
    await page.keyboard.press('Backspace');
    await expectLabel(page, 'edit-log', 'ab');
    await page.keyboard.press('Shift+KeyD');
    await expectLabel(page, 'edit-log', 'abD');
  });

  test('rich text paints a span background behind its run', async ({ page }) => {
    await openTestPage(page, baseUrl(), 'text');

    expect(await labelOf(page, 'rich')).toBe('plain marked red');
    const colors = await colorsAcross(page, 'rich');
    expect(colors).toContain(MARKER);
    // The unstyled "plain " run takes the theme's text color, so it shows dark ink on the light page.
    const plain = [...await colorsAcross(page, 'rich', 0.5, 40)].slice(0, 12);
    expect(plain.some((color) => ((color >> 16) & 0xff) + ((color >> 8) & 0xff) + (color & 0xff) < 300)).toBe(true);
  });

  test('a loaded font reaches C# and re-lays out the text that uses it', async ({ page }) => {
    await openTestPage(page, baseUrl(), 'text');

    await expectLabel(page, 'font-log', 'loaded');
    // Eight monospace i's are much wider than eight proportional ones.
    await expect.poll(async () => (await boundsOf(page, 'mono')).width).toBeGreaterThan((await boundsOf(page, 'proportional')).width * 1.5);
  });

  test('the system theme follows the host dark mode', async ({ page }) => {
    await page.emulateMedia({ colorScheme: 'light' });
    await openTestPage(page, baseUrl(), 'text');
    await expectLabel(page, 'theme-log', 'light');
    expect(rgb(await pixelIn(page, 'text-page', 0.98, 0.98))).toBe(LIGHT_BACKGROUND);

    await page.emulateMedia({ colorScheme: 'dark' });

    await expectLabel(page, 'theme-log', 'dark');
    await expect.poll(async () => rgb(await pixelIn(page, 'text-page', 0.98, 0.98))).toBe(DARK_BACKGROUND);
    await page.screenshot({ path: screenshotPath('fui-cs-text-dark.png') });
  });
});
