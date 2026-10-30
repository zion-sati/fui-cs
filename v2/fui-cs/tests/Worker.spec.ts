import { expect, test } from '@playwright/test';

import { clickNode, labelOf, openTestPage, serveApp } from './support.js';

const baseUrl = serveApp();

test.describe('Workers through NetWasm', () => {
  test('a worker reports progress, then completes with its result', async ({ page }) => {
    await openTestPage(page, baseUrl(), 'worker');

    await clickNode(page, 'count-start');

    await expect.poll(async () => await labelOf(page, 'count-log'), { timeout: 15000 }).toBe('completed counted 5');
    expect(await labelOf(page, 'count-progress')).toBe('5');
  });

  test('a failure, an exception, a job without a result and an unknown entry each raise an error', async ({ page }) => {
    await openTestPage(page, baseUrl(), 'worker');

    for (const id of ['fail', 'throws', 'silent', 'missing']) {
      await clickNode(page, `${id}-start`);
    }

    await expect.poll(async () => await labelOf(page, 'fail-log'), { timeout: 15000 }).toBe('error failed: bad input');
    await expect.poll(async () => await labelOf(page, 'throws-log'), { timeout: 15000 }).toBe('error thrown: boom');
    await expect.poll(async () => await labelOf(page, 'silent-log'), { timeout: 15000 })
      .toBe('error The worker job returned without calling WorkerRuntime.Complete(...) or WorkerRuntime.Fail(...).');
    await expect.poll(async () => await labelOf(page, 'missing-log'), { timeout: 15000 }).toBe('error The worker has no entry named missing.');
  });

  test("a worker calls its own host services, each value type crossing the worker boundary", async ({ page }) => {
    await openTestPage(page, baseUrl(), 'worker');

    await clickNode(page, 'services-start');

    await expect.poll(async () => await labelOf(page, 'services-log'), { timeout: 15000 })
      .toBe('completed hello:-9007199254740993:1,2,3:true|3,-4|3,2,1|in worker');
  });

  test('a worker fetches from inside the worker: a response, a missing resource and an unreachable server', async ({ page }) => {
    const errors = await openTestPage(page, baseUrl(), 'worker');

    for (const id of ['fetch-found', 'fetch-missing', 'fetch-unreachable']) {
      await clickNode(page, `${id}-start`);
    }

    await expect.poll(async () => await labelOf(page, 'fetch-found-log'), { timeout: 15000 }).toBe('completed ok 200');
    await expect.poll(async () => await labelOf(page, 'fetch-missing-log'), { timeout: 15000 }).toBe('completed not ok 404');
    await expect.poll(async () => await labelOf(page, 'fetch-unreachable-log'), { timeout: 15000 }).toBe('error fetch failed');
    // The browser logs the missing resource's 404 and the refused connection; nothing else may.
    expect(errors.filter((message) => !message.includes('status of 404') && !message.includes('ERR_CONNECTION_REFUSED'))).toEqual([]);
  });

  test('cancelling a running job stops it where it is', async ({ page }) => {
    await openTestPage(page, baseUrl(), 'worker');

    await clickNode(page, 'slow-start');
    await expect.poll(async () => Number(await labelOf(page, 'slow-progress')), { timeout: 15000 }).toBeGreaterThanOrEqual(3);
    await clickNode(page, 'slow-cancel');

    await expect.poll(async () => await labelOf(page, 'slow-log'), { timeout: 15000 }).toMatch(/^error cancelled at \d+$/);
    const stoppedAt = Number((await labelOf(page, 'slow-log'))?.split(' ').pop());
    expect(stoppedAt).toBeGreaterThanOrEqual(3);
    expect(stoppedAt).toBeLessThan(100);
  });

  test('a worker that cannot be loaded raises an error', async ({ page }) => {
    const errors = await openTestPage(page, baseUrl(), 'worker');

    await clickNode(page, 'absent-start');

    await expect.poll(async () => await labelOf(page, 'absent-log'), { timeout: 15000 }).toMatch(/^error /);
    // The browser logs the failed module load (a 404 that does not name the URL); nothing else may.
    expect(errors.filter((message) => !message.includes('NoSuchWorker') && !message.includes('status of 404'))).toEqual([]);
  });
});
