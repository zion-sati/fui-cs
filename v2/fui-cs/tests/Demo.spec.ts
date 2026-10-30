import { expect, test, type Page } from '@playwright/test';

import type {} from '../demo/host/shell-state.ts';
import { clickNode, debugNodes, hostNode, labelStartingWith, screenshotPath, serveApp, waitForReady } from './support.js';

const baseUrl = serveApp();

interface ManagerState {
  readonly routePath: string;
  readonly modulePath: string;
  readonly routeLoads: Readonly<Record<string, number>>;
}

async function managerState(page: Page): Promise<ManagerState | undefined> {
  return await page.evaluate(() => (window as Window & { __fuiManagerState?: ManagerState }).__fuiManagerState);
}

async function openDemo(page: Page, path: string): Promise<string[]> {
  const errors: string[] = [];
  page.on('pageerror', (error) => errors.push(error.message));
  page.on('console', (message) => {
    // The workbench route's asset showcase loads two assets that do not exist on purpose.
    const expectedFailure = message.text().includes('assets/missing.') || message.location().url.includes('/assets/missing.');
    if (message.type() === 'error' && !expectedFailure) errors.push(message.text());
  });
  await page.setViewportSize({ width: 1024, height: 700 });
  await page.goto(`${baseUrl()}${path}`);
  await waitForReady(page);
  return errors;
}

function route(key: string): string {
  return `/v2/fui-cs/demo/${key}/`;
}

/** Clicks the center of the node whose semantic label is `label`. */
async function clickLabel(page: Page, label: string): Promise<void> {
  const node = (await debugNodes(page)).find((candidate) => candidate.semanticLabel === label);
  if (node === undefined) throw new Error(`No node is labelled "${label}".`);
  const canvas = await page.locator('#fui-canvas').boundingBox();
  if (canvas === null) throw new Error('Expected the fui-cs canvas.');
  await page.mouse.click(canvas.x + node.bounds.x + (node.bounds.width / 2), canvas.y + node.bounds.y + (node.bounds.height / 2));
}

async function expectRoute(page: Page, routePath: string, nodeId: string): Promise<void> {
  await expect.poll(async () => (await managerState(page))?.routePath).toBe(routePath);
  await expect.poll(async () => (await hostNode(page, nodeId)) !== null).toBe(true);
  expect(new URL(page.url()).pathname).toBe(routePath);
}

test.describe('Routed demo shell', () => {
  test('the shell boots the dashboard route from its own module', async ({ page }) => {
    const errors = await openDemo(page, '/v2/fui-cs/demo/');

    await expectRoute(page, '/v2/fui-cs/demo/', 'demo-key-target');
    await expect.poll(async () => await page.title()).toBe('EffinDOM FUI-CS Demo • Dashboard');
    expect((await managerState(page))?.modulePath).toBe('/v2/fui-cs/demo/browser/FuiCs.Demo.Home.mjs');
    await page.screenshot({ path: screenshotPath('fui-cs-demo-home.png') });
    expect(errors).toEqual([]);
  });

  test('a NavLink swaps in the advanced controls application, and history returns to the dashboard', async ({ page }) => {
    const errors = await openDemo(page, '/v2/fui-cs/demo/');
    await expectRoute(page, '/v2/fui-cs/demo/', 'demo-key-target');

    await clickLabel(page, 'Advanced controls');
    await expectRoute(page, route('advanced-controls'), 'advanced-controls:animation-scroll');
    expect(await hostNode(page, 'demo-key-target')).toBeNull();
    await clickLabel(page, 'Ping advanced controls');
    await expect.poll(async () => await labelStartingWith(page, 'Advanced actions: ')).toBe('Advanced actions: 1');

    await page.goBack();
    await waitForReady(page);
    await expectRoute(page, '/v2/fui-cs/demo/', 'demo-key-target');
    await clickLabel(page, 'Advanced controls');
    await expectRoute(page, route('advanced-controls'), 'advanced-controls:animation-scroll');
    // A fresh application each time: the counter starts over, and the shell counted both loads.
    expect(await labelStartingWith(page, 'Advanced actions: ')).toBe('Advanced actions: 0');
    expect((await managerState(page))?.routeLoads[route('advanced-controls')]).toBe(2);
    expect(errors).toEqual([]);
  });

  test('the published route table lists each route at its folder under the demo base', async ({ page }) => {
    await openDemo(page, '/v2/fui-cs/demo/');

    const routes = await page.evaluate(() => {
      const table = (window as Window & { __fuiCsRoutes?: { routeBase: string; routes: { routePath: string; modulePath: string }[] } }).__fuiCsRoutes;
      return { base: table?.routeBase, routes: (table?.routes ?? []).map((route) => `${route.routePath} ${route.modulePath}`) };
    });

    expect(routes.base).toBe('/v2/fui-cs/demo/');
    for (const [key, module] of [['', 'browser/FuiCs.Demo.Home.mjs'], ['workbench/', 'workbench/FuiCs.Demo.Workbench.mjs'], ['immediate-drawing/', 'immediate-drawing/FuiCs.Demo.ImmediateDrawing.mjs'], ['scrollbar-gutter/', 'scrollbar-gutter/FuiCs.Demo.ScrollbarGutter.mjs'], ['templated-controls/', 'templated-controls/FuiCs.Demo.TemplatedControls.mjs'], ['advanced-controls/', 'advanced-controls/FuiCs.Demo.AdvancedControls.mjs']]) {
      expect(routes.routes).toContain(`/v2/fui-cs/demo/${key} /v2/fui-cs/demo/${module}`);
    }
  });

  test('an index.html address loads the route it names', async ({ page }) => {
    await openDemo(page, '/v2/fui-cs/demo/index.html');
    await expect.poll(async () => (await managerState(page))?.routePath).toBe('/v2/fui-cs/demo/');
    await expect.poll(async () => (await hostNode(page, 'demo-key-target')) !== null).toBe(true);

    await page.goto(`${baseUrl()}/v2/fui-cs/demo/advanced-controls/index.html`);
    await waitForReady(page);
    await expect.poll(async () => (await managerState(page))?.routePath).toBe(route('advanced-controls'));
    await expect.poll(async () => (await hostNode(page, 'advanced-controls:animation-scroll')) !== null).toBe(true);
    expect((await managerState(page))?.modulePath).toBe('/v2/fui-cs/demo/advanced-controls/FuiCs.Demo.AdvancedControls.mjs');
  });

  test('a retained NavLink hands its route to the shell instead of loading a page', async ({ page }) => {
    const errors = await openDemo(page, route('templated-controls'));
    await expectRoute(page, route('templated-controls'), 'templated-controls:house-switch');
    await page.evaluate(() => { (window as Window & { __pageMarker?: number }).__pageMarker = 1; });

    await clickLabel(page, 'Dashboard');

    await waitForReady(page);
    await expectRoute(page, '/v2/fui-cs/demo/', 'demo-key-target');
    // The same page ran the swap: the marker survives, and the shell loaded the dashboard once.
    expect(await page.evaluate(() => (window as Window & { __pageMarker?: number }).__pageMarker)).toBe(1);
    expect((await managerState(page))?.routeLoads['/v2/fui-cs/demo/']).toBe(1);
    expect(errors).toEqual([]);
  });

  test('Back restores a route page\'s persisted switch and page scroll', async ({ page }) => {
    const route = '/v2/fui-cs/demo/templated-controls/';
    const errors = await openDemo(page, route);
    await expectRoute(page, route, 'templated-controls:house-switch');
    const anchor = async (): Promise<number> => (await hostNode(page, 'templated-controls:house-checkbox'))?.bounds.y ?? Number.NaN;
    const top = await anchor();
    await page.mouse.move(16, 16);
    // Far enough that the switch is on screen.
    await page.mouse.wheel(0, 600);
    await expect.poll(anchor).toBeLessThan(top - 400);
    // The page scrolls smoothly: wait for it to come to rest.
    let scrolled = Number.NaN;
    await expect.poll(async () => {
      const y = await anchor();
      const settled = y === scrolled;
      scrolled = y;
      return settled;
    }, { intervals: [200] }).toBe(true);
    const toggle = page.getByRole('switch', { name: 'House style quick actions' });
    await expect(toggle).toHaveAttribute('aria-checked', 'true');
    await clickNode(page, 'templated-controls:house-switch');
    await expect(toggle).toHaveAttribute('aria-checked', 'false');

    await page.evaluate(async () => {
      await (window as Window & { __fui_debug?: { navigateTo(path: string): Promise<void> } }).__fui_debug?.navigateTo('/v2/fui-cs/demo/');
    });
    await waitForReady(page);
    await expectRoute(page, '/v2/fui-cs/demo/', 'demo-key-target');
    await page.goBack();
    await waitForReady(page);
    await expectRoute(page, route, 'templated-controls:house-switch');

    await expect.poll(anchor).toBe(scrolled);
    await expect(toggle).toHaveAttribute('aria-checked', 'false');
    expect((await managerState(page))?.routeLoads[route]).toBe(2);
    expect(errors).toEqual([]);
  });

  test('the dashboard follows viewport changes', async ({ page }) => {
    const errors = await openDemo(page, '/v2/fui-cs/demo/');
    await expectRoute(page, '/v2/fui-cs/demo/', 'demo-key-target');

    await expect.poll(async () => await labelStartingWith(page, 'Viewport: ')).toBe('Viewport: 1024 x 700');
    await page.setViewportSize({ width: 900, height: 640 });
    await expect.poll(async () => await labelStartingWith(page, 'Viewport: ')).toBe('Viewport: 900 x 640');
    expect(errors).toEqual([]);
  });
});

/** Moves the shell's clock, accent color and dark mode, as the shell's own state changes would. */
async function setShellState(page: Page, tick: number, accentColor: number, darkMode: boolean): Promise<void> {
  await page.evaluate(([nextTick, nextAccent, nextDark]) => {
    const shell = window.__fuiDemoShell;
    if (shell === undefined) throw new Error('The demo shell controls are missing.');
    shell.setTick(nextTick);
    shell.setAccentColor(nextAccent);
    shell.setDarkMode(nextDark);
  }, [tick, accentColor, darkMode] as const);
}

/** The page pixel at (x, y) as 0xRRGGBB. */
async function pagePixel(page: Page, x: number, y: number): Promise<number> {
  const png = await page.screenshot({ clip: { x, y, width: 1, height: 1 } });
  return await page.evaluate(async (base64) => {
    const bytes = Uint8Array.from(atob(base64), (character) => character.charCodeAt(0));
    const bitmap = await createImageBitmap(new Blob([bytes], { type: 'image/png' }));
    const context = new OffscreenCanvas(1, 1).getContext('2d');
    if (context === null) throw new Error('Expected a 2D context.');
    context.drawImage(bitmap, 0, 0);
    const data = context.getImageData(0, 0, 1, 1).data;
    return ((data.at(0) ?? 0) << 16) | ((data.at(1) ?? 0) << 8) | (data.at(2) ?? 0);
  }, png.toString('base64'));
}

test.describe('Generated host services and events in the demo', () => {
  test('the demo follows the browser\'s color scheme, at load and when it changes', async ({ page }) => {
    await page.emulateMedia({ colorScheme: 'dark' });
    const errors = await openDemo(page, '/v2/fui-cs/demo/');
    await expectRoute(page, '/v2/fui-cs/demo/', 'demo-key-target');

    // The page behind the panels: dark, then light once the browser switches schemes.
    await expect.poll(async () => await pagePixel(page, 4, 4)).toBeLessThan(0x404040);
    await page.emulateMedia({ colorScheme: 'light' });
    await expect.poll(async () => await pagePixel(page, 4, 4)).toBeGreaterThan(0xc0c0c0);
    expect(errors).toEqual([]);
  });

  test('the dashboard reads the shell through host services and follows its host events', async ({ page }) => {
    const errors = await openDemo(page, '/v2/fui-cs/demo/');
    await expectRoute(page, '/v2/fui-cs/demo/', 'demo-key-target');
    // The routes card's badge is painted in the theme's accent; sample it beside its label.
    const nodes = await debugNodes(page);
    const badgeLabel = nodes.find((node) => node.semanticLabel === 'Dashboard sample');
    const badge = nodes.find((node) => node.handle === badgeLabel?.parentHandle);
    if (badge === undefined) throw new Error('Expected the accent badge.');
    const swatchX = badge.bounds.x + badge.bounds.width - 20;
    const swatchY = badge.bounds.y + (badge.bounds.height / 2);

    // Services: the accent the shell reported when the page was built paints the badge.
    await expect.poll(async () => await pagePixel(page, swatchX, swatchY)).toBe(0x2563eb);
    // Events: the clock ticks once a second on its own.
    const first = await labelStartingWith(page, 'Tick ');
    await expect.poll(async () => await labelStartingWith(page, 'Tick '), { timeout: 5_000 }).not.toBe(first);

    await setShellState(page, 42, 0x16a34aff, true);
    await expect.poll(async () => await labelStartingWith(page, 'Tick ')).toMatch(/^Tick 0:42\b/);
    await expect.poll(async () => await pagePixel(page, swatchX, swatchY)).not.toBe(0x2563eb);
    // Dark mode darkens the page behind the panels.
    await expect.poll(async () => await pagePixel(page, 4, 4)).toBeLessThan(0x404040);
    expect(errors).toEqual([]);
  });
});
