import { expect, test, type Page } from '@playwright/test';

import { boundsOf, debugNodes, labelStartingWith, nextFrames, pointIn, serveApp, waitForReady } from './support.js';

const baseUrl = serveApp();

interface ProjectedField {
  readonly handle: string;
  readonly type: string;
  readonly autocomplete: string | null;
  readonly name: string | null;
  readonly id: string;
  readonly label: string | null;
  readonly inForm: boolean;
  readonly value: string;
}

async function projectedFields(page: Page): Promise<ProjectedField[]> {
  return await page.evaluate(() => Array.from(document.querySelectorAll<HTMLInputElement>('[data-effindom-semantic-light-dom-field="true"]')).map((field) => ({
    handle: field.dataset.effindomHandle ?? '',
    type: field.type,
    autocomplete: field.getAttribute('autocomplete'),
    name: field.getAttribute('name'),
    id: field.id,
    label: field.getAttribute('aria-label'),
    inForm: field.form !== null,
    value: field.value,
  })));
}

/** The type, autocomplete hint and name of the hidden editor that has the browser's focus. */
async function hiddenEditor(page: Page): Promise<string | null> {
  return await page.evaluate(() => {
    const editor = document.activeElement;
    if (!(editor instanceof HTMLInputElement) || editor.dataset.effindomHiddenEditor !== 'true') return null;
    return `${editor.type} ${editor.getAttribute('autocomplete') ?? ''} ${editor.getAttribute('name') ?? ''}`;
  });
}

async function openDashboard(page: Page): Promise<string[]> {
  const errors: string[] = [];
  page.on('pageerror', (error) => errors.push(error.message));
  await page.setViewportSize({ width: 1280, height: 900 });
  await page.goto(`${baseUrl()}/v2/fui-cs/demo/`);
  await waitForReady(page);
  await expect.poll(async () => await labelStartingWith(page, 'Dialog status: ')).toBe('Dialog status: idle');
  return errors;
}

async function clickLabel(page: Page, label: string): Promise<void> {
  const node = (await debugNodes(page)).find((candidate) => candidate.semanticLabel === label);
  if (node === undefined) throw new Error(`No node is labelled "${label}".`);
  const canvas = await page.locator('#fui-canvas').boundingBox();
  await page.mouse.click((canvas?.x ?? 0) + node.bounds.x + (node.bounds.width / 2), (canvas?.y ?? 0) + node.bounds.y + (node.bounds.height / 2));
}

async function openDialog(page: Page): Promise<void> {
  await clickLabel(page, 'Open dialog');
  await expect.poll(async () => await labelStartingWith(page, 'Dialog status: ')).toBe('Dialog status: open');
  await expect.poll(async () => (await projectedFields(page)).length).toBe(2);
}

test.describe('Form and host autofill on the dashboard', () => {
  test('the sign-in dialog projects its fields into a host form for autofill, and focuses the username', async ({ page }) => {
    const errors = await openDashboard(page);
    await openDialog(page);

    const fields = await projectedFields(page);
    expect(fields.map((field) => ({ ...field, handle: '', value: '' }))).toEqual([
      { handle: '', type: 'text', autocomplete: 'username', name: 'username', id: 'username', label: 'Username', inForm: true, value: '' },
      { handle: '', type: 'password', autocomplete: 'current-password', name: 'dialog-current-password', id: 'dialog-current-password', label: 'Password', inForm: true, value: '' },
    ]);
    // The projected fields are the semantic source: no second textbox node for the same handles.
    const duplicates = await page.evaluate((handles) => handles.reduce((count, handle) => count + document.querySelectorAll(
      `[data-effindom-semantic-node="true"][data-role="textbox"][data-handle="${handle}"]:not([data-effindom-semantic-kind="host-autofill-textbox"])`).length, 0), fields.map((field) => field.handle));
    expect(duplicates).toBe(0);
    await expect.poll(async () => await hiddenEditor(page)).toBe('text username username');
    expect(errors).toEqual([]);
  });

  test('autofilled values reach both fields, Enter accepts, and closing removes the projection', async ({ page }) => {
    const errors = await openDashboard(page);
    await openDialog(page);

    await page.evaluate(() => {
      const fields = Array.from(document.querySelectorAll<HTMLInputElement>('[data-effindom-semantic-light-dom-field="true"]'));
      for (const [name, value] of [['username', 'alice@example.com'], ['dialog-current-password', 'secret-password']]) {
        const field = fields.find((candidate) => candidate.name === name);
        if (field === undefined) throw new Error(`Expected the projected ${name} field.`);
        field.value = value;
        field.dispatchEvent(new Event('input', { bubbles: true }));
      }
    });
    await expect.poll(async () => (await projectedFields(page)).map((field) => `${field.name ?? ''}=${field.value}`)).toEqual([
      'username=alice@example.com',
      'dialog-current-password=secret-password',
    ]);

    await page.keyboard.press('Enter');
    await expect.poll(async () => await labelStartingWith(page, 'Dialog status: ')).toBe('Dialog status: accepted');
    await expect.poll(async () => (await projectedFields(page)).length).toBe(0);

    // Reopening starts from empty fields.
    await openDialog(page);
    expect((await projectedFields(page)).map((field) => field.value)).toEqual(['', '']);
    await page.keyboard.press('Escape');
    await expect.poll(async () => await labelStartingWith(page, 'Dialog status: ')).toBe('Dialog status: cancelled');
    expect(errors).toEqual([]);
  });

  test('the dashboard\'s own inputs carry their autofill metadata onto the hidden editor', async ({ page }) => {
    const errors = await openDashboard(page);
    await page.mouse.move(800, 450);
    await expect.poll(async () => {
      const bounds = await boundsOf(page, 'current-password');
      if (bounds.y + bounds.height <= 800) return true;
      await page.mouse.wheel(0, 300);
      return false;
    }, { timeout: 10_000 }).toBe(true);
    await expect.poll(async () => {
      const before = (await boundsOf(page, 'current-password')).y;
      await nextFrames(page);
      return (await boundsOf(page, 'current-password')).y === before;
    }).toBe(true);

    for (const [nodeId, expected] of [['demo-dashboard:text-input', 'text username demo-dashboard:text-input'], ['current-password', 'password current-password current-password']]) {
      const point = await pointIn(page, nodeId, 0.5);
      await page.mouse.click(point.x, point.y);
      await expect.poll(async () => await hiddenEditor(page)).toBe(expected);
    }
    expect(errors).toEqual([]);
  });
});
