import { test, expect } from '@playwright/test';

test('Site is reachable', async ({ page }) => {
  await page.goto('http://localhost:5208');
  await expect(page).toHaveTitle(/Online Language School/); // заміни на реальний <title>
});
