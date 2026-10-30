// An application created from the fui-app template and published against the packages (scripts/test-package.sh
// sets FUICS_PACKAGE_SITE to its publish folder) loads, renders and handles a click.

import { expect, test } from '@playwright/test';

import { clickLabel, collectErrors, labels, serveSite, waitForReady } from './package-support.ts';

const baseUrl = serveSite('FUICS_PACKAGE_SITE', 11_390);

test('the template application loads from its packages and handles a click', async ({ page }) => {
  const errors = collectErrors(page);
  await page.goto(`${baseUrl()}/`);
  await waitForReady(page);

  await expect.poll(async () => await labels(page)).toContain('Clicked 0 times');
  await clickLabel(page, 'Click me');
  await expect.poll(async () => await labels(page)).toContain('Clicked once');
  expect(errors).toEqual([]);
});
