// An application created from the fui-app template and published against the packages (scripts/test-package.sh
// sets FUICS_PACKAGE_SITE to its publish folder) loads, renders and handles a click.

import { expect, test } from '@playwright/test';

import { clickLabel, collectErrors, labels, recordLoadingDetails, serveSite, waitForReady } from './package-support.ts';

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

test('a Release site is titled by its project, runs in release mode and fills the viewport', async ({ page }) => {
  await page.setViewportSize({ width: 390, height: 700 });
  await page.goto(`${baseUrl()}/`);
  await waitForReady(page);

  expect(await page.title()).toBe('TemplateApp');
  expect(await page.evaluate(() => (window as Window & { __effindomRuntime?: { buildMode?: string } }).__effindomRuntime?.buildMode)).toBe('release');
  // The canvas is fixed to the visible viewport, which on a phone is shorter than 100vh.
  const canvas = await page.evaluate(() => {
    const bounds = document.querySelector('canvas')?.getBoundingClientRect();
    return { width: bounds?.width, height: bounds?.height, viewportWidth: window.innerWidth, viewportHeight: window.innerHeight };
  });
  expect(canvas).toEqual({ width: 390, height: 700, viewportWidth: 390, viewportHeight: 700 });
});

test('the loading overlay shows the runtime\'s progress, then names the application\'s WebAssembly', async ({ page }) => {
  const errors = collectErrors(page);
  const loadingDetails = await recordLoadingDetails(page);
  await page.goto(`${baseUrl()}/`);
  await waitForReady(page);

  const details = await loadingDetails();
  expect(details.some((detail) => /^Runtime assets \d+ \/ \d+$/u.test(detail)), details.join(' | ')).toBe(true);
  expect(details.some((detail) => /^Built-in fonts \d+ \/ \d+$/u.test(detail)), details.join(' | ')).toBe(true);
  expect(details).toContain('Loading TemplateApp.wasm');
  expect(details.filter((detail) => detail.includes('data:'))).toEqual([]);
  expect(errors).toEqual([]);
});
