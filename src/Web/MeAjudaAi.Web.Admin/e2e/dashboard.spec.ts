import { test, expect, loginAsAdmin } from '@meajudaai/web-e2e-support';

test.describe('Admin Portal - Dashboard KPIs', () => {
  test.beforeEach(async ({ page }) => {
    await loginAsAdmin(page);
    await page.goto('/admin/dashboard');
  });

  test('should display total providers KPI card', async ({ page }) => {
    const kpiCard = page.locator('[data-testid="kpi-total-providers"]');
    await expect(kpiCard).toBeVisible();
    
    const value = kpiCard.locator('[data-testid="kpi-value"]');
    await expect(value).toBeVisible();
    
    const label = kpiCard.locator('[data-testid="kpi-label"]');
    await expect(label).toContainText(/total de prestadores/i);
  });

  test('should display pending verification KPI card', async ({ page }) => {
    const kpiCard = page.locator('[data-testid="kpi-pending-verification"]');
    await expect(kpiCard).toBeVisible();
    
    const value = kpiCard.locator('[data-testid="kpi-value"]');
    await expect(value).toBeVisible();
    
    const label = kpiCard.locator('[data-testid="kpi-label"]');
    await expect(label).toContainText(/aguardando verificaç/i);
  });

  test('should display approved providers KPI card', async ({ page }) => {
    const kpiCard = page.locator('[data-testid="kpi-approved"]');
    await expect(kpiCard).toBeVisible();
    
    const value = kpiCard.locator('[data-testid="kpi-value"]');
    await expect(value).toBeVisible();
    
    const label = kpiCard.locator('[data-testid="kpi-label"]');
    await expect(label).toContainText(/aprovad/i);
  });

  test('should display rejected providers KPI card', async ({ page }) => {
    const kpiCard = page.locator('[data-testid="kpi-rejected"]');
    await expect(kpiCard).toBeVisible();
    
    const value = kpiCard.locator('[data-testid="kpi-value"]');
    await expect(value).toBeVisible();
    
    const label = kpiCard.locator('[data-testid="kpi-label"]');
    await expect(label).toContainText(/rejeitad/i);
  });

  test('should display KPI cards in a grid layout', async ({ page }) => {
    const kpiGrid = page.locator('[data-testid="kpi-grid"]');
    await expect(kpiGrid).toBeVisible();
    
    const kpiCards = kpiGrid.locator('[data-testid^="kpi-"]');
    const count = await kpiCards.count();
    expect(count).toBeGreaterThanOrEqual(4);
  });

  test('should navigate to providers list from KPI', async ({ page }) => {
    const kpiCard = page.locator('[data-testid="kpi-total-providers"]');
    await kpiCard.click();
    
    await expect(page).toHaveURL(/.*\/admin\/providers/);
  });
});

test.describe('Admin Portal - Dashboard Charts', () => {
  test.beforeEach(async ({ page }) => {
    await loginAsAdmin(page);
    await page.goto('/admin/dashboard');
  });

  test('should display verification status pie chart', async ({ page }) => {
    const pieChart = page.locator('[data-testid="verification-pie-chart"]');
    await expect(pieChart).toBeVisible();
    
    const chartTitle = pieChart.locator('[data-testid="chart-title"]');
    await expect(chartTitle).toContainText(/status de verificaç/i);
  });

  test('should display pie chart legend', async ({ page }) => {
    const pieChart = page.locator('[data-testid="verification-pie-chart"]');
    await expect(pieChart).toBeVisible();
    
    const legend = pieChart.locator('[data-testid="chart-legend"]');
    await expect(legend).toBeVisible();
    
    await expect(legend).toContainText(/aprovad/i);
    await expect(legend).toContainText(/rejeitad/i);
    await expect(legend).toContainText(/pendente/i);
  });

  test('should display chart with data from API', async ({ page }) => {
    const pieChart = page.locator('[data-testid="verification-pie-chart"]');
    await expect(pieChart).toBeVisible();
    
    const chartSvg = pieChart.locator('svg');
    await expect(chartSvg).toBeVisible();
    
    const legendItems = pieChart.locator('[data-testid="legend-item"]');
    const legendCount = await legendItems.count();
    expect(legendCount).toBeGreaterThan(0);
  });

  test('should display providers over time line chart', async ({ page }) => {
    const lineChart = page.locator('[data-testid="providers-line-chart"]');
    await expect(lineChart).toBeVisible();
    
    const chartTitle = lineChart.locator('[data-testid="chart-title"]');
    await expect(chartTitle).toContainText(/prestadores ao longo do tempo/i);
  });

  test('should display chart tooltip on hover', async ({ page }) => {
    const pieChart = page.locator('[data-testid="verification-pie-chart"]');
    await expect(pieChart).toBeVisible();
    
    const chartSegment = pieChart.locator('svg path').first();
    await chartSegment.hover();
    
    const tooltip = page.locator('[data-testid="chart-tooltip"]');
    await expect(tooltip).toBeVisible();
  });
});

test.describe('Admin Portal - Dashboard Data Refresh', () => {
  test.beforeEach(async ({ page }) => {
    await loginAsAdmin(page);
    await page.goto('/admin/dashboard');
  });

  test('should display last updated timestamp', async ({ page }) => {
    const timestamp = page.locator('[data-testid="last-updated"]');
    await expect(timestamp).toBeVisible();
    await expect(timestamp).toContainText(/atualizado/i);
  });

  test('should have refresh button', async ({ page }) => {
    const refreshButton = page.locator('[data-testid="refresh-dashboard"]');
    await expect(refreshButton).toBeVisible();
  });

  test('should refresh data on button click', async ({ page }) => {
    const refreshButton = page.locator('[data-testid="refresh-dashboard"]');
    const kpiValueLocator = page.locator('[data-testid="kpi-total-providers"] [data-testid="kpi-value"]');
    
    const kpiValueBefore = await kpiValueLocator.textContent();
    
    await refreshButton.click();
    
    await kpiValueLocator.waitFor({ state: 'visible' });
    
    const kpiValueAfter = await kpiValueLocator.textContent();
    expect(kpiValueAfter).not.toBe(kpiValueBefore);
  });
});

test.describe('Admin Portal - Dashboard Error Handling', () => {
  test.beforeEach(async ({ page }) => {
    await loginAsAdmin(page);
    await page.goto('/admin/dashboard');
  });

  test('should display loading state while fetching data', async ({ page }) => {
    await page.goto('/admin/dashboard');
    
    const loadingSpinner = page.locator('[data-testid="dashboard-loading"]');
    await expect(loadingSpinner).toBeVisible({ timeout: 10000 }).catch(() => {});
  });

  test('should handle API error gracefully', async ({ page }) => {
    await page.route('**/api/**', async (route) => {
      await route.fulfill({
        status: 500,
        body: JSON.stringify({ error: 'Internal Server Error' })
      });
    });
    
    await page.goto('/admin/dashboard');
    
    const errorMessage = page.locator('[data-testid="dashboard-error"]');
    
    await expect(errorMessage).toContainText(/erro|falha|tentar novamente/i);
    
    const retryButton = page.locator('[data-testid="retry-button"]');
    await expect(retryButton).toBeVisible();
  });
});
