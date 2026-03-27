import { test, expect } from '@playwright/test';

const mobileViewport = { width: 375, height: 667 };

test.describe('@e2e Customer Web App - Mobile Responsiveness', () => {
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
    expect(box).not.toBeNull();
    if (!box) throw new Error('userMenuButton has no layout/bounding box');
    
    expect(box.height).toBeGreaterThanOrEqual(44);
    expect(box.width).toBeGreaterThanOrEqual(44);
  });

  test('should adapt forms for mobile', async ({ page }) => {
    await page.setViewportSize(mobileViewport);
    await page.goto('/buscar');
    
    const formInputs = page.locator('input, select, textarea');
    const count = await formInputs.count();
    expect(count).toBeGreaterThan(0);
    
    const firstInput = formInputs.first();
    const box = await firstInput.boundingBox();
    expect(box).not.toBeNull();
    if (!box) throw new Error('firstInput has no layout/bounding box');
    expect(box.width).toBeLessThanOrEqual(mobileViewport.width - 32);
  });
});

test.describe('@e2e Performance - Core Web Vitals', () => {
  test('should meet LCP threshold on homepage', async ({ page, browser }) => {
    const browserName = browser.browserType().name();
    if (browserName !== 'chromium') {
      test.skip();
    }
    
    await page.goto('/');
    
    const supported = await page.evaluate((): boolean => {
      return PerformanceObserver.supportedEntryTypes.includes('largest-contentful-paint');
    });
    
    if (!supported) {
      test.skip();
    }
    
    const metrics = await page.evaluate((): Promise<{ lcp: number | null }> => {
      return new Promise((resolve) => {
        let resolved = false;
        
        const timeoutId = setTimeout(() => {
          if (!resolved) {
            resolved = true;
            observer.disconnect();
            resolve({ lcp: null });
          }
        }, 5000);

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
      });
    });
    
    expect(metrics.lcp).not.toBeNull();
    expect(metrics.lcp).toBeLessThan(800);
  });

  test('should meet INP threshold', async ({ page, browser }) => {
    const browserName = browser.browserType().name();
    if (browserName !== 'chromium') {
      test.skip();
    }
    
    await page.goto('/');
    
    await page.waitForLoadState('domcontentloaded');
    
    await page.evaluate(() => {
      const button = document.createElement('button');
      button.id = 'inp-test-button';
      button.textContent = 'Test';
      document.body.appendChild(button);
    });
    
    const supported = await page.evaluate((): boolean => {
      return PerformanceObserver.supportedEntryTypes.includes('event');
    });
    
    if (!supported) {
      test.skip();
    }
    
    // PerformanceEventTiming - use real click from test harness
    const metricsPromise = page.evaluate((): Promise<{ inp: number; samples: number }> => {
      return new Promise((resolve) => {
        const inpEntries: number[] = [];
        const observer = new PerformanceObserver((list) => {
          for (const entry of list.getEntries()) {
            if (entry.entryType === 'event') {
              const eventEntry = entry as unknown as { processingStart: number; startTime: number; duration: number };
              const inp = eventEntry.processingStart > 0
                ? eventEntry.processingStart - eventEntry.startTime
                : eventEntry.duration;
              inpEntries.push(inp);
            }
          }
        });
        observer.observe({ type: 'event', buffered: true, durationThreshold: 0 } as Parameters<PerformanceObserver['observe']>[0]);
        
        // Resolve after some time to capture events
        setTimeout(() => {
          observer.disconnect();
          const maxInp = inpEntries.length > 0 ? Math.max(...inpEntries) : 0;
          resolve({ inp: maxInp, samples: inpEntries.length });
        }, 3000); 
      });
    });
    
    // Trigger the interaction while the observer is active
    await page.click('#inp-test-button');
    
    const metrics = await metricsPromise;
    
    expect(metrics.samples).toBeGreaterThan(0);
    expect(metrics.inp).toBeLessThan(150);
  });

  test('should meet CLS threshold', async ({ page, browser }) => {
    const browserName = browser.browserType().name();
    if (browserName !== 'chromium') {
      test.skip();
    }
    
    await page.goto('/');
    await page.waitForLoadState('networkidle');
    
    const supported = await page.evaluate((): boolean => {
      return PerformanceObserver.supportedEntryTypes.includes('layout-shift');
    });
    
    if (!supported) {
      test.skip();
    }
    
    const metrics = await page.evaluate((): { cls: number } => {
      const entries = performance.getEntriesByType('layout-shift') as unknown as { hadRecentInput: boolean; startTime: number; value: number }[];
      
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
    
    expect(loadTime).toBeLessThan(1500);
  });
});

test.describe('@e2e Performance - Network', () => {
  let requests: string[] = [];

  test.beforeEach(() => {
    requests = [];
  });

  test('should optimize images', async ({ page }) => {
    await page.goto('/');
    
    const images = await page.locator('img').evaluateAll((imgs) => {
      const viewportHeight = window.innerHeight;
      return imgs.map((img) => {
        const image = img as HTMLImageElement;
        return {
          src: image.src,
          naturalWidth: image.naturalWidth,
          loading: image.loading,
          isBelowFold: image.getBoundingClientRect().top > viewportHeight
        };
      });
    });
    
    const imagesWithSrc = images.filter((img) => img.src && img.naturalWidth > 0);
    expect(imagesWithSrc.length).toBeGreaterThan(0);
    
    const imagesBelowFold = imagesWithSrc.filter((img) => img.isBelowFold);
    if (imagesBelowFold.length > 0) {
      const lazyLoadedImages = imagesBelowFold.filter((img) => img.loading === 'lazy');
      expect(lazyLoadedImages.length).toBe(imagesBelowFold.length);
    }
  });

  test('should not have excessive same-origin requests', async ({ page, baseURL }) => {
    const origin = new URL('/', baseURL).origin;
    
    page.on('request', (request) => {
      const url = request.url();
      if (url.startsWith(origin)) {
        requests.push(url);
      }
    });
    
    await page.goto('/');
    
    await page.waitForLoadState('networkidle');
    
    expect(requests.length).toBeLessThan(50);
  });
});
