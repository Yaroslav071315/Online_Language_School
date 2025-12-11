import { defineConfig, devices } from '@playwright/test';

export default defineConfig({
  testDir: './tests/e2e/tests',
  use: {
    baseURL: 'http://localhost:5208',
    headless: true,
    ignoreHTTPSErrors: true,
  },
  webServer: {
    command: 'dotnet run',
    url: 'http://localhost:5208',
    reuseExistingServer: !process.env.CI,
    timeout: 120 * 1000,
  },
  reporter: [['html', { outputFolder: 'playwright-report', open: 'never' }]],
  projects: [
    { name: 'Chromium', use: { ...devices['Desktop Chrome'] } },
  ],
});
