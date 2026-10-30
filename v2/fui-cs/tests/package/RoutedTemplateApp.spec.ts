// An application created from the fui-routed-app template and published against the packages
// (scripts/test-package.sh sets FUICS_ROUTED_PACKAGE_SITE to its publish folder): each route loads at its own
// address, and the links swap routes in place.

import { expect, test, type Page } from '@playwright/test';

import { clickLabel, collectErrors, labels, serveSite, waitForReady } from './package-support.ts';

const baseUrl = serveSite('FUICS_ROUTED_PACKAGE_SITE', 11_395);

async function pathname(page: Page): Promise<string> {
  return await page.evaluate(() => window.location.pathname);
}

test('the routed template loads each route at its address and links between them', async ({ page }) => {
  const errors = collectErrors(page);
  await page.goto(`${baseUrl()}/`);
  await waitForReady(page);
  await expect.poll(async () => await labels(page)).toContain('Clicked 0 times');

  await clickLabel(page, 'Settings');
  await expect.poll(async () => await pathname(page)).toBe('/settings/');
  await waitForReady(page);
  await expect.poll(async () => await labels(page)).toContain('Notifications are on');
  await clickLabel(page, 'Notifications');
  await expect.poll(async () => await labels(page)).toContain('Notifications are off');

  await page.goBack();
  await expect.poll(async () => await pathname(page)).toBe('/');
  await waitForReady(page);
  await expect.poll(async () => await labels(page)).toContain('Clicked 0 times');

  // A route's address loads it directly.
  await page.goto(`${baseUrl()}/settings/`);
  await waitForReady(page);
  await expect.poll(async () => await labels(page)).toContain('Notifications are on');
  expect(errors).toEqual([]);
});
