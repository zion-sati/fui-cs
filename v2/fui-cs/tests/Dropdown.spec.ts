import { expect, test, type Page } from '@playwright/test';

import { boundsOf, clickNode, debugNodes, labelOf, labelStartingWith, nextFrames, screenshotPath, serveApp, waitForReady, type HostNode } from './support.js';

const baseUrl = serveApp();

const LIST_ITEM_ROLE = 7;

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
  await expect.poll(async () => await labelOf(page, 'dropdown-readout')).toBe('Dropdown: Balanced (0)');
  return errors;
}

/** Wheels the page until the node sits inside the window with `roomBelow` pixels under it. */
async function scrollIntoView(page: Page, nodeId: string, roomBelow = 120): Promise<void> {
  const viewportHeight = page.viewportSize()?.height ?? 700;
  await page.mouse.move(980, 300);
  await expect.poll(async () => {
    const bounds = await boundsOf(page, nodeId);
    if (bounds.y + bounds.height <= viewportHeight - roomBelow) return true;
    await page.mouse.wheel(0, 240);
    return false;
  }, { timeout: 10_000 }).toBe(true);
  // Let the smooth scroll come to rest before reading positions.
  await expect.poll(async () => {
    const before = (await boundsOf(page, nodeId)).y;
    await nextFrames(page);
    return (await boundsOf(page, nodeId)).y === before;
  }).toBe(true);
}

/** The open list's rows, top to bottom. */
async function rows(page: Page): Promise<HostNode[]> {
  const listed = (await debugNodes(page)).filter((node) => node.semanticRole === LIST_ITEM_ROLE && node.bounds.height > 0);
  return [...listed].sort((a, b) => a.bounds.y - b.bounds.y);
}

/** The labels of the rows the list's scroll view shows. */
async function shownRows(page: Page): Promise<string[]> {
  return (await rows(page)).filter((row) => row.visibleBounds.height > 1).map((row) => row.semanticLabel);
}

async function clickRow(page: Page, label: string): Promise<void> {
  const row = (await rows(page)).find((candidate) => candidate.semanticLabel === label);
  if (row === undefined) throw new Error(`No open row is labelled "${label}".`);
  const canvas = await page.locator('#fui-canvas').boundingBox();
  if (canvas === null) throw new Error('Expected the fui-cs canvas.');
  await page.mouse.move(canvas.x + row.bounds.x + 30, canvas.y + row.bounds.y + (row.bounds.height / 2));
  await nextFrames(page);
  await page.mouse.down();
  await page.mouse.up();
}

test.describe('Dropdown in the host', () => {
  test('a press opens the list, a release on a row selects it, and a press outside closes without choosing', async ({ page }) => {
    const errors = await openWorkbench(page);
    await scrollIntoView(page, 'dropdown-plain');

    await clickNode(page, 'dropdown-plain', 0.3);
    await expect.poll(async () => (await rows(page)).map((row) => row.semanticLabel)).toEqual(['Balanced', 'Quality first', 'Speed first']);
    // The list opens under the field at its width.
    const field = await boundsOf(page, 'dropdown-plain');
    const [first] = await rows(page);
    expect(first.bounds.y).toBeGreaterThan(field.y + field.height);
    expect(first.bounds.width).toBeLessThanOrEqual(field.width);
    await page.screenshot({ path: screenshotPath('fui-cs-dropdown-open.png') });

    await clickRow(page, 'Speed first');
    await expect.poll(async () => await labelOf(page, 'dropdown-readout')).toBe('Dropdown: Speed first (2)');
    await expect.poll(async () => (await rows(page)).length).toBe(0);

    await clickNode(page, 'dropdown-plain', 0.3);
    await expect.poll(async () => (await rows(page)).length).toBe(3);
    const canvas = await page.locator('#fui-canvas').boundingBox();
    await page.mouse.click((canvas?.x ?? 0) + 900, (canvas?.y ?? 0) + 40);
    await expect.poll(async () => (await rows(page)).length).toBe(0);
    expect(await labelOf(page, 'dropdown-readout')).toBe('Dropdown: Speed first (2)');
    expect(errors).toEqual([]);
  });

  test('the keyboard opens, moves, selects and closes while focus stays on the field, and Tab moves on', async ({ page }) => {
    const errors = await openWorkbench(page);
    await scrollIntoView(page, 'dropdown-plain');
    await clickNode(page, 'dropdown-plain', 0.3);
    await expect.poll(async () => (await rows(page)).length).toBe(3);
    await page.keyboard.press('Escape');
    await expect.poll(async () => (await rows(page)).length).toBe(0);

    await page.keyboard.press('ArrowDown');
    await expect.poll(async () => (await rows(page)).length).toBe(3);
    await page.keyboard.press('ArrowDown');
    await page.keyboard.press('Enter');
    await expect.poll(async () => await labelOf(page, 'dropdown-readout')).toBe('Dropdown: Quality first (1)');
    await expect.poll(async () => (await rows(page)).length).toBe(0);

    // Up opens on the row above the selection, and Space opens too.
    await page.keyboard.press('ArrowUp');
    await page.keyboard.press('Enter');
    await expect.poll(async () => await labelOf(page, 'dropdown-readout')).toBe('Dropdown: Balanced (0)');
    await page.keyboard.press(' ');
    await expect.poll(async () => (await rows(page)).length).toBe(3);
    await page.keyboard.press('Escape');

    // Tab skips the disabled dropdown to the long list, which Enter opens.
    await page.keyboard.press('Tab');
    await page.keyboard.press('Enter');
    await expect.poll(async () => (await rows(page))[0]?.semanticLabel).toBe('Item 1');
    expect(errors).toEqual([]);
  });

  test('a long list shows five rows and scrolls the highlight into view', async ({ page }) => {
    const errors = await openWorkbench(page);
    await scrollIntoView(page, 'dropdown-long');

    await clickNode(page, 'dropdown-long', 0.3);
    await expect.poll(async () => await shownRows(page)).toEqual(['Item 1', 'Item 2', 'Item 3', 'Item 4', 'Item 5']);
    await page.keyboard.press('End');
    await expect.poll(async () => (await shownRows(page)).at(-1)).toBe('Item 30');
    expect(await shownRows(page)).not.toContain('Item 1');
    await page.keyboard.press('Enter');

    // Reopening shows the selected row again.
    await clickNode(page, 'dropdown-long', 0.3);
    await expect.poll(async () => await shownRows(page)).toContain('Item 30');
    expect(errors).toEqual([]);
  });

  test('a list taller than the window is clamped inside it', async ({ page }) => {
    const errors = await openWorkbench(page);
    await scrollIntoView(page, 'dropdown-clamped', 16);

    await clickNode(page, 'dropdown-clamped', 0.3);
    await expect.poll(async () => (await shownRows(page)).length).toBeGreaterThan(10);
    const shown = (await rows(page)).filter((row) => row.visibleBounds.height > 1);
    const top = Math.min(...shown.map((row) => row.visibleBounds.y));
    const bottom = Math.max(...shown.map((row) => row.visibleBounds.y + row.visibleBounds.height));
    expect(top).toBeGreaterThanOrEqual(8);
    expect(bottom).toBeLessThanOrEqual(700 - 8);
    expect(shown.length).toBeLessThan(60);
    expect(errors).toEqual([]);
  });

  test('templated rows take the template\'s height, and a disabled dropdown does not open', async ({ page }) => {
    const errors = await openWorkbench(page);
    await scrollIntoView(page, 'dropdown-templated', 16);

    await clickNode(page, 'dropdown-disabled', 0.3);
    await nextFrames(page);
    expect((await rows(page)).length).toBe(0);

    await clickNode(page, 'dropdown-templated', 0.3);
    await expect.poll(async () => (await rows(page)).map((row) => Math.round(row.bounds.height))).toEqual([44, 44, 44]);
    expect(errors).toEqual([]);
  });

  test('the open list follows a theme change', async ({ page }) => {
    await page.emulateMedia({ colorScheme: 'light' });
    const errors = await openWorkbench(page);
    await scrollIntoView(page, 'dropdown-plain');
    await clickNode(page, 'dropdown-plain', 0.3);
    await expect.poll(async () => (await rows(page)).length).toBe(3);
    const row = (await rows(page))[2];
    const canvas = await page.locator('#fui-canvas').boundingBox();
    const rowPoint = { x: (canvas?.x ?? 0) + row.bounds.x + row.bounds.width - 12, y: (canvas?.y ?? 0) + row.bounds.y + (row.bounds.height / 2) };
    const panelPixel = async (): Promise<number> => {
      const png = await page.screenshot({ clip: { x: Math.floor(rowPoint.x), y: Math.floor(rowPoint.y), width: 1, height: 1 } });
      return await page.evaluate(async (base64) => {
        const bytes = Uint8Array.from(atob(base64), (character) => character.charCodeAt(0));
        const bitmap = await createImageBitmap(new Blob([bytes], { type: 'image/png' }));
        const context = new OffscreenCanvas(1, 1).getContext('2d');
        if (context === null) throw new Error('Expected a 2D context.');
        context.drawImage(bitmap, 0, 0);
        const data = context.getImageData(0, 0, 1, 1).data;
        return (data.at(0) ?? 0) + (data.at(1) ?? 0) + (data.at(2) ?? 0);
      }, png.toString('base64'));
    };
    await expect.poll(panelPixel).toBeGreaterThan(600);

    await page.emulateMedia({ colorScheme: 'dark' });
    await expect.poll(panelPixel).toBeLessThan(200);
    expect(errors).toEqual([]);
  });
});

test.describe('Dropdown on the dashboard', () => {
  test('choosing a render mode updates the dashboard\'s status line', async ({ page }) => {
    const errors: string[] = [];
    page.on('pageerror', (error) => errors.push(error.message));
    await page.setViewportSize({ width: 1280, height: 900 });
    await page.goto(`${baseUrl()}/v2/fui-cs/demo/`);
    await waitForReady(page);
    await expect.poll(async () => await labelStartingWith(page, 'Dropdown: ')).toBe('Dropdown: Balanced');
    await page.mouse.move(800, 450);
    await expect.poll(async () => {
      const bounds = await boundsOf(page, 'demo-dashboard:render-mode-dropdown');
      if (bounds.y + bounds.height <= 640) return true;
      await page.mouse.wheel(0, 300);
      return false;
    }, { timeout: 10_000 }).toBe(true);
    await expect.poll(async () => {
      const before = (await boundsOf(page, 'demo-dashboard:render-mode-dropdown')).y;
      await nextFrames(page);
      return (await boundsOf(page, 'demo-dashboard:render-mode-dropdown')).y === before;
    }).toBe(true);

    await clickNode(page, 'demo-dashboard:render-mode-dropdown', 0.3);
    await expect.poll(async () => (await rows(page)).map((row) => row.semanticLabel)).toEqual(['Balanced', 'Quality first', 'Speed first']);
    await clickRow(page, 'Quality first');
    await expect.poll(async () => await labelStartingWith(page, 'Dropdown: ')).toBe('Dropdown: Quality first');
    expect(errors).toEqual([]);
  });
});
