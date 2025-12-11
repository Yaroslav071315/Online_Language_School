import { test, expect } from '@playwright/test';

test('User can register as Student', async ({ page }) => {
await page.goto('http://localhost:5208/Account/Register');


  await page.fill('input[name="FirstName"]', 'Test');
  await page.fill('input[name="LastName"]', 'User');

  // Використовуємо унікальний email, щоб уникнути конфлікту з існуючим користувачем
  
  await page.fill('input[name="Email"]', 'testuser@example.com');

  await page.fill('input[name="Password"]', 'Password123!');
  await page.fill('input[name="RepeatPassword"]', 'Password123!');

  await page.click('button[type="submit"]');

  // Перевіряємо, що після реєстрації відбувся редірект на Login
  await expect(page).toHaveURL(/Account\/Login/);
});