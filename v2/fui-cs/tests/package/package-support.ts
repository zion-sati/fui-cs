// Helpers for the packaged-application checks: serving a published site and reading and driving its page.

import { expect, test, type Page } from '@playwright/test';

import { startStaticServer, type StaticServerHandle } from '../static_server.ts';

interface DebugNode {
  readonly semanticLabel: string;
  readonly bounds: { readonly x: number; readonly y: number; readonly width: number; readonly height: number };
}

interface DebugWindow {
  __fuiReady?: boolean;
  __fuiError?: string;
  __fui_debug?: { getDebugTree(): Promise<{ readonly nodes: readonly DebugNode[] }> };
}

/** Serves the site scripts/test-package.sh published to the folder in `variable`, skipping the file without it. */
export function serveSite(variable: string, port: number): () => string {
  const site = process.env[variable];
  let server: StaticServerHandle | undefined;
  test.skip(site === undefined, `scripts/test-package.sh sets ${variable}.`);
  test.beforeAll(async () => {
    if (site !== undefined) server = await startStaticServer(site, port);
  });
  test.afterAll(async () => {
    await server?.close();
  });
  return () => `http://127.0.0.1:${String(server?.port)}`;
}

/** Collects the page's errors: uncaught exceptions and console errors. */
export function collectErrors(page: Page): string[] {
  const errors: string[] = [];
  page.on('pageerror', (error) => errors.push(String(error)));
  page.on('console', (message) => {
    if (message.type() === 'error') errors.push(message.text());
  });
  return errors;
}

export async function waitForReady(page: Page): Promise<void> {
  await expect.poll(async () => await page.evaluate(() => {
    const state = window as DebugWindow;
    return state.__fuiError ?? (state.__fuiReady === true ? 'ready' : 'pending');
  })).toBe('ready');
  // The runtime's loading overlay covers the page, and takes clicks, until it fades out after the first frames.
  await expect(page.locator('#effindom-loading-overlay')).toHaveCount(0);
}

async function nodes(page: Page): Promise<readonly DebugNode[]> {
  return await page.evaluate(async () => {
    const debug = (window as DebugWindow).__fui_debug;
    if (debug === undefined) throw new Error('The harness debug API is not installed.');
    return (await debug.getDebugTree()).nodes;
  });
}

export async function labels(page: Page): Promise<string[]> {
  return (await nodes(page)).map((node) => node.semanticLabel);
}

/** Clicks the middle of the laid-out node labelled `label`, in page coordinates once offset by the canvas. */
export async function clickLabel(page: Page, label: string): Promise<void> {
  await expect.poll(async () => (await nodes(page)).some((node) => node.semanticLabel === label && node.bounds.width > 0)).toBe(true);
  const node = (await nodes(page)).find((candidate) => candidate.semanticLabel === label && candidate.bounds.width > 0);
  const canvas = await page.locator('#fui-canvas').boundingBox();
  if (node === undefined || canvas === null) throw new Error(`No laid-out node is labelled "${label}".`);
  await page.mouse.click(canvas.x + node.bounds.x + node.bounds.width / 2, canvas.y + node.bounds.y + node.bounds.height / 2);
}
