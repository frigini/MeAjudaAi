import { test, expect, devices } from '@playwright/test';

const mobileViewport = { width: 375, height: 667 };

test.describe('Customer Web App - Mobile Responsiveness', () => {
  test('should render correctly on mobile viewport', async ({ page }) => {
    await page.setViewportSize(mobileViewport);
    await page.goto('/');
    
    await expect(page.locator('header')).toBeVisible();
    await expect(page.getByRole('link', { name: /meajudaaí/i })).toBeVisible();
  });

  test('should have touch-friendly tap targets on mobile', async ({ page }) => {
    await page.setViewportSize(mobileViewport);
    await page.goto('/');
    
    const userMenuButton = page.locator('button').first();
    const count = await userMenuButton.count();
    expect(count).toBeGreaterThan(0);
    
    const box = await userMenuButton.boundingBox();
    
    expect(box?.height).toBeGreaterThanOrEqual(44);
    expect(box?.width).toBeGreaterThanOrEqual(44);
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
        let timeoutId: NodeJS.Timeout;
        
        const observer = new PerformanceObserver((list) => {
          if (resolved) return;
          const entries = list.getEntries();
          const lcpEntry = entries[entries.length - 1];
          if (lcpEntry) {
            resolved = true;
            clearTimeout(timeoutId);
            observer.disconnect();
            resolve({ lcp: lcpEntry.startTime });
          }
        });
        observer.observe({ type: 'largest-contentful-paint', buffered: true });
        
        timeoutId = setTimeout(() => {
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
    
    await page.evaluate(() => {
      const button = document.createElement('button');
      button.id = 'inp-test-button';
      button.textContent = 'Test';
      document.body.appendChild(button);
    });
    
    const metrics = await page.evaluate(() => {
      return new Promise((resolve) => {
        const inpEntries: number[] = [];
        
        const observer = new PerformanceObserver((list) => {
          for (const entry of list.getEntries()) {
            if (entry.entryType === 'event') {
              const eventEntry = entry as PerformanceEventTiming;
              const inp = eventEntry.processingStart > 0
                ? eventEntry.processingStart - eventEntry.startTime
                : eventEntry.duration;
              inpEntries.push(inp);
            }
          }
        });
        
        observer.observe({ type: 'event', buffered: true });
        
        setTimeout(() => {
          observer.disconnect();
          const maxInp = inpEntries.length > 0 ? Math.max(...inpEntries) : 0;
          resolve({ inp: maxInp });
        }, 2000);
      });
    });
    
    await page.click('#inp-test-button');
    
    expect(metrics.inp).toBeDefined();
    expect(metrics.inp).toBeLessThan(500);
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
      expect(lazyLoadedImages.length).toBe(imagesBelowFold.length);
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
