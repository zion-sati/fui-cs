// Shared Playwright helpers for the fui-cs browser test pages. Layout and semantics are read back from the
// EffinDom host's debug tree (window.__fui_debug), so the assertions check what the host laid out and painted,
// not what C# asked for.

import * as fs from 'node:fs';
import * as path from 'node:path';
import { fileURLToPath } from 'node:url';

import { expect, test, type Page } from '@playwright/test';

import { startStaticServer, type StaticServerHandle } from './static_server.js';

export interface Bounds {
  readonly x: number;
  readonly y: number;
  readonly width: number;
  readonly height: number;
}

export interface HostNode {
  readonly handle: string;
  readonly parentHandle: string | null;
  readonly nodeId: string;
  readonly nodeTypeName: string;
  readonly semanticRole: number;
  readonly semanticLabel: string;
  readonly bounds: Bounds;
  readonly visibleBounds: Bounds;
  readonly childHandles: readonly string[];
  readonly style: { readonly opacity: number };
  readonly behavior: { readonly interactive: boolean };
  readonly scroll: {
    readonly scrollProxyTargetHandle: string | null;
    readonly viewportWidth: number;
    readonly viewportHeight: number;
    readonly contentWidth: number;
    readonly contentHeight: number;
  };
}

interface DebugTreeSnapshot {
  readonly nodes: readonly HostNode[];
}

declare global {
  interface Window {
    __fuiReady?: boolean;
    __fuiError?: string;
  }
}

type DebugWindow = Window & { readonly __fui_debug?: { getDebugTree(): Promise<DebugTreeSnapshot> } };

const __dirname = path.dirname(fileURLToPath(import.meta.url));
const PUBLIC_DIR = path.join(__dirname, '..', '..', '..', 'public');
const SCREENSHOT_DIR = path.join(__dirname, 'screenshots');

/** Starts a static server over the published app for the spec file and returns its base URL getter. */
export function serveApp(): () => string {
  let server: StaticServerHandle | undefined;
  test.beforeAll(async () => {
    server = await startStaticServer(PUBLIC_DIR, 11_330);
  });
  test.afterAll(async () => {
    await server?.close();
  });
  return () => {
    if (server === undefined) throw new Error('The static server has not started.');
    return `http://127.0.0.1:${String(server.port)}`;
  };
}

export function screenshotPath(name: string): string {
  fs.mkdirSync(SCREENSHOT_DIR, { recursive: true });
  return path.join(SCREENSHOT_DIR, name);
}

export async function waitForReady(page: Page): Promise<void> {
  await expect.poll(async () => {
    return await page.evaluate(() => {
      if (window.__fuiError !== undefined) return `error:${window.__fuiError}`;
      return window.__fuiReady === true ? 'ready' : 'pending';
    });
  }).toBe('ready');
  await expect(page.locator('#effindom-loading-overlay')).toHaveCount(0);
}

/** Opens a test page and records page errors and console errors, which every spec expects to stay empty. */
export async function openTestPage(page: Page, baseUrl: string, name: string): Promise<string[]> {
  const errors: string[] = [];
  page.on('pageerror', (error) => {
    errors.push(error.message);
  });
  page.on('console', (message) => {
    if (message.type() === 'error') errors.push(message.text());
  });
  await page.goto(`${baseUrl}/v2/fui-cs/index.html?page=${name}`);
  await waitForReady(page);
  return errors;
}

/**
 * The host's node with this node id in the rendered tree, or null when there is none. Detached subtrees (a hidden
 * popup keeps its native nodes for reuse) are not part of the rendered tree; the application root is the parentless
 * node with the most descendants.
 */
export async function hostNode(page: Page, nodeId: string): Promise<HostNode | null> {
  return await page.evaluate(async (id) => {
    const debug = (window as DebugWindow).__fui_debug;
    if (debug === undefined) throw new Error('The harness debug API is not installed.');
    const tree = await debug.getDebugTree();
    const byHandle = new Map(tree.nodes.map((node) => [node.handle, node]));
    const rootOf = (node: HostNode): HostNode => {
      let current = node;
      for (let parent = current.parentHandle; parent !== null; parent = current.parentHandle) {
        const next = byHandle.get(parent);
        if (next === undefined) break;
        current = next;
      }
      return current;
    };
    const sizes = new Map<string, number>();
    for (const node of tree.nodes) {
      const root = rootOf(node).handle;
      sizes.set(root, (sizes.get(root) ?? 0) + 1);
    }
    let appRoot: string | null = null;
    for (const [handle, size] of sizes) {
      if (appRoot === null || size > (sizes.get(appRoot) ?? 0)) appRoot = handle;
    }
    const node = tree.nodes.find((candidate) => candidate.nodeId === id);
    return node !== undefined && rootOf(node).handle === appRoot ? node : null;
  }, nodeId);
}

export async function boundsOf(page: Page, nodeId: string): Promise<Bounds> {
  const node = await hostNode(page, nodeId);
  if (node === null) throw new Error(`No host node has the id "${nodeId}".`);
  return node.bounds;
}

/** The semantic label the host holds for the node, which for text is its content. */
export async function labelOf(page: Page, nodeId: string): Promise<string | null> {
  return (await hostNode(page, nodeId))?.semanticLabel ?? null;
}

/** Every node in the host's debug tree, including detached ones. */
export async function debugNodes(page: Page): Promise<readonly HostNode[]> {
  return await page.evaluate(async () => {
    const debug = (window as DebugWindow).__fui_debug;
    if (debug === undefined) throw new Error('The harness debug API is not installed.');
    return (await debug.getDebugTree()).nodes;
  });
}

/** The bounds of the node whose semantic label is `label`, or null when there is none. */
export async function boundsOfLabel(page: Page, label: string): Promise<Bounds | null> {
  const node = (await debugNodes(page)).find((candidate) => candidate.semanticLabel === label);
  return node?.bounds ?? null;
}

/** The first semantic label that starts with `prefix`, or null when there is none. */
export async function labelStartingWith(page: Page, prefix: string): Promise<string | null> {
  return (await debugNodes(page)).find((node) => node.semanticLabel.startsWith(prefix))?.semanticLabel ?? null;
}

/** A page coordinate inside the node, at the given fraction of its width and height. */
export async function pointIn(page: Page, nodeId: string, fx = 0.5, fy = 0.5): Promise<{ x: number; y: number }> {
  const canvas = await page.locator('#fui-canvas').boundingBox();
  if (canvas === null) throw new Error('Expected the fui-cs canvas.');
  const bounds = await boundsOf(page, nodeId);
  return { x: canvas.x + bounds.x + bounds.width * fx, y: canvas.y + bounds.y + bounds.height * fy };
}

/** Waits until the browser has drawn two more frames, so input already sent has been processed by the host. */
export async function nextFrames(page: Page): Promise<void> {
  await page.evaluate(async () => {
    await new Promise<void>((resolve) => {
      requestAnimationFrame(() => {
        requestAnimationFrame(() => { resolve(); });
      });
    });
  });
}

/**
 * Clicks the node the way a person does: the pointer arrives, then presses. The host reports the hover changes of
 * a move a frame later, so a press in the same frame as the move would reach the app before them.
 */
export async function clickNode(page: Page, nodeId: string, fx = 0.5, fy = 0.5): Promise<void> {
  const point = await pointIn(page, nodeId, fx, fy);
  await page.mouse.move(point.x, point.y);
  await nextFrames(page);
  await page.mouse.down();
  await page.mouse.up();
}

/** The RGBA of the canvas pixel at the node's center (or the given fraction), read from a screenshot. */
export async function pixelIn(page: Page, nodeId: string, fx = 0.5, fy = 0.5): Promise<readonly [number, number, number, number]> {
  const point = await pointIn(page, nodeId, fx, fy);
  const png = await page.screenshot({ clip: { x: Math.floor(point.x), y: Math.floor(point.y), width: 1, height: 1 } });
  return await page.evaluate(async (base64) => {
    const bytes = Uint8Array.from(atob(base64), (character) => character.charCodeAt(0));
    const bitmap = await createImageBitmap(new Blob([bytes], { type: 'image/png' }));
    const context = new OffscreenCanvas(1, 1).getContext('2d');
    if (context === null) throw new Error('Expected a 2D context.');
    context.drawImage(bitmap, 0, 0);
    const data = context.getImageData(0, 0, 1, 1).data;
    return [data.at(0) ?? 0, data.at(1) ?? 0, data.at(2) ?? 0, data.at(3) ?? 0] as const;
  }, png.toString('base64'));
}

/** The RGBA of the canvas pixel at an offset in pixels from the node's top-left corner. */
export async function pixelAt(page: Page, nodeId: string, x: number, y: number): Promise<readonly [number, number, number, number]> {
  const bounds = await boundsOf(page, nodeId);
  return await pixelIn(page, nodeId, (x + 0.5) / bounds.width, (y + 0.5) / bounds.height);
}

/** Packs an RGB pixel as 0xRRGGBB for compact expectations. */
export function rgb(pixel: readonly [number, number, number, number]): number {
  return (pixel[0] << 16) | (pixel[1] << 8) | pixel[2];
}
