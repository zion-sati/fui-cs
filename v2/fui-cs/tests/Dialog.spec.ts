import { expect, test, type Page } from '@playwright/test';

import { clickNode, hostNode, labelOf, openTestPage, screenshotPath, serveApp } from './support.js';

const baseUrl = serveApp();

async function expectLabel(page: Page, nodeId: string, text: string): Promise<void> {
  await expect.poll(async () => await labelOf(page, nodeId)).toBe(text);
}

async function open(page: Page): Promise<void> {
  await clickNode(page, 'open');
  await expect.poll(async () => (await hostNode(page, 'dialog-ok')) !== null).toBe(true);
  await expect(page.getByRole('dialog', { name: 'Delete file?' })).toHaveCount(1);
}

async function expectClosed(page: Page): Promise<void> {
  await expect.poll(async () => await hostNode(page, 'dialog-ok')).toBeNull();
}

test.describe('Dialog in the host', () => {
  test('OK accepts and closes the dialog', async ({ page }) => {
    const errors = await openTestPage(page, baseUrl(), 'dialog');

    await open(page);
    await page.screenshot({ path: screenshotPath('fui-cs-dialog.png') });
    await clickNode(page, 'dialog-ok');

    await expectLabel(page, 'result', 'accepted');
    await expectClosed(page);
    expect(errors).toEqual([]);
  });

  test('Escape cancels and focus returns to the opener', async ({ page }) => {
    await openTestPage(page, baseUrl(), 'dialog');
    await open(page);
    await expectLabel(page, 'opener-focus', 'blurred');

    await page.keyboard.press('Escape');

    await expectLabel(page, 'result', 'cancelled');
    await expectClosed(page);
    await expectLabel(page, 'opener-focus', 'focused');
  });

  test('Enter accepts the focused OK button', async ({ page }) => {
    await openTestPage(page, baseUrl(), 'dialog');
    await open(page);

    await page.keyboard.press('Enter');

    await expectLabel(page, 'result', 'accepted');
  });

  test('the backdrop cancels and keeps clicks from the page underneath', async ({ page }) => {
    await openTestPage(page, baseUrl(), 'dialog');
    await open(page);

    await clickNode(page, 'dialog-title');
    await page.waitForTimeout(100);
    expect(await labelOf(page, 'result')).toBe('none');
    await clickNode(page, 'under');

    await expectLabel(page, 'result', 'cancelled');
    expect(await labelOf(page, 'page-clicks')).toBe('0');
    await clickNode(page, 'under');
    await expectLabel(page, 'page-clicks', '1');
  });
});
