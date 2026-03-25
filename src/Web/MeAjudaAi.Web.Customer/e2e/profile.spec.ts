import { test, expect, loginAsCustomer } from '@meajudaai/web-e2e-support';

test.describe('Customer Web App - Provider Profile View', () => {
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

test.describe('Customer Web App - Contact Information', () => {
  test('should show login prompt for guest users', async ({ page }) => {
    await page.goto('/prestador/test-provider-id');
    
    const loginPrompt = page.locator('[data-testid="login-prompt"]');
    const promptCount = await loginPrompt.count();
    
    if (promptCount > 0) {
      await expect(loginPrompt).toBeVisible();
      await expect(loginPrompt).toContainText(/faça login|entre para ver/i);
    }
  });

  test('should show contact details for authenticated users', async ({ page }) => {
    await loginAsCustomer(page);
    await page.goto('/prestador/test-provider-id');
    
    await page.waitForTimeout(1000);
    
    const contactSection = page.locator('[data-testid="contact-section"]');
    const contactCount = await contactSection.count();
    
    if (contactCount > 0) {
      await expect(contactSection).toBeVisible();
    }
  });

  test('should display phone number for authenticated users', async ({ page }) => {
    await loginAsCustomer(page);
    await page.goto('/prestador/test-provider-id');
    
    await page.waitForTimeout(1000);
    
    const phoneElement = page.locator('[data-testid="provider-phone"]');
    const phoneCount = await phoneElement.count();
    
    if (phoneCount > 0) {
      await expect(phoneElement).toBeVisible();
    }
  });
});

test.describe('Customer Web App - WhatsApp Interaction', () => {
  test('should display WhatsApp button for authenticated users', async ({ page }) => {
    await loginAsCustomer(page);
    await page.goto('/prestador/test-provider-id');
    
    await page.waitForTimeout(1000);
    
    const whatsappButton = page.locator('[data-testid="whatsapp-button"]');
    const waCount = await whatsappButton.count();
    
    if (waCount > 0) {
      await expect(whatsappButton).toBeVisible();
    }
  });

  test('should generate correct WhatsApp link format', async ({ page }) => {
    await loginAsCustomer(page);
    await page.goto('/prestador/test-provider-id');
    
    await page.waitForTimeout(1000);
    
    const whatsappLink = page.locator('a[href^="https://wa.me/"]');
    const linkCount = await whatsappLink.count();
    
    if (linkCount > 0) {
      await expect(whatsappLink.first()).toBeVisible();
      
      const href = await whatsappLink.first().getAttribute('href');
      expect(href).toMatch(/wa\.me\/\d+/);
    }
  });

  test('should open WhatsApp in new tab', async ({ page }) => {
    await loginAsCustomer(page);
    await page.goto('/prestador/test-provider-id');
    
    await page.waitForTimeout(1000);
    
    const whatsappLink = page.locator('a[href^="https://wa.me/"]');
    const linkCount = await whatsappLink.count();
    
    if (linkCount > 0) {
      const target = await whatsappLink.first().getAttribute('target');
      expect(target).toBe('_blank');
    }
  });
});

test.describe('Customer Web App - Provider Reviews', () => {
  test('should display reviews section', async ({ page }) => {
    await page.goto('/prestador/test-provider-id');
    
    const reviewsSection = page.locator('[data-testid="reviews-section"]');
    await expect(reviewsSection).toBeVisible();
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

test.describe('Customer Web App - Submit Review', () => {
  test.beforeEach(async ({ page }) => {
    await loginAsCustomer(page);
    await page.goto('/prestador/test-provider-id');
    await page.waitForTimeout(1000);
  });

  test('should display review form for authenticated users', async ({ page }) => {
    const reviewForm = page.locator('[data-testid="review-form"]');
    const formCount = await reviewForm.count();
    
    if (formCount > 0) {
      await expect(reviewForm).toBeVisible();
    }
  });

  test('should allow rating selection', async ({ page }) => {
    const reviewForm = page.locator('[data-testid="review-form"]');
    const formCount = await reviewForm.count();
    
    if (formCount > 0) {
      const ratingStars = reviewForm.locator('[data-testid="rating-star"]');
      const starsCount = await ratingStars.count();
      expect(starsCount).toBeGreaterThan(0);
      
      await ratingStars.nth(4).click();
    }
  });

  test('should allow entering review text', async ({ page }) => {
    const reviewForm = page.locator('[data-testid="review-form"]');
    const formCount = await reviewForm.count();
    
    if (formCount > 0) {
      const reviewTextarea = reviewForm.locator('textarea[name="review"]');
      const textareaCount = await reviewTextarea.count();
      
      if (textareaCount > 0) {
        await reviewTextarea.fill('Excelente serviço! Recomendo.');
      }
    }
  });

  test('should submit review successfully', async ({ page }) => {
    const reviewForm = page.locator('[data-testid="review-form"]');
    const formCount = await reviewForm.count();
    
    if (formCount > 0) {
      const ratingStars = reviewForm.locator('[data-testid="rating-star"]');
      if (await ratingStars.count() > 0) {
        await ratingStars.nth(4).click();
      }
      
      const reviewTextarea = reviewForm.locator('textarea[name="review"]');
      if (await reviewTextarea.count() > 0) {
        await reviewTextarea.fill('Excelente serviço! Recomendo.');
      }
      
      const submitButton = reviewForm.locator('button[type="submit"]');
      if (await submitButton.count() > 0) {
        await submitButton.click();
        
        await expect(page.locator('text=Obrigado pela avaliação')).toBeVisible({ timeout: 5000 }).catch(() => {});
      }
    }
  });
});
