import { expect, test } from '@playwright/test';

import { labelOf, openTestPage, serveApp } from './support.js';

const baseUrl = serveApp();

test.describe('Fetch through the host', () => {
  test('responses, failures and cancellation reach the awaiting C# code', async ({ page }) => {
    await openTestPage(page, baseUrl(), 'fetch');

    await expect.poll(async () => await labelOf(page, 'found-log')).toBe('ok 200');
    await expect.poll(async () => await labelOf(page, 'missing-log')).toBe('not ok 404');
    await expect.poll(async () => await labelOf(page, 'unreachable-log')).toBe('failed');
    await expect.poll(async () => await labelOf(page, 'cancelled-log')).toBe('cancelled');
  });
});
