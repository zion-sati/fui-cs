import { expect, test, type Page } from '@playwright/test';

import { clickNode, labelOf, openTestPage, pointIn, screenshotPath, serveApp } from './support.js';

const baseUrl = serveApp();

async function expectLabel(page: Page, nodeId: string, text: string): Promise<void> {
  await expect.poll(async () => await labelOf(page, nodeId)).toBe(text);
}

async function rightClick(page: Page, nodeId: string, fx = 0.5, fy = 0.5): Promise<void> {
  const point = await pointIn(page, nodeId, fx, fy);
  await page.mouse.move(point.x, point.y);
  await page.mouse.click(point.x, point.y, { button: 'right' });
}

function item(page: Page, name: string) {
  return page.getByRole('button', { name, exact: true });
}

/** Moves to a menu item, then clicks it a frame later, as a person would. */
async function choose(page: Page, name: string): Promise<void> {
  const box = await item(page, name).boundingBox();
  if (box === null) throw new Error(`Expected the menu item ${name}.`);
  await page.mouse.move(box.x + 20, box.y + (box.height / 2));
  await page.evaluate(async () => await new Promise((resolve) => requestAnimationFrame(() => requestAnimationFrame(resolve))));
  await page.mouse.down();
  await page.mouse.up();
}

async function canvasCursor(page: Page): Promise<string> {
  return await page.evaluate(() => (document.getElementById('fui-canvas') as HTMLCanvasElement).style.cursor);
}

test.describe('ContextMenu in the host', () => {
  test('menu items show a pointer, disabled ones the default; links point, images and SVGs do not', async ({ page }) => {
    await openTestPage(page, baseUrl(), 'context-menu');
    for (const [nodeId, cursor] of [['docs-link', 'pointer'], ['menu-image', 'default'], ['menu-svg', 'default']] as const) {
      const point = await pointIn(page, nodeId);
      await page.mouse.move(point.x, point.y);
      await expect.poll(async () => await canvasCursor(page), { message: nodeId }).toBe(cursor);
    }

    await rightClick(page, 'words', 0.2);
    await expect(item(page, 'Select All')).toHaveCount(1);
    for (const [name, cursor] of [['Select All', 'pointer'], ['Copy', 'default']] as const) {
      const box = await item(page, name).boundingBox();
      if (box === null) throw new Error(`Expected the menu item ${name}.`);
      await page.mouse.move(box.x + 20, box.y + (box.height / 2));
      await expect.poll(async () => await canvasCursor(page), { message: name }).toBe(cursor);
    }
    await page.keyboard.press('Escape');
  });

  test('right-clicking the page shows the built-in menu; Escape hides it', async ({ page }) => {
    const errors = await openTestPage(page, baseUrl(), 'context-menu');

    await rightClick(page, 'blank');

    await expect(item(page, 'Reload Page')).toHaveCount(1);
    await expect(item(page, 'Back')).toHaveCount(0);
    await page.screenshot({ path: screenshotPath('fui-cs-context-menu-page.png') });
    await page.keyboard.press('Escape');
    await expect(item(page, 'Reload Page')).toHaveCount(0);
    expect(errors).toEqual([]);
  });

  test('selectable text offers Select All, then Copy of the selection', async ({ page, context }) => {
    await context.grantPermissions(['clipboard-read', 'clipboard-write']);
    await openTestPage(page, baseUrl(), 'context-menu');

    await rightClick(page, 'words', 0.2);
    await expect(item(page, 'Copy')).toHaveAttribute('aria-disabled', 'true');
    await choose(page, 'Select All');
    await expect(item(page, 'Select All')).toHaveCount(0);

    await rightClick(page, 'words', 0.2);
    await expect(item(page, 'Copy')).not.toHaveAttribute('aria-disabled', 'true');
    await choose(page, 'Copy');
    await expect.poll(async () => await page.evaluate(async () => await navigator.clipboard.readText())).toBe('Selectable words for the menu');
  });

  test('a node shows its own menu whose items run and whose disabled items do nothing', async ({ page }) => {
    await openTestPage(page, baseUrl(), 'context-menu');

    await rightClick(page, 'file');
    await expectLabel(page, 'menu-visible', 'shown');
    await expect(item(page, 'Reload Page')).toHaveCount(0);
    await page.screenshot({ path: screenshotPath('fui-cs-context-menu-app.png') });
    await choose(page, 'Delete');
    await page.waitForTimeout(100);
    expect(await labelOf(page, 'menu-visible')).toBe('shown');

    await choose(page, 'Duplicate');
    await expectLabel(page, 'menu-log', 'duplicated');
    await expectLabel(page, 'menu-visible', 'hidden');
  });

  test('a press outside hides the menu without reaching the page', async ({ page }) => {
    await openTestPage(page, baseUrl(), 'context-menu');

    await rightClick(page, 'file');
    await expectLabel(page, 'menu-visible', 'shown');
    await clickNode(page, 'under');

    await expectLabel(page, 'menu-visible', 'hidden');
    await page.waitForTimeout(100);
    expect(await labelOf(page, 'menu-log')).toBe('none');
    await clickNode(page, 'under');
    await expectLabel(page, 'menu-log', 'under clicked');
  });

  test('a region with menus disabled shows none', async ({ page }) => {
    await openTestPage(page, baseUrl(), 'context-menu');

    await rightClick(page, 'no-menu');
    await page.waitForTimeout(200);

    await expect(item(page, 'Reload Page')).toHaveCount(0);
    expect(await labelOf(page, 'menu-visible')).toBe('hidden');
  });
});

test.describe('Non-editable context targets', () => {
  test('a link previews its URL while hovered and a click navigates', async ({ page }) => {
    await openTestPage(page, baseUrl(), 'context-menu');
    const preview = page.locator('#fui-url-bar');
    const point = await pointIn(page, 'docs-link');

    await page.mouse.move(point.x, point.y);
    await expect(preview).toBeVisible();
    await expect(preview).toContainText('?page=context-menu#docs');
    await page.mouse.move(2, 2);
    await expect(preview).toBeHidden();

    await clickNode(page, 'docs-link');
    await expect.poll(async () => await labelOf(page, 'link-log')).toBe('navigated ?page=context-menu#docs');
    await expect(page).toHaveURL(/#docs$/);
  });

  test('right-clicking a link offers it and keeps its preview while the menu is open', async ({ page }) => {
    await openTestPage(page, baseUrl(), 'context-menu');
    const preview = page.locator('#fui-url-bar');

    await rightClick(page, 'docs-link');
    await expect(item(page, 'New Tab')).toHaveCount(1);
    await expect(item(page, 'Open')).toHaveCount(1);
    await expect(preview).toBeVisible();
    await page.keyboard.press('Escape');
    await expect(item(page, 'Open')).toHaveCount(0);
  });

  test('images and SVGs offer to open their URL', async ({ page }) => {
    await openTestPage(page, baseUrl(), 'context-menu');

    await rightClick(page, 'menu-image');
    await expect(item(page, 'New Tab')).toHaveCount(1);
    await page.keyboard.press('Escape');
    await rightClick(page, 'menu-svg');
    await expect(item(page, 'Open')).toHaveCount(1);
    await page.keyboard.press('Escape');
    await rightClick(page, 'blank');
    await expect(item(page, 'Open')).toHaveCount(0);
    await expect(item(page, 'Reload Page')).toHaveCount(1);
  });
});
