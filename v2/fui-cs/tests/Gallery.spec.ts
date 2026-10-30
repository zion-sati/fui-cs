import { expect, test, type Page } from '@playwright/test';

import { boundsOf, clickNode, hostNode, labelOf, openTestPage, pointIn, screenshotPath, serveApp } from './support.js';

const baseUrl = serveApp();

/** Polls the readout until it contains every expected `key=value` pair. */
async function expectReadout(page: Page, ...pairs: string[]): Promise<void> {
  await expect.poll(async () => {
    const readout = (await labelOf(page, 'readout')) ?? '';
    return pairs.filter((pair) => !readout.split(' ').includes(pair));
  }).toEqual([]);
}

async function choose(page: Page, name: string): Promise<void> {
  const box = await page.getByRole('button', { name, exact: true }).boundingBox();
  if (box === null) throw new Error(`Expected the menu item ${name}.`);
  await page.mouse.move(box.x + 20, box.y + (box.height / 2));
  await page.evaluate(async () => await new Promise((resolve) => requestAnimationFrame(() => requestAnimationFrame(resolve))));
  await page.mouse.down();
  await page.mouse.up();
}

test.describe('Stage 3 control gallery', () => {
  test('every control drives the readout end to end', async ({ page }) => {
    const errors = await openTestPage(page, baseUrl(), 'gallery');
    await expectReadout(page, 'clicks=0', 'agree=False', 'wifi=off', 'size=none', 'volume=30', 'popup=closed');

    await clickNode(page, 'primary');
    await clickNode(page, 'primary');
    await expectReadout(page, 'clicks=2');

    await clickNode(page, 'agree');
    await clickNode(page, 'agree');
    await clickNode(page, 'wifi');
    await clickNode(page, 'size-m');
    await expectReadout(page, 'agree=Mixed', 'wifi=on', 'size=m');
    await page.keyboard.press('ArrowRight');
    await expectReadout(page, 'size=l');

    const volume = await boundsOf(page, 'volume');
    const canvas = await page.locator('#fui-canvas').boundingBox();
    const y = (canvas?.y ?? 0) + volume.y + (volume.height / 2);
    // The value spans the track from the thumb's center at either end: 5 px of chrome plus half the 18 px thumb.
    const x0 = (canvas?.x ?? 0) + volume.x + 14;
    const span = volume.width - 28;
    await page.mouse.move(x0 + (span * 0.3), y);
    await page.mouse.down();
    await page.mouse.move(x0 + (span * 0.7), y, { steps: 6 });
    await page.mouse.up();
    await expectReadout(page, 'volume=70');

    await clickNode(page, 'open-popup');
    await expectReadout(page, 'popup=open');
    expect(await hostNode(page, 'popup-surface')).not.toBeNull();
    await page.mouse.click(780, 580);
    await expectReadout(page, 'popup=closed');

    await clickNode(page, 'open-dialog');
    await expect.poll(async () => (await hostNode(page, 'dialog-ok')) !== null).toBe(true);
    await page.screenshot({ path: screenshotPath('fui-cs-gallery-dialog.png') });
    await page.keyboard.press('Enter');
    await expectReadout(page, 'dialog=accepted');

    const tile = await pointIn(page, 'tile');
    await page.mouse.move(tile.x, tile.y);
    await page.mouse.click(tile.x, tile.y, { button: 'right' });
    await choose(page, 'Duplicate');
    await expectReadout(page, 'menu=duplicated');

    await page.mouse.move(2, 2);
    await page.screenshot({ path: screenshotPath('fui-cs-gallery.png') });
    expect(errors).toEqual([]);
  });
});
