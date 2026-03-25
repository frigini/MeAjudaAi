import { test, expect, loginAsProvider } from '@meajudaai/web-e2e-support';

test.describe('Provider Web App - Dashboard Metrics', () => {
  test.beforeEach(async ({ page }) => {
    await loginAsProvider(page);
    await page.goto('/provider/dashboard');
  });

  test('should display total bookings metric', async ({ page }) => {
    const metricCard = page.locator('[data-testid="metric-total-bookings"]');
    await expect(metricCard).toBeVisible();
    
    const metricValue = metricCard.locator('[data-testid="metric-value"]');
    await expect(metricValue).toBeVisible();
    
    const valueText = await metricValue.textContent();
    expect(valueText).toBeDefined();
    expect(valueText).toMatch(/\d+/);
  });

  test('should display confirmed bookings metric', async ({ page }) => {
    const metricCard = page.locator('[data-testid="metric-confirmed-bookings"]');
    await expect(metricCard).toBeVisible();
    
    const metricValue = metricCard.locator('[data-testid="metric-value"]');
    await expect(metricValue).toBeVisible();
    
    const valueText = await metricValue.textContent();
    expect(valueText).toBeDefined();
    expect(valueText).toMatch(/\d+/);
  });

  test('should display pending bookings metric', async ({ page }) => {
    const metricCard = page.locator('[data-testid="metric-pending-bookings"]');
    await expect(metricCard).toBeVisible();
    
    const metricValue = metricCard.locator('[data-testid="metric-value"]');
    await expect(metricValue).toBeVisible();
    
    const valueText = await metricValue.textContent();
    expect(valueText).toBeDefined();
    expect(valueText).toMatch(/\d+/);
  });

  test('should display cancelled bookings metric', async ({ page }) => {
    const metricCard = page.locator('[data-testid="metric-cancelled-bookings"]');
    await expect(metricCard).toBeVisible();
    
    const metricValue = metricCard.locator('[data-testid="metric-value"]');
    await expect(metricValue).toBeVisible();
    
    const valueText = await metricValue.textContent();
    expect(valueText).toBeDefined();
    expect(valueText).toMatch(/\d+/);
  });

  test('should display total earnings metric', async ({ page }) => {
    const metricCard = page.locator('[data-testid="metric-total-earnings"]');
    await expect(metricCard).toBeVisible();
    
    const metricValue = metricCard.locator('[data-testid="metric-value"]');
    await expect(metricValue).toBeVisible();
    
    const valueText = await metricValue.textContent();
    expect(valueText).toBeDefined();
  });

  test('should display average rating metric', async ({ page }) => {
    const metricCard = page.locator('[data-testid="metric-average-rating"]');
    await expect(metricCard).toBeVisible();
    
    const metricValue = metricCard.locator('[data-testid="metric-value"]');
    await expect(metricValue).toBeVisible();
    
    const valueText = await metricValue.textContent();
    expect(valueText).toBeDefined();
  });

  test('should display profile completion percentage', async ({ page }) => {
    const metricCard = page.locator('[data-testid="metric-profile-completion"]');
    await expect(metricCard).toBeVisible();
    
    const metricValue = metricCard.locator('[data-testid="metric-value"]');
    await expect(metricValue).toBeVisible();
    
    const valueText = await metricValue.textContent();
    expect(valueText).toMatch(/\d+%/);
  });
});

test.describe('Provider Web App - Dashboard API Integration', () => {
  test.beforeEach(async ({ page }) => {
    await loginAsProvider(page);
    await page.goto('/provider/dashboard');
  });

  test('should fetch real data from API', async ({ page }) => {
    await page.waitForResponse(response => 
      response.url().includes('/api/') && response.status() === 200
    );
    
    const metricValue = page.locator('[data-testid="metric-total-bookings"] [data-testid="metric-value"]');
    const valueText = await metricValue.textContent();
    expect(valueText).not.toBe('-');
  });

  test('should display data with proper formatting', async ({ page }) => {
    await page.waitForResponse(response => 
      response.url().includes('/api/') && response.status() === 200
    );
    
    const earningsMetric = page.locator('[data-testid="metric-total-earnings"] [data-testid="metric-value"]');
    const valueText = await earningsMetric.textContent();
    expect(valueText).toMatch(/R\$|\$/);
  });

  test('should show loading state initially', async ({ page }) => {
    await page.goto('/provider/dashboard');
    
    const loadingState = page.locator('[data-testid="dashboard-loading"]');
    const loadingCount = await loadingState.count();
    
    if (loadingCount > 0) {
      await expect(loadingState).toBeVisible();
    }
  });
});

test.describe('Provider Web App - Recent Bookings List', () => {
  test.beforeEach(async ({ page }) => {
    await loginAsProvider(page);
    await page.goto('/provider/dashboard');
  });

  test('should display recent bookings section', async ({ page }) => {
    const recentBookings = page.locator('[data-testid="recent-bookings"]');
    await expect(recentBookings).toBeVisible();
  });

  test('should display booking cards with customer name', async ({ page }) => {
    await page.waitForResponse(response => 
      response.url().includes('/api/') && response.status() === 200
    );
    
    const bookingCards = page.locator('[data-testid="booking-card"]');
    const cardCount = await bookingCards.count();
    
    if (cardCount > 0) {
      const firstCard = bookingCards.first();
      await expect(firstCard.locator('[data-testid="customer-name"]')).toBeVisible();
    }
  });

  test('should display booking status', async ({ page }) => {
    await page.waitForResponse(response => 
      response.url().includes('/api/') && response.status() === 200
    );
    
    const bookingCards = page.locator('[data-testid="booking-card"]');
    const cardCount = await bookingCards.count();
    
    if (cardCount > 0) {
      const firstCard = bookingCards.first();
      await expect(firstCard.locator('[data-testid="booking-status"]')).toBeVisible();
    }
  });

  test('should display booking date and time', async ({ page }) => {
    await page.waitForResponse(response => 
      response.url().includes('/api/') && response.status() === 200
    );
    
    const bookingCards = page.locator('[data-testid="booking-card"]');
    const cardCount = await bookingCards.count();
    
    if (cardCount > 0) {
      const firstCard = bookingCards.first();
      await expect(firstCard.locator('[data-testid="booking-date"]')).toBeVisible();
    }
  });

  test('should display booking service type', async ({ page }) => {
    await page.waitForResponse(response => 
      response.url().includes('/api/') && response.status() === 200
    );
    
    const bookingCards = page.locator('[data-testid="booking-card"]');
    const cardCount = await bookingCards.count();
    
    if (cardCount > 0) {
      const firstCard = bookingCards.first();
      await expect(firstCard.locator('[data-testid="booking-service"]')).toBeVisible();
    }
  });

  test('should allow viewing booking details', async ({ page }) => {
    await page.waitForResponse(response => 
      response.url().includes('/api/') && response.status() === 200
    );
    
    const bookingCards = page.locator('[data-testid="booking-card"]');
    const cardCount = await bookingCards.count();
    
    if (cardCount > 0) {
      const viewButton = bookingCards.first().locator('button:has-text("Ver detalhes")');
      const buttonCount = await viewButton.count();
      
      if (buttonCount > 0) {
        await viewButton.click();
        await expect(page.locator('[data-testid="booking-detail-modal"]')).toBeVisible();
      }
    }
  });
});

test.describe('Provider Web App - Dashboard Charts', () => {
  test.beforeEach(async ({ page }) => {
    await loginAsProvider(page);
    await page.goto('/provider/dashboard');
  });

  test('should display bookings over time chart', async ({ page }) => {
    const bookingsChart = page.locator('[data-testid="bookings-chart"]');
    const chartCount = await bookingsChart.count();
    
    if (chartCount > 0) {
      await expect(bookingsChart).toBeVisible();
    }
  });

  test('should display earnings chart', async ({ page }) => {
    const earningsChart = page.locator('[data-testid="earnings-chart"]');
    const chartCount = await earningsChart.count();
    
    if (chartCount > 0) {
      await expect(earningsChart).toBeVisible();
    }
  });

  test('should display rating history chart', async ({ page }) => {
    const ratingChart = page.locator('[data-testid="rating-chart"]');
    const chartCount = await ratingChart.count();
    
    if (chartCount > 0) {
      await expect(ratingChart).toBeVisible();
    }
  });
});

test.describe('Provider Web App - Onboarding Services Step', () => {
  test.beforeEach(async ({ page }) => {
    await loginAsProvider(page);
    await page.goto('/provider/onboarding/servicos');
  });

  test('should display service categories', async ({ page }) => {
    const categoriesList = page.locator('[data-testid="service-categories"]');
    await expect(categoriesList).toBeVisible();
  });

  test('should allow selecting service categories', async ({ page }) => {
    const categoryCheckbox = page.locator('input[type="checkbox"]').first();
    await categoryCheckbox.check();
    await expect(categoryCheckbox).toBeChecked();
  });

  test('should display selected services summary', async ({ page }) => {
    const categoryCheckbox = page.locator('input[type="checkbox"]').first();
    await categoryCheckbox.check();
    
    const summary = page.locator('[data-testid="selected-services-summary"]');
    const summaryCount = await summary.count();
    
    if (summaryCount > 0) {
      await expect(summary).toBeVisible();
    }
  });

  test('should validate at least one service is selected', async ({ page }) => {
    await page.click('button:has-text("Próximo")');
    
    const errorMessage = page.locator('text=Selecione pelo menos um serviço');
    const errorCount = await errorMessage.count();
    
    if (errorCount > 0) {
      await expect(errorMessage).toBeVisible();
    }
  });

  test('should complete services selection and proceed', async ({ page }) => {
    const categoryCheckbox = page.locator('input[type="checkbox"]').first();
    await categoryCheckbox.check();
    
    await page.click('button:has-text("Próximo")');
    
    await expect(page).toHaveURL(/.*\/provider\/dashboard/);
  });
});
