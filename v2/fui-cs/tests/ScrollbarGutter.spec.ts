import { expect, test, type Page } from '@playwright/test';

import { debugNodes, hostNode, nextFrames, screenshotPath, serveApp, waitForReady } from './support.js';

const baseUrl = serveApp();

interface ScrollMetrics {
  readonly box: string;
  readonly viewport: string;
  readonly content: string;
}

/** The scroll box's own bounds, and the viewport and content sizes of the scroll view it drives, rounded to pixels. */
async function metrics(page: Page, nodeId: string): Promise<ScrollMetrics> {
  const nodes = await debugNodes(page);
  const box = nodes.find((node) => node.nodeId === nodeId);
  if (box === undefined) throw new Error(`No host node has the id "${nodeId}".`);
  const view = nodes.find((node) => node.handle === box.scroll.scrollProxyTargetHandle) ?? box;
  const r = Math.round;
  return {
    box: `${String(r(box.bounds.x))},${String(r(box.bounds.y))} ${String(r(box.bounds.width))}x${String(r(box.bounds.height))}`,
    viewport: `${String(r(view.scroll.viewportWidth))}x${String(r(view.scroll.viewportHeight))}`,
    content: `${String(r(view.scroll.contentWidth))}x${String(r(view.scroll.contentHeight))}`,
  };
}

async function openRoute(page: Page, width: number, height: number): Promise<void> {
  await page.setViewportSize({ width, height });
  await page.goto(`${baseUrl()}/v2/fui-cs/demo/scrollbar-gutter/`);
  await waitForReady(page);
  await expect.poll(async () => (await hostNode(page, 'innerBugScrollBox')) !== null).toBe(true);
}

/** Both boxes' metrics, once they have held still for a few frames: a box whose bars flip never settles. */
async function settledMetrics(page: Page): Promise<string> {
  const read = async (): Promise<string> => JSON.stringify([await metrics(page, 'outerBugScrollBox'), await metrics(page, 'innerBugScrollBox')]);
  let previous = '';
  for (let attempt = 0; attempt < 20; attempt += 1) {
    await nextFrames(page);
    const current = await read();
    if (current === previous) {
      for (let frame = 0; frame < 6; frame += 1) {
        await nextFrames(page);
        expect(await read()).toBe(current);
      }
      return current;
    }
    previous = current;
  }
  throw new Error(`The scroll boxes never settled: ${previous}`);
}

test.describe('Scrollbar gutter route', () => {
  test('the route sets its caption and builds both scroll boxes', async ({ page }) => {
    await openRoute(page, 1024, 700);

    await expect.poll(async () => await page.title()).toBe('EffinDOM FUI-CS Demo • Scrollbar Gutter');
    expect(await hostNode(page, 'outerBugScrollBox')).not.toBeNull();
    await page.screenshot({ path: screenshotPath('fui-cs-scrollbar-gutter.png') });
  });

  // The reference's metrics at each size: no outer bars, then the outer vertical bar only, then both outer bars. The
  // inner box always shows its vertical bar, whose gutter takes 12 px from its viewport.
  for (const [width, height, outer, inner] of [
    [1024, 700, { box: '0,0 1024x700', viewport: '1024x700', content: '1024x700' }, { box: '24,36 976x140', viewport: '964x140', content: '964x308' }],
    [900, 560, { box: '0,0 900x560', viewport: '888x560', content: '888x600' }, { box: '24,36 840x140', viewport: '828x140', content: '828x308' }],
    [700, 420, { box: '0,0 700x420', viewport: '688x408', content: '800x600' }, { box: '24,36 752x140', viewport: '740x140', content: '740x308' }],
  ] as const) {
    test(`at ${String(width)}x${String(height)} each box reserves its bars' gutters once and holds still`, async ({ page }) => {
      await openRoute(page, width, height);

      expect(await settledMetrics(page)).toBe(JSON.stringify([outer, inner]));
    });
  }
});
