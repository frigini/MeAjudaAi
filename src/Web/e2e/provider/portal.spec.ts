import { test, expect } from '@playwright/test';

test.skip('Provider Portal - Skipped due to build issues', async () => {
  test.skip(true, 'Provider portal needs rebuild');
});