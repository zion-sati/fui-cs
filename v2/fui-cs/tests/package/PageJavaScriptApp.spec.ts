// An application created from the fui-routed-app template that calls the page's own JavaScript: the package test
// gives its shared project a JavaScript module (Shared/Host/shell.mjs, declared there only) that ticks once a second,
// and each route subscribes to the tick through [JSImport], ending the subscription when the route unloads. The
// module reaches every route, and swapping routes leaves one subscription.

import { expect, test, type Page } from '@playwright/test';

import { clickLabel, collectErrors, labels, serveSite, waitForReady } from './package-support.ts';

const baseUrl = serveSite('FUICS_PAGE_JAVASCRIPT_SITE', 11_396);

async function pathname(page: Page): Promise<string> {
  return await page.evaluate(() => window.location.pathname);
}

/** The tick the route's label shows, or -1 before it shows one. */
async function tick(page: Page, route: string): Promise<number> {
  const label = (await labels(page)).find((candidate) => candidate.startsWith(`${route} tick `));
  return label === undefined ? -1 : Number(label.slice(`${route} tick `.length));
}

async function subscriptions(page: Page): Promise<number> {
  return await page.evaluate(() =>
    (globalThis as { fuiShell?: { listeners: Set<unknown> } }).fuiShell?.listeners.size ?? -1);
}

test('routes call the page\'s JavaScript and end their subscriptions when they unload', async ({ page }) => {
  const errors = collectErrors(page);
  await page.goto(`${baseUrl()}/`);
  await waitForReady(page);
  await expect.poll(async () => await tick(page, 'Home'), { timeout: 10_000 }).toBeGreaterThan(0);

  await clickLabel(page, 'Settings');
  await expect.poll(async () => await pathname(page)).toBe('/settings/');
  await waitForReady(page);
  const settingsStart = await tick(page, 'Settings');
  await expect.poll(async () => await tick(page, 'Settings'), { timeout: 10_000 }).toBeGreaterThan(settingsStart);

  await clickLabel(page, 'Home');
  await expect.poll(async () => await pathname(page)).toBe('/');
  await waitForReady(page);
  const homeStart = await tick(page, 'Home');
  await expect.poll(async () => await tick(page, 'Home'), { timeout: 10_000 }).toBeGreaterThan(homeStart);

  // Each route ended its subscription as it unloaded, so only the current route's is left.
  expect(await subscriptions(page)).toBe(1);
  expect(errors).toEqual([]);
});
