import { expect, test, type Page } from '@playwright/test';

import { debugNodes, hostNode, labelStartingWith, screenshotPath, serveApp, waitForReady } from './support.js';

const baseUrl = serveApp();
const route = '/v2/fui-cs/demo/advanced-controls/';

async function openRoute(page: Page): Promise<string[]> {
  const errors: string[] = [];
  page.on('pageerror', (error) => errors.push(error.message));
  page.on('console', (message) => {
    if (message.type() === 'error') errors.push(message.text());
  });
  // Tall enough that every section is on screen.
  await page.setViewportSize({ width: 1280, height: 4200 });
  await page.goto(`${baseUrl()}${route}`);
  await waitForReady(page);
  await expect.poll(async () => await labelStartingWith(page, 'Worker status: ')).not.toBeNull();
  return errors;
}

/** Clicks the center of the node whose semantic label is `label`. */
async function clickLabel(page: Page, label: string): Promise<void> {
  const node = (await debugNodes(page)).find((candidate) => candidate.semanticLabel === label);
  if (node === undefined) throw new Error(`No node is labelled "${label}".`);
  const canvas = await page.locator('#fui-canvas').boundingBox();
  if (canvas === null) throw new Error('Expected the fui-cs canvas.');
  await page.mouse.click(canvas.x + node.bounds.x + (node.bounds.width / 2), canvas.y + node.bounds.y + (node.bounds.height / 2));
}

/** The vertical offset of the animation sample's scroll box. */
async function animationScrollOffset(page: Page): Promise<number> {
  const nodes = await debugNodes(page);
  const box = nodes.find((node) => node.nodeId === 'advanced-controls:animation-scroll');
  const view = nodes.find((node) => node.handle === box?.scroll.scrollProxyTargetHandle) ?? box;
  return Math.round((view?.scroll as unknown as { offsetY: number } | undefined)?.offsetY ?? -1);
}

test.describe('Advanced controls route', () => {
  test('the route page shows its sections in the reference\'s order', async ({ page }) => {
    const errors = await openRoute(page);

    await expect.poll(async () => await page.title()).toBe('EffinDOM FUI-CS Demo • Advanced controls');
    expect(await labelStartingWith(page, 'Current route: ')).toBe(`Current route: ${route}`);
    // The TextArea section's title repeats the route's, so its description marks it.
    const titles = ['Configure a TextArea live to explore wrapping, read-only mode, scrollbar policy, line height, font family, and visibility.', 'Transitions + scroll surfaces', 'Drag-and-drop reorder / Custom control', 'ProgressBar + Worker sample', 'Online Fetch sample', 'Static rich text', 'App-authored custom fonts', 'Auto sizing'];
    const positions = [];
    for (const title of titles) {
      const node = (await debugNodes(page)).find((candidate) => candidate.semanticLabel === title);
      expect(node, title).toBeDefined();
      positions.push(node?.bounds.y ?? 0);
    }
    expect([...positions].sort((a, b) => a - b)).toEqual(positions);
    await page.screenshot({ path: screenshotPath('fui-cs-advanced-controls.png') });
    expect(errors).toEqual([]);
  });

  test('the preview card transitions, and the samples scroll smoothly to each target', async ({ page }) => {
    await openRoute(page);

    await clickLabel(page, 'Emphasize preview card');
    await expect.poll(async () => await labelStartingWith(page, 'Preview state: ')).toBe('Preview state: emphasized • Transition slots: bgColor + opacity');
    expect((await debugNodes(page)).some((node) => node.semanticLabel === 'Emphasized transition target')).toBe(true);

    await clickLabel(page, 'Scroll to 13th sample');
    await expect.poll(async () => await animationScrollOffset(page)).toBe(960);
    expect(await labelStartingWith(page, 'Smooth scroll target: ')).toBe('Smooth scroll target: final • API: scrollToAnimated(0, offset, cubicOut) • scrollContentSize(-1, 1680)');
    // The logical tail lies past the mounted rows: scrollContentSize extends the range to it.
    await clickLabel(page, 'Scroll to logical tail');
    await expect.poll(async () => await animationScrollOffset(page)).toBe(1400);
    await clickLabel(page, 'Scroll to first sample');
    await expect.poll(async () => await animationScrollOffset(page)).toBe(0);
  });

  test('the prime worker reports progress and completes, and a cancel stops it at its next yield', async ({ page }) => {
    await openRoute(page);
    expect(await labelStartingWith(page, 'Worker status: ')).toBe('Worker status: idle • Progress: 0%');

    await clickLabel(page, 'Start prime worker');
    await expect.poll(async () => await labelStartingWith(page, 'Prime search progress: ')).not.toBeNull();
    await expect.poll(async () => await labelStartingWith(page, 'Worker status: '), { timeout: 15_000 }).toBe('Worker status: complete • Progress: 100%');
    expect(Number((await labelStartingWith(page, 'Largest prime after 5s: '))?.split(': ')[1])).toBeGreaterThan(1_000);

    await clickLabel(page, 'Start prime worker');
    await expect.poll(async () => await labelStartingWith(page, 'Worker status: running')).not.toBeNull();
    await clickLabel(page, 'Cancel prime worker');
    await expect.poll(async () => await labelStartingWith(page, 'Worker status: '), { timeout: 15_000 }).toMatch(/^Worker status: cancelled • Progress: \d+%$/);
    expect(await labelStartingWith(page, 'Prime search cancelled after yielding at ')).toMatch(/^Prime search cancelled after yielding at \d+%\.$/);
    expect(await hostNode(page, 'advanced-controls:animation-scroll')).not.toBeNull();
  });
});
