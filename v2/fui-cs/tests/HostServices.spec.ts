import { expect, test, type Page } from '@playwright/test';

import type {} from '../browser/probe-host.ts';
import { clickNode, labelOf, openTestPage, serveApp } from './support.js';

const baseUrl = serveApp();

async function recordedCalls(page: Page): Promise<string[]> {
  return await page.evaluate(() => [...(window.__fuiProbe?.calls ?? [])]);
}

async function emitEverything(page: Page): Promise<void> {
  await page.evaluate(() => { window.__fuiProbe?.emitEverything(); });
}

async function ping(page: Page): Promise<void> {
  await page.evaluate(() => { window.__fuiProbe?.ping(); });
}

test.describe('Generated host bindings in the host', () => {
  test('C# calls every host-service type through its generated bindings', async ({ page }) => {
    const errors = await openTestPage(page, baseUrl(), 'host');

    await clickNode(page, 'run-services');

    await expect.poll(async () => await labelOf(page, 'services-log')).toBe([
      'Hello, C#',
      'false',
      '5',
      '4294967295',
      '-9007199254740993',
      '18446744073709551615',
      '2.5',
      '3,2,1',
      '-2,4',
      '4294967295,4294967294',
      '-9007199254740993',
      '18446744073709551615',
      '0.5,1.5',
    ].join('|'));
    expect(await recordedCalls(page)).toEqual(['done:7,8']);
    expect(errors).toEqual([]);
  });

  test('host events reach their generated C# events with every type', async ({ page }) => {
    const errors = await openTestPage(page, baseUrl(), 'host');

    await emitEverything(page);
    await ping(page);
    await ping(page);

    await expect.poll(async () => await labelOf(page, 'events-log')).toBe([
      'héllo ✓',
      'true',
      '-7',
      '4294967295',
      '-9007199254740993',
      '18446744073709551615',
      '0.25',
      '9,8',
      '-1,2',
      '4294967295',
      '-5',
      '18446744073709551615',
      '0.5',
    ].join('|'));
    await expect.poll(async () => await labelOf(page, 'ping-log')).toBe('2 pings');
    expect(errors).toEqual([]);
  });
});
