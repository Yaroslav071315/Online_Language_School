import { test, expect } from '@playwright/test';

test('Admin changes  role to Teacher', async ({ page }) => {
  // Логін як адмін
  await page.goto('http://localhost:5208/Account/Login');
  await page.fill('input[name="email"]', '3@gmail.com');
  await page.fill('input[name="password"]', '3');
  await page.click('button[type="submit"]');

  // Перехід у Users Management
  await page.goto('http://localhost:5208/Administrator/Office');
  await page.screenshot({ path: 'users-management.png', fullPage: true });


  // Дочекатися, що таблиця з’явилась
  await page.waitForSelector('tbody tr');

  // Знаходимо рядок саме користувача  по email (регулярка, щоб не залежати від регістру)
  const batmanRow = page.locator('tbody tr', { hasText: /batman@gmail.com/i });

  // Переконуємось, що рядок видимий
  await expect(batmanRow).toBeVisible();

  // Клікаємо кнопку Change Role у цьому рядку
  await batmanRow.locator('button:has-text("Change Role")').click();

  // Дочекатися відкриття модалки
  const modal = page.locator('.modal.show');
  await expect(modal).toBeVisible();

  // Усередині модалки вибрати Teacher
  await modal.locator('select[name="role"]').selectOption('Teacher');

  // Натиснути Save у модалці
  await modal.locator('button:has-text("Save")').click();

  // Перевірити, що у рядку Bat Man роль змінилась
  await expect(batmanRow.locator('td').nth(2)).toContainText('Teacher');
});