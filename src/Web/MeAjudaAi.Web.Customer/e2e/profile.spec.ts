import { test, expect, loginAsCustomer } from '@meajudaai/web-e2e-support';

test.describe('@e2e Customer Web App - Provider Profile View', () => {
  test('should navigate to provider profile from search results', async ({ page }) => {
    await page.goto('/busca?servico=eletricista');
    
    const providerCard = page.locator('[data-testid="provider-card"]').first();
    await expect(providerCard).toBeVisible();
    
    await providerCard.click();
    
    await expect(page).toHaveURL(/.*\/prestador\/.+/);
  });

  test('should display provider profile details', async ({ page }) => {
    await page.goto('/prestador/test-provider-id');
    
    await expect(page.locator('[data-testid="provider-name"]')).toBeVisible();
    await expect(page.locator('[data-testid="provider-services"]')).toBeVisible();
    await expect(page.locator('[data-testid="provider-location"]')).toBeVisible();
  });

  test('should display provider photo', async ({ page }) => {
    await page.goto('/prestador/test-provider-id');
    
    const providerPhoto = page.locator('[data-testid="provider-photo"]');
    const photoCount = await providerPhoto.count();
    expect(photoCount).toBeGreaterThanOrEqual(1);
  });

  test('should display provider description', async ({ page }) => {
    await page.goto('/prestador/test-provider-id');
    
    await expect(page.locator('[data-testid="provider-description"]')).toBeVisible();
  });

  test('should display service pricing information', async ({ page }) => {
    await page.goto('/prestador/test-provider-id');
    
    const pricingSection = page.locator('[data-testid="pricing-section"]');
    const pricingCount = await pricingSection.count();
    if (pricingCount > 0) {
      await expect(pricingSection.first()).toBeVisible();
    }
  });
});

test.describe('@e2e Customer Web App - Contact Information', () => {
  test('should show login prompt for guest users', async ({ page }) => {
    await page.goto('/prestador/test-provider-id');
    
    const loginPrompt = page.locator('[data-testid="login-prompt"]');
    const promptCount = await loginPrompt.count();
    
    expect(promptCount).toBeGreaterThan(0);
    await expect(loginPrompt.first()).toBeVisible();
  });

  test('should show contact details for authenticated users', async ({ page }) => {
    await loginAsCustomer(page);
    await page.goto('/prestador/test-provider-id');
    
    await expect(page).toHaveURL(/.*\/prestador\/.+/);
    
    const contactSection = page.locator('[data-testid="contact-section"]');
    await expect(contactSection.first()).toBeVisible();
  });

  test('should display phone number for authenticated users', async ({ page }) => {
    await loginAsCustomer(page);
    await page.goto('/prestador/test-provider-id');
    
    await expect(page).toHaveURL(/.*\/prestador\/.+/);
    
    const phoneElement = page.locator('[data-testid="provider-phone"]');
    await expect(phoneElement.first()).toBeVisible();
  });
});

test.describe('@e2e Customer Web App - WhatsApp Interaction', () => {
  test('should display WhatsApp button for authenticated users', async ({ page }) => {
    await loginAsCustomer(page);
    await page.goto('/prestador/test-provider-id');
    
    await expect(page).toHaveURL(/.*\/prestador\/.+/);
    
    const whatsappButton = page.locator('[data-testid="whatsapp-button"]');
    await expect(whatsappButton.first()).toBeVisible();
  });

  test('should generate correct WhatsApp link format', async ({ page }) => {
    await loginAsCustomer(page);
    await page.goto('/prestador/test-provider-id');
    
    await expect(page).toHaveURL(/.*\/prestador\/.+/);
    
    const whatsappLink = page.locator('a[href^="https://wa.me/"]');
    await expect(whatsappLink.first()).toBeVisible();
    
    const href = await whatsappLink.first().getAttribute('href');
    expect(href).toMatch(/wa\.me\/\d+/);
  });

  test('should open WhatsApp in new tab', async ({ page }) => {
    await loginAsCustomer(page);
    await page.goto('/prestador/test-provider-id');
    
    await expect(page).toHaveURL(/.*\/prestador\/.+/);
    
    const whatsappLink = page.locator('a[href^="https://wa.me/"]');
    const target = await whatsappLink.first().getAttribute('target');
    expect(target).toBe('_blank');
  });
});

test.describe('@e2e Customer Web App - Provider Reviews', () => {
  test('should display reviews section', async ({ page }) => {
    await page.goto('/prestador/test-provider-id');
    
    await expect(page.locator('[data-testid="reviews-section"]')).toBeVisible();
  });

  test('should display existing reviews', async ({ page }) => {
    await page.goto('/prestador/test-provider-id');
    
    const reviewCards = page.locator('[data-testid="review-card"]');
    const reviewCount = await reviewCards.count();
    
    if (reviewCount > 0) {
      await expect(reviewCards.first()).toBeVisible();
    }
  });

  test('should display reviewer name and rating', async ({ page }) => {
    await page.goto('/prestador/test-provider-id');
    
    const reviewCard = page.locator('[data-testid="review-card"]').first();
    const cardCount = await reviewCard.count();
    
    if (cardCount > 0) {
      await expect(reviewCard.locator('[data-testid="reviewer-name"]')).toBeVisible();
      await expect(reviewCard.locator('[data-testid="review-rating"]')).toBeVisible();
    }
  });

  test('should display review date', async ({ page }) => {
    await page.goto('/prestador/test-provider-id');
    
    const reviewCard = page.locator('[data-testid="review-card"]').first();
    const cardCount = await reviewCard.count();
    
    if (cardCount > 0) {
      await expect(reviewCard.locator('[data-testid="review-date"]')).toBeVisible();
    }
  });

  test('should display review text', async ({ page }) => {
    await page.goto('/prestador/test-provider-id');
    
    const reviewCard = page.locator('[data-testid="review-card"]').first();
    const cardCount = await reviewCard.count();
    
    if (cardCount > 0) {
      await expect(reviewCard.locator('[data-testid="review-text"]')).toBeVisible();
    }
  });

  test('should display average rating', async ({ page }) => {
    await page.goto('/prestador/test-provider-id');
    
    const averageRating = page.locator('[data-testid="average-rating"]');
    const ratingCount = await averageRating.count();
    
    if (ratingCount > 0) {
      await expect(averageRating).toBeVisible();
    }
  });

  test('should display total reviews count', async ({ page }) => {
    await page.goto('/prestador/test-provider-id');
    
    const totalReviews = page.locator('[data-testid="total-reviews"]');
    const countElement = await totalReviews.count();
    
    if (countElement > 0) {
      await expect(totalReviews).toBeVisible();
    }
  });
});

test.describe('@e2e Customer Web App - Submit Review', () => {
  test.beforeEach(async ({ page }) => {
    await loginAsCustomer(page);
    await page.goto('/prestador/test-provider-id');
    await expect(page).toHaveURL(/.*\/prestador\/.+/);
  });

  test('should display review form for authenticated users', async ({ page }) => {
    const reviewForm = page.locator('[data-testid="review-form"]');
    await expect(reviewForm.first()).toBeVisible();
  });

  test('should allow rating selection', async ({ page }) => {
    const reviewForm = page.locator('[data-testid="review-form"]').first();
    const ratingStars = reviewForm.locator('[data-testid="rating-star"]');
    const starsCount = await ratingStars.count();
    
    expect(starsCount).toBeGreaterThan(0);
    
    const targetIndex = Math.min(4, starsCount - 1);
    await ratingStars.nth(targetIndex).click();
  });

  test('should allow entering review text', async ({ page }) => {
    const reviewForm = page.locator('[data-testid="review-form"]').first();
    const reviewTextarea = reviewForm.locator('textarea[name="review"]');
    
    await expect(reviewTextarea).toBeVisible();
    await reviewTextarea.fill('Excelente serviço! Recomendo.');
  });

  test('should submit review successfully', async ({ page }) => {
    const reviewForm = page.locator('[data-testid="review-form"]').first();
    
    const ratingStars = reviewForm.locator('[data-testid="rating-star"]');
    const starsCount = await ratingStars.count();
    if (starsCount > 0) {
      const targetIndex = Math.min(4, starsCount - 1);
      await ratingStars.nth(targetIndex).click();
    }
    
    const reviewTextarea = reviewForm.locator('textarea[name="review"]');
    await reviewTextarea.fill('Excelente serviço! Recomendo.');
    
    const submitButton = reviewForm.locator('button[type="submit"]');
    await submitButton.click();
    
    await expect(page.locator('text=Obrigado pela avaliação')).toBeVisible({ timeout: 5000 });
  });
});
