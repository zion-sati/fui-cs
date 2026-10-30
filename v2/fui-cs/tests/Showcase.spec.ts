import { expect, test, type Page } from '@playwright/test';

import { clickNode, labelOf, nextFrames, openTestPage, pixelAt, pixelIn, pointIn, rgb, screenshotPath, serveApp } from './support.js';

const baseUrl = serveApp();

const LIGHT_BACKGROUND = 0xf8fafc;
const DARK_BACKGROUND = 0x040a14;
const GAUGE_FILL = 0x16a34a;

type DebugWindow = Window & {
  readonly __fui_debug?: { getDebugTree(): Promise<{ nodes: readonly { semanticLabel: string; bounds: { y: number } }[] }> };
};

async function rowTop(page: Page, label: string): Promise<number | null> {
  return await page.evaluate(async (text) => {
    const tree = await (window as DebugWindow).__fui_debug?.getDebugTree();
    return tree?.nodes.find((node) => node.semanticLabel === text)?.bounds.y ?? null;
  }, label);
}

test.describe('The Stage 2 route', () => {
  test('boots with its text, rich text and ready assets', async ({ page }) => {
    await page.emulateMedia({ colorScheme: 'light' });
    const errors = await openTestPage(page, baseUrl(), 'showcase');

    expect(await labelOf(page, 'title')).toBe('Stage 2 in C#');
    expect(await labelOf(page, 'intro')).toBe('Retained nodes, rich text, images, scrolling and custom drawing');
    await expect.poll(async () => await labelOf(page, 'asset-state')).toBe('Image and SVG ready');
    expect(errors).toEqual([]);
    await page.screenshot({ path: screenshotPath('fui-cs-showcase.png') });
  });

  test('the gauge redraws, the list scrolls and the theme switch repaints the page', async ({ page }) => {
    await page.emulateMedia({ colorScheme: 'light' });
    await openTestPage(page, baseUrl(), 'showcase');

    // The arc's top is outside the 25% fill until the gauge is raised to 75%.
    expect(rgb(await pixelAt(page, 'gauge', 80, 20))).not.toBe(GAUGE_FILL);
    await clickNode(page, 'bump');
    await clickNode(page, 'bump');
    await expect.poll(async () => rgb(await pixelAt(page, 'gauge', 80, 20))).toBe(GAUGE_FILL);

    const before = await rowTop(page, 'Row 1');
    const list = await pointIn(page, 'showcase-list');
    await page.mouse.move(list.x, list.y);
    await nextFrames(page);
    await page.mouse.wheel(0, 120);
    await expect.poll(async () => await rowTop(page, 'Row 1')).toBeLessThan((before ?? 0) - 50);

    expect(rgb(await pixelIn(page, 'showcase-card', 0.98, 0.5))).toBe(LIGHT_BACKGROUND);
    await clickNode(page, 'theme-toggle');
    await expect.poll(async () => rgb(await pixelIn(page, 'showcase-card', 0.98, 0.5))).toBe(DARK_BACKGROUND);
    await page.screenshot({ path: screenshotPath('fui-cs-showcase-dark.png') });
  });
});
