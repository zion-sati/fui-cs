import * as fs from 'node:fs';
import * as path from 'node:path';
import { fileURLToPath } from 'node:url';

import { expect, test } from '@playwright/test';

import { startStaticServer, type StaticServerHandle } from './static_server.js';

declare global {
  interface Window {
    __fuiCsReady?: boolean;
    __fuiCsError?: string;
    __fuiCsState?: {
      readonly commandWordCount: number;
      readonly commandWords: readonly number[];
      readonly rootHandle: string | null;
    };
  }
}

const __dirname = path.dirname(fileURLToPath(import.meta.url));
const PUBLIC_DIR = path.join(__dirname, '..', '..', '..', 'public');
const SCREENSHOT_DIR = path.join(__dirname, 'screenshots');

let server: StaticServerHandle;
let baseUrl: string;

function screenshotPath(name: string): string {
  fs.mkdirSync(SCREENSHOT_DIR, { recursive: true });
  return path.join(SCREENSHOT_DIR, name);
}

test.beforeAll(async () => {
  server = await startStaticServer(PUBLIC_DIR, 11_330);
  baseUrl = `http://127.0.0.1:${String(server.port)}`;
});

test.afterAll(async () => {
  await server.close();
});

test('renders the fui-cs smoke through the browser bridge', async ({ page }) => {
  const errors: string[] = [];
  page.on('pageerror', (error) => {
    errors.push(error.message);
  });

  await page.goto(`${baseUrl}/v2/fui-cs/index.html`);

  await expect.poll(async () => {
    return await page.evaluate(() => {
      if (window.__fuiCsError !== undefined) return `error:${window.__fuiCsError}`;
      return window.__fuiCsReady === true ? 'ready' : 'pending';
    });
  }).toBe('ready');

  if (errors.length > 0) {
    throw new Error(`Page errors: ${errors.join('; ')}`);
  }

  const state = await page.evaluate(() => window.__fuiCsState);
  expect(state).toBeDefined();
  if (state === undefined) throw new Error('Expected fui-cs state.');
  expect(state.commandWordCount).toBeGreaterThan(0);
  expect(state.rootHandle).not.toBeNull();

  await page.screenshot({ path: screenshotPath('fui-cs-smoke.png') });
});
