import { test, expect } from '@playwright/test';

test('Student can open material modal', async ({ page }) => {
  // Припускаємо, що студент вже залогінений; інакше додати кроки логіну




  await page.goto('http://localhost:5208/Account/Login');
  await page.fill('input[name="email"]', '1@gmail.com');
  await page.fill('input[name="password"]', '1');
  await page.click('button[type="submit"]');

 
  await page.goto('http://localhost:5208/Student/Materials');


  // Натискаємо першу кнопку "View"
  await page.click('button:has-text("View")');

  // Дочекатися, що модалка відкрита
  const modal = page.locator('.modal.show');
  await expect(modal).toBeVisible();

  // Перевіряємо, що заголовок модалки містить назву матеріалу
  await expect(modal.locator('.modal-title')).not.toBeEmpty();

  // Перевіряємо, що всередині картки є Lesson
  await expect(modal.locator('.card-title')).toContainText('Lesson');
});