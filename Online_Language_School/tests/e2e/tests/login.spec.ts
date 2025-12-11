import { test, expect } from '@playwright/test';

test('Student can log in and reach Office', async ({ page }) => {
  // Використовуємо повний URL для переходу на сторінку реєстрації
  await page.goto('http://localhost:5208/Account/Register');

  // Заповнюємо форму реєстрації
  await page.fill('input[name="FirstName"]', 'Test');
  await page.fill('input[name="LastName"]', 'User');
  const uniqueEmail = `test_${Date.now()}@example.com`;
  await page.fill('input[name="Email"]', uniqueEmail);
  await page.fill('input[name="Password"]', 'Password123!');
  await page.fill('input[name="RepeatPassword"]', 'Password123!');
  await page.click('button[type="submit"]');

  // Після реєстрації має бути редірект на Login
  await expect(page).toHaveURL(/Account\/Login/);

  // Логін новим користувачем
  await page.fill('input[name="email"]', uniqueEmail);
  await page.fill('input[name="password"]', 'Password123!');
  await page.click('button[type="submit"]');

  // Перевіряємо, що студент потрапив у свій Office
  await expect(page).toHaveURL(/Student\/Office/);
});