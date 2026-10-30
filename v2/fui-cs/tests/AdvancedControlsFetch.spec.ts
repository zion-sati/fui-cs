import { expect, test, type Page } from '@playwright/test';

import { boundsOf, clickNode, hostNode, labelOf, serveApp, waitForReady } from './support.js';

const baseUrl = serveApp();
const getUrl = 'https://jsonplaceholder.typicode.com/posts/1';
const postUrl = 'https://jsonplaceholder.typicode.com/posts';

async function openAdvancedControls(page: Page): Promise<void> {
  await page.setViewportSize({ width: 1024, height: 700 });
  await page.goto(`${baseUrl()}/v2/fui-cs/demo/advanced-controls/`);
  await waitForReady(page);
  await expect.poll(async () => await labelOf(page, 'fetch-status')).toBe('Fetch status: idle');
}

/** Wheels the route page, in its left padding, until the node sits inside the canvas and stops moving. */
async function scrollIntoView(page: Page, nodeId: string): Promise<void> {
  const canvas = await page.locator('#fui-canvas').boundingBox();
  if (canvas === null) throw new Error('Expected the fui-cs canvas.');
  for (let attempt = 0; attempt < 30; attempt += 1) {
    const bounds = await boundsOf(page, nodeId);
    const fits = bounds.y >= 48 && bounds.y + bounds.height <= canvas.height - 48;
    if (fits) {
      await page.waitForTimeout(120);
      if ((await boundsOf(page, nodeId)).y === bounds.y) return;
      continue;
    }
    await page.mouse.move(canvas.x + 16, canvas.y + 16);
    await page.mouse.wheel(0, Math.max(-720, Math.min(720, bounds.y - 220)));
    await page.waitForTimeout(80);
  }
  throw new Error(`Unable to scroll ${nodeId} into view.`);
}

async function isInteractive(page: Page, nodeId: string): Promise<boolean | null> {
  return (await hostNode(page, nodeId))?.behavior.interactive ?? null;
}

test.describe('Advanced controls online fetch', () => {
  test('a GET runs with both buttons disabled, then reports its completion metadata', async ({ page }) => {
    let release: () => void = () => undefined;
    const responded = new Promise<void>((resolve) => { release = resolve; });
    let requests = 0;
    await page.route(getUrl, async (route) => {
      requests += 1;
      await responded;
      await route.fulfill({ status: 200, contentType: 'application/json', body: '{"id":1}' });
    });
    await openAdvancedControls(page);
    await scrollIntoView(page, 'fetch-get');

    await clickNode(page, 'fetch-get');

    await expect.poll(async () => await labelOf(page, 'fetch-status')).toBe('Fetch status: running');
    expect(await labelOf(page, 'fetch-request')).toBe(`Latest request: GET ${getUrl}`);
    expect(await labelOf(page, 'fetch-result')).toBe('Latest result: waiting for JSONPlaceholder to respond...');
    await expect.poll(async () => await isInteractive(page, 'fetch-get')).toBe(false);
    expect(await isInteractive(page, 'fetch-post')).toBe(false);

    release();

    await expect.poll(async () => await labelOf(page, 'fetch-status')).toBe('Fetch status: complete');
    expect(await labelOf(page, 'fetch-result')).toMatch(new RegExp(`^Latest result: GET ${getUrl} -> ok=true • status 200 .* • resolved url ${getUrl}$`));
    await expect.poll(async () => await isInteractive(page, 'fetch-get')).toBe(true);
    expect(await isInteractive(page, 'fetch-post')).toBe(true);
    expect(requests).toBe(1);
  });

  test('a POST sends its method, content type and JSON body', async ({ page }) => {
    const sent: { method: string; contentType: string | undefined; body: string | null }[] = [];
    await page.route(postUrl, async (route) => {
      const request = route.request();
      sent.push({ method: request.method(), contentType: request.headers()['content-type'], body: request.postData() });
      await route.fulfill({ status: 201, contentType: 'application/json', body: '{"id":101}' });
    });
    await openAdvancedControls(page);
    await scrollIntoView(page, 'fetch-post');

    await clickNode(page, 'fetch-post');

    await expect.poll(async () => await labelOf(page, 'fetch-status')).toBe('Fetch status: complete');
    expect(await labelOf(page, 'fetch-request')).toBe(`Latest request: POST ${postUrl}`);
    expect(await labelOf(page, 'fetch-result')).toMatch(/ -> ok=true • status 201 /);
    expect(sent).toHaveLength(1);
    expect(sent[0]?.method).toBe('POST');
    expect(sent[0]?.contentType).toBe('application/json; charset=UTF-8');
    expect(JSON.parse(sent[0]?.body ?? 'null')).toEqual({
      title: 'EffinDom advanced-controls demo',
      body: 'Posting through the shipped Fetch API.',
      userId: 29,
    });
  });

  test('a request that fails reports an error and frees the buttons', async ({ page }) => {
    await page.route(getUrl, async (route) => { await route.abort('failed'); });
    await openAdvancedControls(page);
    await scrollIntoView(page, 'fetch-get');

    await clickNode(page, 'fetch-get');

    await expect.poll(async () => await labelOf(page, 'fetch-status')).toBe('Fetch status: error');
    expect(await labelOf(page, 'fetch-result')).toMatch(new RegExp(`^Latest result: GET ${getUrl} -> error • .+`));
    await expect.poll(async () => await isInteractive(page, 'fetch-get')).toBe(true);
  });
});
