import { test, expect } from '@playwright/test';

test('Checkout flow reaches shopping bag safely', async ({ page }) => {
  await page.goto('/');

  await expect(page.getByRole('heading', { name: 'Ready for a new adventure?' })).toBeVisible();
  await page.getByRole('link', { name: 'Adventurer GPS Watch' }).click();
  await page.getByRole('button', { name: 'Add to shopping bag' }).click();
  await page.getByRole('link', { name: 'shopping bag' }).click();

  await expect(page.getByRole('heading', { name: 'Shopping bag' })).toBeVisible();
  await expect.poll(() => page.getByLabel('product quantity').count()).toBeGreaterThan(0);
  await expect.poll(() => page.getByText('Total').count()).toBeGreaterThan(0);
});
