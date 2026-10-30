import { expect, test, type Page } from '@playwright/test';

import { boundsOfLabel, debugNodes, labelStartingWith, nextFrames, screenshotPath, serveApp, waitForReady, type Bounds } from './support.js';

const baseUrl = serveApp();

interface Point {
  readonly x: number;
  readonly y: number;
}

async function openAdvancedControls(page: Page): Promise<void> {
  await page.setViewportSize({ width: 1024, height: 700 });
  await page.goto(`${baseUrl()}/v2/fui-cs/demo/advanced-controls/`);
  await waitForReady(page);
  await expect.poll(async () => await labelStartingWith(page, 'Reorder order: ')).not.toBeNull();
}

async function canvasBox(page: Page): Promise<Bounds> {
  const box = await page.locator('#fui-canvas').boundingBox();
  if (box === null) throw new Error('Expected the fui-cs canvas.');
  return box;
}

/** A page coordinate inside the labelled node, at the given fraction of its width and height. */
async function pointInLabel(page: Page, label: string, fx = 0.5, fy = 0.5): Promise<Point> {
  const bounds = await boundsOfLabel(page, label);
  if (bounds === null) throw new Error(`No node is labelled "${label}".`);
  const canvas = await canvasBox(page);
  return { x: canvas.x + bounds.x + (bounds.width * fx), y: canvas.y + bounds.y + (bounds.height * fy) };
}

/** Wheels the route page, in its left padding, until the labelled node's top and bottom satisfy `fits`. */
async function scrollPageUntil(page: Page, label: string, fits: (bounds: Bounds, canvas: Bounds) => number): Promise<void> {
  const canvas = await canvasBox(page);
  for (let attempt = 0; attempt < 30; attempt += 1) {
    const bounds = await boundsOfLabel(page, label);
    if (bounds === null) throw new Error(`No node is labelled "${label}".`);
    const delta = fits(bounds, canvas);
    if (delta === 0) {
      // The page scrolls smoothly: it is in place once the node stops moving.
      await page.waitForTimeout(120);
      if ((await boundsOfLabel(page, label))?.y === bounds.y) return;
      continue;
    }
    await page.mouse.move(canvas.x + 16, canvas.y + 16);
    await page.mouse.wheel(0, Math.max(-720, Math.min(720, delta)));
    await page.waitForTimeout(80);
  }
  throw new Error(`Unable to scroll ${label} into place.`);
}

async function scrollIntoView(page: Page, label: string): Promise<void> {
  await scrollPageUntil(page, label, (bounds, canvas) =>
    bounds.y >= 48 && bounds.y + bounds.height <= canvas.height - 32 ? 0 : bounds.y - 220);
}

async function moveNearCanvasBottom(page: Page, label: string, bottomMargin: number): Promise<void> {
  await scrollPageUntil(page, label, (bounds, canvas) => {
    const bottom = bounds.y + bounds.height;
    return bottom >= canvas.height - bottomMargin && bottom <= canvas.height - 8 ? 0 : bottom - (canvas.height - bottomMargin);
  });
}

async function dragLabelToLabel(page: Page, sourceLabel: string, targetLabel: string): Promise<void> {
  const start = await pointInLabel(page, sourceLabel);
  const end = await pointInLabel(page, targetLabel);
  await page.mouse.move(start.x, start.y);
  await page.mouse.down();
  await page.mouse.move(end.x, end.y, { steps: 10 });
  await page.mouse.up();
}

/** The item numbers of the rows whose insertion marker (the first child of the row's slot) is showing. */
async function visibleMarkers(page: Page): Promise<number[]> {
  const nodes = await debugNodes(page);
  const byHandle = new Map(nodes.map((node) => [node.handle, node]));
  const rows: number[] = [];
  for (const card of nodes) {
    const match = /^Reorder item (\d+): /.exec(card.semanticLabel);
    const slot = card.parentHandle === null ? undefined : byHandle.get(card.parentHandle);
    const marker = slot === undefined ? undefined : byHandle.get(slot.childHandles[0] ?? '');
    if (match !== null && marker !== undefined && marker.style.opacity > 0.5) rows.push(Number(match[1]));
  }
  return rows;
}

async function canvasCursor(page: Page): Promise<string> {
  return await page.evaluate(() => (document.getElementById('fui-canvas') as HTMLCanvasElement).style.cursor);
}

test.describe('Advanced controls drag-and-drop reorder', () => {
  test('dragging a grip onto another row moves the item there', async ({ page }) => {
    await openAdvancedControls(page);
    await scrollIntoView(page, 'Reorder demo viewport');

    await dragLabelToLabel(page, 'Drag grip for Add drag reorder demo', 'Audit font shard cache');

    await expect.poll(async () => await labelStartingWith(page, 'Reorder order: '))
      .toContain('Document Core rename | Add drag reorder demo | Audit font shard cache');
    await expect.poll(async () => await labelStartingWith(page, 'Reorder drag status: '))
      .toContain('moved Add drag reorder demo to slot 2');
  });

  test('a move onto the spot the trailing preview covers keeps the drop target', async ({ page }) => {
    await openAdvancedControls(page);
    await scrollIntoView(page, 'Reorder demo viewport');
    const start = await pointInLabel(page, 'Drag grip for Document Core rename');
    // Left of the row's centre, so the preview is not clamped against the section's right edge.
    const over = await pointInLabel(page, 'Reorder item 2: Audit font shard cache', 0.2, 0.3);
    await page.mouse.move(start.x, start.y);
    await page.mouse.down();
    await page.mouse.move(over.x, over.y, { steps: 10 });
    await expect.poll(async () => await labelStartingWith(page, 'Reorder drag status: ')).toMatch(/preview slot \d+$/);

    // The preview sits 2 px below and right of where the pointer was, with its first line of text 15 px inside it, so
    // this move lands on that text.
    await page.mouse.move(over.x + 30, over.y + 24);
    await nextFrames(page);
    await page.waitForTimeout(100);
    expect(await labelStartingWith(page, 'Reorder drag status: ')).toMatch(/preview slot \d+$/);
    await page.mouse.up();
  });

  test('moving across a row\'s parts keeps the insertion marker on that row', async ({ page }) => {
    await openAdvancedControls(page);
    await scrollIntoView(page, 'Reorder demo viewport');
    const start = await pointInLabel(page, 'Drag grip for Document Core rename');
    const row = await boundsOfLabel(page, 'Reorder item 2: Audit font shard cache');
    if (row === null) throw new Error('Expected the second row.');
    const canvas = await canvasBox(page);
    const at = (fx: number, fy: number): Point => ({ x: canvas.x + row.x + (row.width * fx), y: canvas.y + row.y + (row.height * fy) });
    const title = at(0.5, 0.3);
    await page.mouse.move(start.x, start.y);
    await page.mouse.down();
    await page.mouse.move(title.x, title.y, { steps: 10 });

    // The detail text, the card's padding, the grip and the title: each move enters a different node of the row, and
    // a commit after it must not hand the drop target back to the dragged row.
    for (const [fx, fy] of [[0.5, 0.75], [0.03, 0.5], [0.12, 0.5], [0.5, 0.3]] as const) {
      const point = at(fx, fy);
      await page.mouse.move(point.x, point.y);
      await nextFrames(page);
      await page.waitForTimeout(100);
      expect(await visibleMarkers(page)).toEqual([2]);
    }
    await page.mouse.up();
  });

  test('a touch held on a grip picks the row up, and lifting drops it', async ({ page }) => {
    await page.addInitScript(() => {
      Object.defineProperty(navigator, 'maxTouchPoints', { configurable: true, get: () => 5 });
    });
    await openAdvancedControls(page);
    await scrollIntoView(page, 'Reorder demo viewport');
    await page.waitForTimeout(400);
    const start = await pointInLabel(page, 'Drag grip for Document Core rename');
    const end = await pointInLabel(page, 'Reorder demo viewport', 0.5, 0.75);
    const cdp = await page.context().newCDPSession(page);

    await cdp.send('Input.dispatchTouchEvent', { type: 'touchStart', touchPoints: [{ ...start, id: 71, radiusX: 8, radiusY: 8, force: 1 }] });
    await page.waitForTimeout(900);
    await expect.poll(async () => await labelStartingWith(page, 'Reorder drag status: ')).toContain('dragging Document Core rename');
    for (let step = 1; step <= 8; step += 1) {
      const progress = step / 8;
      await cdp.send('Input.dispatchTouchEvent', {
        type: 'touchMove',
        touchPoints: [{ x: start.x + ((end.x - start.x) * progress), y: start.y + ((end.y - start.y) * progress), id: 71, radiusX: 8, radiusY: 8, force: 1 }],
      });
      await page.waitForTimeout(35);
    }
    await cdp.send('Input.dispatchTouchEvent', { type: 'touchEnd', touchPoints: [] });

    await expect.poll(async () => await labelStartingWith(page, 'Reorder drag status: ')).toContain('moved Document Core rename');
    await expect.poll(async () => await labelStartingWith(page, 'Reorder order: '))
      .not.toContain('Reorder order: Document Core rename | Audit font shard cache');
  });

  test('a drag shows its preview and the grab, then move, cursor', async ({ page }) => {
    await openAdvancedControls(page);
    await scrollIntoView(page, 'Reorder demo viewport');
    const start = await pointInLabel(page, 'Drag grip for Add drag reorder demo');
    // Left of the title's centre, so the preview is not clamped against the section's right edge.
    const end = await pointInLabel(page, 'Audit font shard cache', 0.2, 0.5);

    await page.mouse.move(start.x, start.y);
    await expect.poll(async () => await canvasCursor(page)).toBe('grab');
    await page.mouse.down();
    await page.mouse.move(end.x, end.y, { steps: 10 });

    await expect.poll(async () => await canvasCursor(page)).toBe('move');
    await expect.poll(async () => await labelStartingWith(page, 'Reorder drag preview for ')).toContain('Add drag reorder demo');
    await expect.poll(async () => await labelStartingWith(page, 'Reorder drag status: ')).toContain('preview slot 2');
    // The preview sits 2 px below and right of the pointer, which the browser rounds to whole pixels.
    const preview = await boundsOfLabel(page, 'Reorder drag preview for Add drag reorder demo');
    const canvas = await canvasBox(page);
    expect(Math.abs((preview?.x ?? 0) - (end.x - canvas.x + 2))).toBeLessThanOrEqual(1);
    expect(Math.abs((preview?.y ?? 0) - (end.y - canvas.y + 2))).toBeLessThanOrEqual(1);
    await page.screenshot({ path: screenshotPath('fui-cs-reorder-drag.png') });

    await page.mouse.up();
    await expect.poll(async () => await canvasCursor(page)).not.toBe('move');
  });

  test('releasing outside every drop target cancels the drag and keeps the order', async ({ page }) => {
    await openAdvancedControls(page);
    await scrollIntoView(page, 'Reorder demo viewport');
    const before = await labelStartingWith(page, 'Reorder order: ');
    const start = await pointInLabel(page, 'Drag grip for Add drag reorder demo');
    const canvas = await canvasBox(page);

    await page.mouse.move(start.x, start.y);
    await page.mouse.down();
    await page.mouse.move(canvas.x + 24, canvas.y + 24, { steps: 10 });
    await expect.poll(async () => await canvasCursor(page)).toBe('grabbing');
    await page.mouse.up();

    await expect.poll(async () => await labelStartingWith(page, 'Reorder order: ')).toBe(before);
    await expect.poll(async () => await labelStartingWith(page, 'Reorder drag status: ')).toContain('cancelled Add drag reorder demo');
  });

  test('holding a drag near the bottom edge scrolls the list', async ({ page }) => {
    await openAdvancedControls(page);
    await scrollIntoView(page, 'Reorder demo viewport');
    const start = await pointInLabel(page, 'Drag grip for Document Core rename');
    const target = await pointInLabel(page, 'Add drag reorder demo');
    const viewport = await boundsOfLabel(page, 'Reorder demo viewport');
    const canvas = await canvasBox(page);
    if (viewport === null) throw new Error('Expected the reorder viewport.');

    await page.mouse.move(start.x, start.y);
    await page.mouse.down();
    await page.mouse.move(target.x, canvas.y + viewport.y + viewport.height - 6, { steps: 12 });

    await expect.poll(async () => {
      const match = /offset (\d+)/.exec((await labelStartingWith(page, 'Reorder viewport status: ')) ?? '');
      return match === null ? 0 : Number.parseInt(match[1], 10);
    }).toBeGreaterThan(0);
    await page.waitForTimeout(300);
    await page.mouse.up();

    await expect.poll(async () => await labelStartingWith(page, 'Reorder order: '))
      .not.toContain('Reorder order: Document Core rename | Audit font shard cache');
  });

  test('a drop inside the list works with the route page already scrolled', async ({ page }) => {
    await openAdvancedControls(page);
    await moveNearCanvasBottom(page, 'Reorder demo viewport', 28);

    await dragLabelToLabel(page, 'Drag grip for Audit font shard cache', 'Document Core rename');

    await expect.poll(async () => ({
      first: await labelStartingWith(page, 'Reorder item 1: '),
      second: await labelStartingWith(page, 'Reorder item 2: '),
    })).toEqual({
      first: 'Reorder item 1: Audit font shard cache',
      second: 'Reorder item 2: Document Core rename',
    });
  });
});
