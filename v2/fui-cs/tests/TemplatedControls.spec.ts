import { expect, test, type Page } from '@playwright/test';

import { clickNode, debugNodes, hostNode, labelStartingWith, pointIn, screenshotPath, serveApp, waitForReady } from './support.js';

const baseUrl = serveApp();
const route = '/v2/fui-cs/demo/templated-controls/';

async function openRoute(page: Page): Promise<string[]> {
  const errors: string[] = [];
  page.on('pageerror', (error) => errors.push(error.message));
  page.on('console', (message) => {
    if (message.type() === 'error') errors.push(message.text());
  });
  // Tall enough that every section is on screen.
  await page.setViewportSize({ width: 1280, height: 2600 });
  await page.goto(`${baseUrl()}${route}`);
  await waitForReady(page);
  await expect.poll(async () => await labelStartingWith(page, 'Defaults: ')).not.toBeNull();
  return errors;
}

/** Clicks the center of the node whose semantic label is `label`. */
async function clickLabel(page: Page, label: string): Promise<void> {
  const node = (await debugNodes(page)).find((candidate) => candidate.semanticLabel === label);
  if (node === undefined) throw new Error(`No node is labelled "${label}".`);
  const canvas = await page.locator('#fui-canvas').boundingBox();
  if (canvas === null) throw new Error('Expected the fui-cs canvas.');
  await page.mouse.click(canvas.x + node.bounds.x + (node.bounds.width / 2), canvas.y + node.bounds.y + (node.bounds.height / 2));
}

test.describe('Templated controls route', () => {
  test('the route page shows its header, navigation and a summary of its controls', async ({ page }) => {
    const errors = await openRoute(page);

    await expect.poll(async () => await page.title()).toBe('EffinDOM FUI-CS Demo • Templated controls');
    expect(await labelStartingWith(page, 'Current route: ')).toBe(`Current route: ${route}`);
    expect(await labelStartingWith(page, 'Defaults: ')).toBe('Defaults: button taps 0, checkbox off, switch on, mode compact, intensity 40.');
    expect(await labelStartingWith(page, 'Override checkbox ')).toBe('Override checkbox stays on with its distinct per-control template.');
    for (const id of ['templated-controls:house-checkbox', 'templated-controls:color-button', 'templated-controls:color-radio-warm', 'templated-controls:color-slider']) {
      expect(await hostNode(page, id)).not.toBeNull();
    }
    await page.screenshot({ path: screenshotPath('fui-cs-templated-controls.png') });
    expect(errors).toEqual([]);
  });

  test('the templated controls keep their behavior and update the live summary', async ({ page }) => {
    await openRoute(page);

    await clickLabel(page, 'Run house action');
    await clickNode(page, 'templated-controls:house-checkbox');
    await clickNode(page, 'templated-controls:house-switch');
    await clickLabel(page, 'Bold chrome');
    // Pressing the thumb keeps the value; the arrow key then steps it by 5.
    const thumb = await pointIn(page, 'templated-controls:intensity-slider', 0.409, 0.5);
    await page.mouse.click(thumb.x, thumb.y);
    await page.keyboard.press('ArrowRight');

    await expect.poll(async () => await labelStartingWith(page, 'Defaults: '))
      .toBe('Defaults: button taps 1, checkbox on, switch off, mode bold, intensity 45.');
    await clickNode(page, 'templated-controls:override-checkbox');
    await expect.poll(async () => await labelStartingWith(page, 'Override checkbox '))
      .toBe('Override checkbox stays off with its distinct per-control template.');
  });

  test('the controls keep their state across a route swap and Back', async ({ page }) => {
    await openRoute(page);
    await clickNode(page, 'templated-controls:house-checkbox');
    await clickNode(page, 'templated-controls:house-switch');
    await expect.poll(async () => await labelStartingWith(page, 'Defaults: ')).toContain('checkbox on, switch off');

    await clickLabel(page, 'Dashboard');
    await expect.poll(() => new URL(page.url()).pathname).toBe('/v2/fui-cs/demo/');
    await page.goBack();

    await expect.poll(() => new URL(page.url()).pathname).toBe(route);
    await expect.poll(async () => await labelStartingWith(page, 'Defaults: ')).toContain('checkbox on, switch off');
  });
});
