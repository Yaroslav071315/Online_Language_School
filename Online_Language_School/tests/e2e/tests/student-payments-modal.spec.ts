import { test, expect } from '@playwright/test';

test('Student can open payment modal', async ({ page }) => {
  // Припускаємо, що студент вже залогінений; інакше додати кроки логіну
  await page.goto('/Student/Payments');

  // Натискаємо першу кнопку "View"
  await page.click('button:has-text("View")');

  // Дочекатися, що модалка відкрита
  const modal = page.locator('.modal.show');
  await expect(modal).toBeVisible();

  // Перевіряємо, що заголовок модалки містить Payment #
  await expect(modal.locator('.modal-title')).toContainText('Payment');

  // Перевіряємо, що всередині картки є Course
  await expect(modal.locator('.card-title')).toContainText('Course');
});