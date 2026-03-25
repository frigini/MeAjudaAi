import { test, expect, devices } from '@playwright/test';

const desktopViewport = { width: 1920, height: 1080 };
const tabletViewport = { width: 768, height: 1024 };
const mobileViewport = { width: 375, height: 667 };

test.describe('Customer Web App - Mobile Responsiveness', () => {
  test('should render correctly on mobile viewport', async ({ page }) => {
    await page.setViewportSize(mobileViewport);
    await page.goto('/');
    
    await expect(page.locator('[data-testid="mobile-menu"]')).toBeVisible();
    await expect(page.locator('nav')).toBeVisible();
  });

  test('should have touch-friendly tap targets on mobile', async ({ page }) => {
    await page.setViewportSize(mobileViewport);
    await page.goto('/');
    
    const searchButton = page.locator('button:has-text("Buscar")').first();
    const box = await searchButton.boundingBox();
    
    expect(box?.height).toBeGreaterThanOrEqual(44);
    expect(box?.width).toBeGreaterThanOrEqual(44);
  });

  test('should display mobile-friendly navigation', async ({ page }) => {
    await page.setViewportSize(mobileViewport);
    await page.goto('/');
    
    await page.click('[data-testid="mobile-menu-toggle"]');
    await expect(page.locator('[data-testid="mobile-nav"]')).toBeVisible();
  });

  test('should adapt forms for mobile', async ({ page }) => {
    await page.setViewportSize(mobileViewport);
    await page.goto('/buscar');
    
    const formInputs = page.locator('input, select, textarea');
    const count = await formInputs.count();
    expect(count).toBeGreaterThan(0);
    
    const firstInput = formInputs.first();
    const box = await firstInput.boundingBox();
    expect(box?.width).toBeLessThanOrEqual(mobileViewport.width - 32);
  });
});

test.describe('Performance - Core Web Vitals', () => {
  test('should meet LCP threshold on homepage', async ({ page }) => {
    await page.goto('/');
    
    const metrics = await page.evaluate(() => {
      return new Promise((resolve) => {
        let resolved = false;
        const observer = new PerformanceObserver((list) => {
          if (resolved) return;
          const entries = list.getEntries();
          const lcpEntry = entries[entries.length - 1];
          if (lcpEntry) {
            resolved = true;
            observer.disconnect();
            resolve({ lcp: lcpEntry.startTime });
          }
        });
        observer.observe({ type: 'largest-contentful-paint', buffered: true });
        
        const timeoutId = setTimeout(() => {
          if (!resolved) {
            resolved = true;
            observer.disconnect();
            resolve({ lcp: null });
          }
        }, 5000);
      });
    });
    
    expect(metrics.lcp).toBeDefined();
    expect(metrics.lcp).toBeGreaterThan(0);
    expect(metrics.lcp).toBeLessThan(2500);
  });

  test('should meet INP threshold', async ({ page }) => {
    await page.goto('/');
    
    await page.waitForLoadState('domcontentloaded');
    
    const metrics = await page.evaluate(() => {
      return new Promise((resolve) => {
        let resolved = false;
        const observer = new PerformanceObserver((list) => {
          if (resolved) return;
          const entries = list.getEntries();
          const eventEntries = entries.filter((entry) => entry.entryType === 'event');
          
          let maxInp = 0;
          eventEntries.forEach((entry: any) => {
            const processingStart = entry.processingStart || 0;
            const inp = processingStart > 0 
              ? processingStart - entry.startTime 
              : entry.duration;
            if (inp > maxInp) {
              maxInp = inp;
            }
          });
          
          if (eventEntries.length > 0) {
            resolved = true;
            observer.disconnect();
            resolve({ inp: maxInp });
          }
        });
        observer.observe({ type: 'event', buffered: true, durationThreshold: 0 });
        
        document.body.click();
        
        setTimeout(() => {
          if (!resolved) {
            resolved = true;
            observer.disconnect();
            resolve({ inp: null });
          }
        }, 5000);
      });
    });
    
    expect(metrics.inp).not.toBeNull();
    expect(metrics.inp).toBeLessThan(200);
  });

  test('should meet CLS threshold', async ({ page }) => {
    await page.goto('/');
    await page.waitForLoadState('networkidle');
    
    const metrics = await page.evaluate(() => {
      const entries = performance.getEntriesByType('layout-shift') as any[];
      
      const validEntries = entries.filter((entry) => !entry.hadRecentInput);
      
      validEntries.sort((a, b) => a.startTime - b.startTime);
      
      let maxCls = 0;
      let currentWindowSum = 0;
      let windowStartTime = 0;
      let lastEntryTime = 0;
      
      validEntries.forEach((entry) => {
        if (windowStartTime === 0) {
          windowStartTime = entry.startTime;
          currentWindowSum = entry.value;
        } else {
          const gap = entry.startTime - lastEntryTime;
          const sessionDuration = entry.startTime - windowStartTime;
          
          if (gap >= 1000 || sessionDuration >= 5000) {
            if (currentWindowSum > maxCls) {
              maxCls = currentWindowSum;
            }
            windowStartTime = entry.startTime;
            currentWindowSum = entry.value;
          } else {
            currentWindowSum += entry.value;
          }
        }
        lastEntryTime = entry.startTime;
      });
      
      if (currentWindowSum > maxCls) {
        maxCls = currentWindowSum;
      }
      
      return { cls: maxCls };
    });
    
    expect(metrics.cls).toBeLessThan(0.1);
  });

  test('should load page within acceptable time', async ({ page }) => {
    const startTime = Date.now();
    await page.goto('/', { waitUntil: 'domcontentloaded' });
    const loadTime = Date.now() - startTime;
    
    expect(loadTime).toBeLessThan(3000);
  });
});

test.describe('Performance - Network', () => {
  test('should optimize images', async ({ page }) => {
    await page.goto('/');
    
    const images = await page.locator('img').evaluateAll((imgs) => {
      const viewportHeight = window.innerHeight;
      return imgs.map((img) => ({
        src: img.src,
        naturalWidth: img.naturalWidth,
        loading: img.loading,
        isBelowFold: img.getBoundingClientRect().top > viewportHeight
      }));
    });
    
    const imagesWithSrc = images.filter((img) => img.src && img.naturalWidth > 0);
    expect(imagesWithSrc.length).toBeGreaterThan(0);
    
    const imagesBelowFold = imagesWithSrc.filter((img) => img.isBelowFold);
    if (imagesBelowFold.length > 0) {
      const lazyLoadedImages = imagesBelowFold.filter((img) => img.loading === 'lazy');
      expect(lazyLoadedImages.length).toBeGreaterThan(0);
    }
  });

  test('should not have excessive same-origin requests', async ({ page }) => {
    const requests: string[] = [];
    page.on('request', (request) => {
      const url = request.url();
      if (url.includes('localhost') || url.includes('127.0.0.1')) {
        requests.push(url);
      }
    });
    
    await page.goto('/');
    await page.waitForLoadState('networkidle');
    
    expect(requests.length).toBeLessThan(50);
  });
});
