import { defineConfig } from '@playwright/test';

export default defineConfig({
  testDir: './tests',
  // tests/package checks an application built from the packages; scripts/test-package.sh runs it.
  testIgnore: ['package/**'],
  timeout: 30_000,
  retries: 1,
  use: {
    browserName: 'chromium',
  },
});
