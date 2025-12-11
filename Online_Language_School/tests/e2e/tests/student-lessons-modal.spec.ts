import { test, expect } from '@playwright/test';

test('Student can search and see courses', async ({ page }) => {


  await page.goto('http://localhost:5208/Account/Login');
  await page.fill('input[name="email"]', '1@gmail.com');
  await page.fill('input[name="password"]', '1');
  await page.click('button[type="submit"]');

  // Перехід у Users Management
  await page.goto('http://localhost:5208/Student/Courses');


  // Припускаємо, що студент вже залогінений; інакше додати кроки логіну


  // Заповнюємо форму пошуку (наприклад, шукаємо англійську мову рівня Beginner)
  await page.fill('input[name="Language"]', 'English');
 

  // Натискаємо кнопку Search
  await page.click('#courseSearchForm button[type="submit"]');

  // Перевіряємо, що з’явились картки курсів
  const courseCards = page.locator('.card .card-title');
  await expect(courseCards.first()).toBeVisible();

  // Перевіряємо, що заголовок містить слово "Start"
  await expect(courseCards.first()).toContainText(/Start/i);
});