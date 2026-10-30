import { expect, test, type Page } from '@playwright/test';

import { boundsOf, boundsOfLabel, labelStartingWith, openTestPage, pixelAt, pixelIn, rgb, screenshotPath, serveApp } from './support.js';

const baseUrl = serveApp();

/** The page coordinate of a point inside the labelled node. */
async function pointInLabel(page: Page, label: string, fx = 0.5, fy = 0.5): Promise<{ x: number; y: number }> {
  const bounds = await boundsOfLabel(page, label);
  const canvas = await page.locator('#fui-canvas').boundingBox();
  if (bounds === null || canvas === null) throw new Error(`No node is labelled "${label}".`);
  return { x: canvas.x + bounds.x + (bounds.width * fx), y: canvas.y + bounds.y + (bounds.height * fy) };
}

/** Wheels the page until the node's top rests between 80 and 200 px into the canvas, so what follows it shows too. */
async function scrollToTop(page: Page, nodeId: string): Promise<void> {
  const canvas = await page.locator('#fui-canvas').boundingBox();
  if (canvas === null) throw new Error('Expected the fui-cs canvas.');
  for (let attempt = 0; attempt < 30; attempt += 1) {
    const bounds = await boundsOf(page, nodeId);
    if (bounds.y >= 80 && bounds.y <= 200) {
      // The page scrolls smoothly: it is in place once the node stops moving.
      await page.waitForTimeout(150);
      if ((await boundsOf(page, nodeId)).y === bounds.y) return;
      continue;
    }
    await page.mouse.move(canvas.x + (canvas.width / 2), canvas.y + 40);
    await page.mouse.wheel(0, Math.max(-600, Math.min(600, bounds.y - 140)));
    await page.waitForTimeout(200);
  }
  throw new Error(`Unable to scroll ${nodeId} into place.`);
}

test.describe('Demo design system', () => {
  test('a route page shows its navigation, badge, title, status and sections in the demo theme', async ({ page }) => {
    const errors = await openTestPage(page, baseUrl(), 'design-system');

    await expect.poll(async () => await boundsOfLabel(page, 'Design system')).not.toBeNull();
    expect(await labelStartingWith(page, 'Current route: /v2/fui-cs/')).not.toBeNull();
    expect(await labelStartingWith(page, 'Pings: 0')).toBe('Pings: 0');
    expect(await boundsOfLabel(page, 'Design showcase')).not.toBeNull();
    expect(await boundsOfLabel(page, 'Hot-swap notes')).not.toBeNull();
    await page.screenshot({ path: screenshotPath('fui-cs-design-system-top.png') });
    await scrollToTop(page, 'ds-primary');
    // The primary button and the badge are in the shell's green; the secondary button is a light surface.
    await expect.poll(async () => {
      const primary = await pixelIn(page, 'ds-primary', 0.08, 0.5);
      return primary[1] > primary[0] && primary[1] > primary[2];
    }).toBe(true);
    const secondary = await pixelIn(page, 'ds-secondary', 0.08, 0.5);
    expect(secondary[0] + secondary[1] + secondary[2]).toBeGreaterThan(600);
    expect(rgb(await pixelAt(page, 'ds-row', 30, 28))).not.toBe(rgb(await pixelAt(page, 'ds-row-alt', 30, 28)));
    await page.screenshot({ path: screenshotPath('fui-cs-design-system.png') });
    expect(errors).toEqual([]);
  });

  test('the action button counts, and hovering a link lights it in the accent', async ({ page }) => {
    await openTestPage(page, baseUrl(), 'design-system');
    const button = await pointInLabel(page, 'Record ping');
    const home = await pointInLabel(page, 'Home', 0.15, 0.5);
    const idle = await page.screenshot({ clip: { x: Math.floor(home.x), y: Math.floor(home.y), width: 1, height: 1 } });

    await page.mouse.click(button.x, button.y);
    await page.mouse.click(button.x, button.y);
    await expect.poll(async () => await labelStartingWith(page, 'Pings: ')).toBe('Pings: 2');

    await page.mouse.move(home.x, home.y);
    await expect.poll(async () => {
      const hovered = await page.screenshot({ clip: { x: Math.floor(home.x), y: Math.floor(home.y), width: 1, height: 1 } });
      return hovered.equals(idle);
    }).toBe(false);
  });

  test('the page scrolls inside its own scroll box', async ({ page }) => {
    await openTestPage(page, baseUrl(), 'design-system');
    const before = await boundsOfLabel(page, 'Design system');
    const canvas = await page.locator('#fui-canvas').boundingBox();
    if (before === null || canvas === null) throw new Error('Expected the page title.');

    await page.mouse.move(canvas.x + (canvas.width / 2), canvas.y + (canvas.height / 2));
    await page.mouse.wheel(0, 400);

    await expect.poll(async () => (await boundsOfLabel(page, 'Design system'))?.y ?? before.y).toBeLessThan(before.y - 100);
  });
});
