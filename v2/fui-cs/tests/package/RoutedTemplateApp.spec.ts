// An application created from the fui-routed-app template and published against the packages
// (scripts/test-package.sh sets FUICS_ROUTED_PACKAGE_SITE to its publish folder): each route loads at its own
// address, and the links swap routes in place.

import { expect, test, type Page } from '@playwright/test';

import { clickLabel, collectErrors, labels, recordLoadingDetails, serveSite, waitForReady } from './package-support.ts';

const baseUrl = serveSite('FUICS_ROUTED_PACKAGE_SITE', 11_395);

async function pathname(page: Page): Promise<string> {
  return await page.evaluate(() => window.location.pathname);
}

test('the routed template loads each route at its address and links between them', async ({ page }) => {
  const errors = collectErrors(page);
  await page.goto(`${baseUrl()}/`);
  await waitForReady(page);
  await expect.poll(async () => await labels(page)).toContain('Clicked 0 times');
  // Each page is titled by its route, and the title follows the route as the links swap it in place.
  expect(await page.title()).toBe('Home');

  await clickLabel(page, 'Settings');
  await expect.poll(async () => await pathname(page)).toBe('/settings/');
  await waitForReady(page);
  await expect.poll(async () => await labels(page)).toContain('Notifications are on');
  expect(await page.title()).toBe('Settings');
  await clickLabel(page, 'Notifications');
  await expect.poll(async () => await labels(page)).toContain('Notifications are off');

  await page.goBack();
  await expect.poll(async () => await pathname(page)).toBe('/');
  await waitForReady(page);
  await expect.poll(async () => await labels(page)).toContain('Clicked 0 times');
  expect(await page.title()).toBe('Home');

  // A route's address loads it directly.
  await page.goto(`${baseUrl()}/settings/`);
  expect(await page.title()).toBe('Settings');
  await waitForReady(page);
  await expect.poll(async () => await labels(page)).toContain('Notifications are on');
  expect(errors).toEqual([]);
});

test('the loading overlay shows the runtime\'s progress, then names the application\'s WebAssembly', async ({ page }) => {
  const errors = collectErrors(page);
  const loadingDetails = await recordLoadingDetails(page);
  await page.goto(`${baseUrl()}/`);
  await waitForReady(page);

  const details = await loadingDetails();
  expect(details.some((detail) => /^Runtime assets \d+ \/ \d+$/u.test(detail)), details.join(' | ')).toBe(true);
  expect(details.some((detail) => /^Built-in fonts \d+ \/ \d+$/u.test(detail)), details.join(' | ')).toBe(true);
  expect(details).toContain('Loading RoutedTemplateApp.wasm');
  expect(details.filter((detail) => detail.includes('data:'))).toEqual([]);
  expect(errors).toEqual([]);
});
