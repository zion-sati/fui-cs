import { defineConfig } from '@playwright/test';

// The packaged-application check that scripts/test-package.sh runs against an application made from the templates.
export default defineConfig({
  testDir: './tests/package',
  timeout: 60_000,
  retries: 0,
  use: {
    browserName: 'chromium',
  },
});
