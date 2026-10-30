// Each demo route is a micro-frontend: swapping routes must release the previous route's application and the runtime
// the shell replaced, so no WebAssembly memory outlives its route.

import { expect, test, type CDPSession, type Page } from '@playwright/test';

import { debugNodes, serveApp, waitForReady } from './support.js';

const baseUrl = serveApp();

/** The bytes of every live WebAssembly memory after a collection. The probe releases its own references. */
async function liveWasmBytes(cdp: CDPSession): Promise<number> {
  await cdp.send('HeapProfiler.collectGarbage');
  const prototype = await cdp.send('Runtime.evaluate', { expression: 'WebAssembly.Memory.prototype', objectGroup: 'memory-probe' });
  const prototypeId = prototype.result.objectId;
  if (prototypeId === undefined) throw new Error('Expected the WebAssembly.Memory prototype.');
  const memories = await cdp.send('Runtime.queryObjects', { prototypeObjectId: prototypeId, objectGroup: 'memory-probe' });
  const listId = memories.objects.objectId;
  if (listId === undefined) throw new Error('Expected the list of memories.');
  const total = await cdp.send('Runtime.callFunctionOn', {
    objectId: listId,
    functionDeclaration: 'function () { return this.reduce((sum, memory) => sum + memory.buffer.byteLength, 0); }',
    returnByValue: true,
  });
  await cdp.send('Runtime.releaseObjectGroup', { objectGroup: 'memory-probe' });
  return total.result.value as number;
}

async function clickLabel(page: Page, label: string): Promise<void> {
  const node = (await debugNodes(page)).find((candidate) => candidate.semanticLabel === label);
  if (node === undefined) throw new Error(`No node is labelled "${label}".`);
  const canvas = await page.locator('#fui-canvas').boundingBox();
  if (canvas === null) throw new Error('Expected the fui-cs canvas.');
  await page.mouse.click(canvas.x + node.bounds.x + (node.bounds.width / 2), canvas.y + node.bounds.y + (node.bounds.height / 2));
}

async function routePath(page: Page): Promise<string | undefined> {
  return await page.evaluate(() => (window as Window & { __fuiManagerState?: { readonly routePath: string } }).__fuiManagerState?.routePath);
}

// Open: each route visit still leaves about 1.2 MB of the route module's NetWasm state reachable after its close(), in
// module-level async generator contexts; the runtimes themselves are released. Runs again once NetWasm releases them.
test.fixme('swapping routes releases each route\'s application and the replaced runtime', async ({ page }) => {
  const cdp = await page.context().newCDPSession(page);
  await page.setViewportSize({ width: 1280, height: 800 });
  await page.goto(`${baseUrl()}/v2/fui-cs/demo/`);
  await waitForReady(page);
  const baseline = await liveWasmBytes(cdp);

  for (let swap = 0; swap < 3; swap += 1) {
    await clickLabel(page, 'Immediate-mode drawing');
    await expect.poll(async () => await routePath(page)).toBe('/v2/fui-cs/demo/immediate-drawing/');
    await waitForReady(page);
    await page.goBack();
    await expect.poll(async () => await routePath(page)).toBe('/v2/fui-cs/demo/');
    await waitForReady(page);
  }

  // Before the fix each round trip kept a whole runtime (about 70 MB) and the routes' applications alive.
  await expect.poll(async () => await liveWasmBytes(cdp), { timeout: 15_000 }).toBeLessThanOrEqual(baseline + (4 * 1024 * 1024));
});
