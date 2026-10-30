import { expect, test, type Page } from '@playwright/test';

import { boundsOf, clickNode, hostNode, labelOf, pixelIn, pointIn, rgb, screenshotPath, serveApp, waitForReady } from './support.js';

const baseUrl = serveApp();

type Pixel = readonly [number, number, number, number];

/**
 * Opens the workbench route. The host may paint its first frames before the route's theme and scene settle (up to a
 * second under load), so pixel checks poll until they hold.
 */
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
  await expect.poll(async () => await labelOf(page, 'probe-log')).toBe('probe not clicked');
  return errors;
}

function distance(a: Pixel, b: Pixel): number {
  return Math.abs(a[0] - b[0]) + Math.abs(a[1] - b[1]) + Math.abs(a[2] - b[2]);
}

test.describe('Workbench style showcase', () => {
  test('the card paints with a shadow that darkens the page below it', async ({ page }) => {
    const errors = await openWorkbench(page);

    // How much darker the shadow below the card is than the page beside it.
    await expect.poll(async () => {
      const below = await pixelIn(page, 'gradient-card', 0.5, 1.08);
      const page0 = await pixelIn(page, 'gradient-card', 1.5, 0.5);
      return (page0[0] + page0[1] + page0[2]) - (below[0] + below[1] + below[2]);
    }).toBeGreaterThan(15);
    await page.screenshot({ path: screenshotPath('fui-cs-workbench-style.png') });
    expect(errors).toEqual([]);
  });

  test('a linear gradient runs across its own node', async ({ page }) => {
    await openWorkbench(page);

    // Blue at the start, green at the end.
    await expect.poll(async () => {
      const left = await pixelIn(page, 'gradient-card', 0.05, 0.8);
      const right = await pixelIn(page, 'gradient-card', 0.95, 0.8);
      return left[2] > left[1] && right[1] > right[2];
    }).toBe(true);
  });

  test('background blur softens the stripes behind the glass panel', async ({ page }) => {
    await openWorkbench(page);
    const host = await boundsOf(page, 'glass-host');
    // The first stripe edge is a quarter of the way across; the glass covers rows 20 to 60 of the 80-row host.
    const edge = 0.25;
    const step = 4 / host.width;
    // The edge is sharp below the glass and less than half as sharp through it.
    await expect.poll(async () => {
      const sharp = distance(await pixelIn(page, 'glass-host', edge - step, 0.94), await pixelIn(page, 'glass-host', edge + step, 0.94));
      const blurred = distance(await pixelIn(page, 'glass-host', edge - step, 0.5), await pixelIn(page, 'glass-host', edge + step, 0.5));
      return sharp > 60 && blurred < sharp / 2;
    }).toBe(true);
  });

  test('an accent swatch re-themes the route', async ({ page }) => {
    await openWorkbench(page);
    // The default accent, once the route has painted in its settled theme.
    const before = 0x2563eb;
    await expect.poll(async () => rgb(await pixelIn(page, 'accent-sample', 0.06, 0.5))).toBe(before);

    await clickNode(page, 'accent-purple');
    await expect.poll(async () => await labelOf(page, 'accent-log')).toBe('accent purple');
    await page.mouse.move(2, 2);

    await expect.poll(async () => rgb(await pixelIn(page, 'accent-sample', 0.06, 0.5))).not.toBe(before);
    const after = await pixelIn(page, 'accent-sample', 0.06, 0.5);
    expect(after[0]).toBeGreaterThan(after[1]);
    expect(after[2]).toBeGreaterThan(after[1]);
  });

  test('the pair stacks when the viewport narrows', async ({ page }) => {
    await openWorkbench(page);
    const wideA = await boundsOf(page, 'pair-a');
    const wideB = await boundsOf(page, 'pair-b');
    expect(wideB.y).toBe(wideA.y);
    expect(wideB.x).toBeGreaterThan(wideA.x);

    await page.setViewportSize({ width: 900, height: 700 });

    await expect.poll(async () => (await boundsOf(page, 'pair-b')).y).toBeGreaterThan(wideA.y);
    expect((await boundsOf(page, 'pair-b')).x).toBe((await boundsOf(page, 'pair-a')).x);
  });

  test('the probe shows its cursor and maps the click into its own coordinates', async ({ page }) => {
    await openWorkbench(page);
    const point = await pointIn(page, 'probe', 0.25, 0.5);

    await page.mouse.move(point.x, point.y);
    await expect.poll(async () => await page.evaluate(() => (document.getElementById('fui-canvas') as HTMLCanvasElement).style.cursor)).toBe('move');
    await clickNode(page, 'probe', 0.25, 0.5);

    await expect.poll(async () => await labelOf(page, 'probe-log')).toMatch(/^local (39|40),(29|30) round trip ok$/);
  });
});

async function canvasCursor(page: Page): Promise<string> {
  return await page.evaluate(() => (document.getElementById('fui-canvas') as HTMLCanvasElement).style.cursor);
}

async function hover(page: Page, nodeId: string, fx = 0.5, fy = 0.5): Promise<void> {
  const point = await pointIn(page, nodeId, fx, fy);
  await page.mouse.move(point.x, point.y);
}

test.describe('Workbench cursors', () => {
  test('each target shows its cursor and blank, disabled and hidden ones show the default', async ({ page }) => {
    const errors = await openWorkbench(page);
    const cases: readonly (readonly [string, string, number?])[] = [
      ['cursor-button', 'pointer'],
      ['cursor-disabled', 'default'],
      ['cursor-checkbox', 'pointer', 0.2],
      ['notify', 'pointer', 0.2],
      ['level', 'pointer'],
      ['cursor-radio', 'pointer', 0.2],
      ['cursor-link', 'pointer'],
      ['cursor-text', 'text'],
      ['cursor-plain', 'default'],
      ['cursor-hidden', 'default'],
      ['cursor-grip', 'ew-resize'],
      ['press', 'pointer'],
      ['probe', 'move'],
    ];

    for (const [nodeId, cursor, fx] of cases) {
      await hover(page, nodeId, fx ?? 0.5);
      await expect.poll(async () => await canvasCursor(page), { message: nodeId }).toBe(cursor);
    }
    await hover(page, 'cursor-showcase', 0.95, 0.5);
    await expect.poll(async () => await canvasCursor(page)).toBe('default');
    expect(errors).toEqual([]);
  });

  test('a scroll bar thumb grabs, and a captured drag keeps its cursor outside the node', async ({ page }) => {
    await openWorkbench(page);
    const box = await boundsOf(page, 'cursor-scroll');
    const canvas = await page.locator('#fui-canvas').boundingBox();
    const thumb = { x: (canvas?.x ?? 0) + box.x + box.width - 4, y: (canvas?.y ?? 0) + box.y + 12 };

    await page.mouse.move(thumb.x, thumb.y);
    await expect.poll(async () => await canvasCursor(page)).toBe('grab');
    await page.mouse.down();
    await expect.poll(async () => await canvasCursor(page)).toBe('grabbing');
    await page.mouse.move(thumb.x + 60, thumb.y + 20);
    expect(await canvasCursor(page)).toBe('grabbing');
    await page.mouse.up();
    await expect.poll(async () => await canvasCursor(page)).toBe('default');

    const panel = await boundsOf(page, 'cursor-panel');
    await hover(page, 'cursor-grip');
    await page.mouse.down();
    const grip = await pointIn(page, 'cursor-grip');
    await page.mouse.move(grip.x + 30, grip.y + 40, { steps: 4 });
    // Captured by the grip, the pointer keeps the resize cursor away from it.
    expect(await canvasCursor(page)).toBe('ew-resize');
    await page.mouse.up();
    await expect.poll(async () => (await boundsOf(page, 'cursor-panel')).width).toBe(panel.width + 30);
  });

  test('a coarse pointer leaves the cursor alone', async ({ browser }) => {
    const context = await browser.newContext({ hasTouch: true, isMobile: true, viewport: { width: 1024, height: 700 } });
    const page = await context.newPage();
    try {
      await page.goto(`${baseUrl()}/v2/fui-cs/demo/workbench/`);
      await waitForReady(page);
      expect(await page.evaluate(() => matchMedia('(pointer: coarse)').matches)).toBe(true);

      await hover(page, 'cursor-button');
      await page.evaluate(async () => await new Promise((resolve) => requestAnimationFrame(() => requestAnimationFrame(resolve))));
      await hover(page, 'cursor-text');
      await page.evaluate(async () => await new Promise((resolve) => requestAnimationFrame(() => requestAnimationFrame(resolve))));

      expect(await canvasCursor(page)).toBe('');
    } finally {
      await context.close();
    }
  });
});

async function toolTipShown(page: Page): Promise<boolean> {
  const node = await hostNode(page, 'fui-tooltip');
  return node !== null && node.visibleBounds.width > 0 && node.visibleBounds.height > 0;
}

test.describe('Workbench tooltips and browser tooling', () => {
  test('resting on a button shows its tooltip at the pointer after the delay; leaving or pressing hides it', async ({ page }) => {
    const errors = await openWorkbench(page);
    const point = await pointIn(page, 'press', 0.3, 0.5);

    await page.mouse.move(point.x, point.y);
    await page.waitForTimeout(300);
    expect(await toolTipShown(page)).toBe(false);
    await expect.poll(async () => await toolTipShown(page)).toBe(true);
    expect(await labelOf(page, 'fui-tooltip-text')).toBe('Counts each press');
    const canvas = await page.locator('#fui-canvas').boundingBox();
    const tip = await boundsOf(page, 'fui-tooltip');
    expect(Math.abs(tip.x - (point.x - (canvas?.x ?? 0)))).toBeLessThanOrEqual(1);
    await page.screenshot({ path: screenshotPath('fui-cs-workbench-tooltip.png') });

    await page.mouse.down();
    await expect.poll(async () => await toolTipShown(page)).toBe(false);
    await page.mouse.up();
    await page.mouse.move(point.x, point.y + 200);
    await page.mouse.move(point.x, point.y);
    await expect.poll(async () => await toolTipShown(page)).toBe(true);
    await page.mouse.move(point.x, point.y + 200);
    await expect.poll(async () => await toolTipShown(page)).toBe(false);
    expect(errors).toEqual([]);
  });

  test('keyboard focus shows the tooltip above its button', async ({ page }) => {
    await openWorkbench(page);
    await page.mouse.move(5, 5);

    await page.keyboard.press('Tab');
    await page.keyboard.press('Tab');

    await expect.poll(async () => await toolTipShown(page)).toBe(true);
    const button = await boundsOf(page, 'press');
    const tip = await boundsOf(page, 'fui-tooltip');
    expect(tip.y + tip.height).toBeLessThanOrEqual(button.y);
    expect(tip.x).toBeCloseTo(button.x, 0);
    // Focus moves on, and the tooltip goes with it rather than waiting out its five seconds.
    await page.keyboard.press('Tab');
    await expect.poll(async () => await toolTipShown(page), { timeout: 1_500 }).toBe(false);
  });

  test('scrolling hides a pointer tooltip', async ({ page }) => {
    await openWorkbench(page);
    const point = await pointIn(page, 'cursor-line-1', 0.5, 0.5);
    await page.mouse.move(point.x, point.y);
    await expect.poll(async () => await toolTipShown(page)).toBe(true);
    expect(await labelOf(page, 'fui-tooltip-text')).toBe('The first line');

    await page.mouse.wheel(0, 60);

    await expect.poll(async () => await toolTipShown(page)).toBe(false);
  });

  test('find on page finds the route\'s text and highlights the active match', async ({ page }) => {
    await openWorkbench(page);
    // A point in the title's line box below the glyphs' baseline, where the highlight shows over the page.
    const before = await pixelIn(page, 'workbench-title', 0.5, 0.95);

    const state = await page.evaluate(() => {
      const api = window.__OPEN_CANVAS_API__;
      if (api === undefined) throw new Error('The Open Canvas API is missing.');
      const results = api.findText('Workbench');
      const applied = api.setFindState({ ...results, activeMatchIndex: 0 }, true);
      const lengths = results.matches.map((match) => match.end - match.start);
      return { lengths, applied, active: api.getFindState()?.activeMatchIndex ?? -1 };
    });
    expect(state.lengths.length).toBeGreaterThanOrEqual(1);
    expect(state.lengths[0]).toBe('Workbench'.length);
    expect(state.applied).toBe(true);
    expect(state.active).toBe(0);
    await expect.poll(async () => distance(await pixelIn(page, 'workbench-title', 0.5, 0.95), before)).toBeGreaterThan(40);

    expect(await page.evaluate(() => window.__OPEN_CANVAS_API__?.setFindState(null) ?? false)).toBe(true);
    await expect.poll(async () => distance(await pixelIn(page, 'workbench-title', 0.5, 0.95), before)).toBeLessThan(10);
    expect(await page.evaluate(() => window.__OPEN_CANVAS_API__?.getFindState() ?? null)).toBeNull();
  });
});

test.describe('Workbench assets', () => {
  test('route-relative assets load, missing ones fail, and a texture asset draws', async ({ page }) => {
    const errors = await openWorkbench(page);

    await expect.poll(async () => await labelOf(page, 'asset-image-state')).toBe('Image ready');
    await expect.poll(async () => await labelOf(page, 'asset-svg-state')).toBe('SVG ready');
    await expect.poll(async () => await labelOf(page, 'asset-missing-image-state')).toBe('Missing image failed');
    await expect.poll(async () => await labelOf(page, 'asset-missing-svg-state')).toBe('Missing SVG failed');
    // The halves image is red on the left and blue on the right, shown as an image and drawn from a texture asset.
    for (const nodeId of ['asset-image', 'asset-drawing']) {
      await expect.poll(async () => rgb(await pixelIn(page, nodeId, 0.25, 0.5)), { message: nodeId }).toBe(0xE11D48);
      expect(rgb(await pixelIn(page, nodeId, 0.75, 0.5))).toBe(0x2563EB);
    }
    await page.screenshot({ path: screenshotPath('fui-cs-workbench-assets.png') });
    expect(errors).toEqual([]);
  });

  test('an image\'s context menu opens its route-relative URL', async ({ page, context }) => {
    await openWorkbench(page);
    await expect.poll(async () => await labelOf(page, 'asset-image-state')).toBe('Image ready');
    const point = await pointIn(page, 'asset-image', 0.5, 0.5);
    await page.mouse.move(point.x, point.y);
    await page.mouse.click(point.x, point.y, { button: 'right' });
    const newTab = page.getByRole('button', { name: 'New Tab', exact: true });
    await expect(newTab).toHaveCount(1);
    const box = await newTab.boundingBox();
    if (box === null) throw new Error('Expected the New Tab item.');

    await page.mouse.move(box.x + 20, box.y + (box.height / 2));
    const opened = context.waitForEvent('page');
    await page.mouse.down();
    await page.mouse.up();

    expect(new URL((await opened).url()).pathname).toBe('/v2/fui-cs/demo/workbench/assets/halves.png');
  });
});
