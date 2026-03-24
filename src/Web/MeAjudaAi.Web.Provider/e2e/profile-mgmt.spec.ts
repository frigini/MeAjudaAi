import { test, expect, loginAsProvider, logout } from '@meajudaai/web-e2e-support';

test.describe('Provider Web App - Profile Management', () => {
  test.beforeEach(async ({ page }) => {
    await loginAsProvider(page);
    await page.goto('/provider/perfil');
  });

  test('should display profile information', async ({ page }) => {
    await expect(page.locator('[data-testid="profile-header"]')).toBeVisible();
    await expect(page.locator('input[name="name"]')).toBeVisible();
    await expect(page.locator('input[name="phone"]')).toBeVisible();
    await expect(page.locator('input[name="email"]')).toBeVisible();
  });

  test('should update profile information', async ({ page }) => {
    await page.fill('input[name="name"]', 'João Silva Atualizado');
    await page.fill('input[name="phone"]', '21988888888');
    await page.click('button:has-text("Salvar")');
    
    await expect(page.locator('text=Perfil atualizado com sucesso')).toBeVisible();
  });

  test('should update profile photo', async ({ page }) => {
    const photoInput = page.locator('input[type="file"][accept*="image"]');
    await photoInput.setInputFiles({
      name: 'profile-photo.jpg',
      mimeType: 'image/jpeg',
      buffer: Buffer.from('dummy image data')
    });
    
    await expect(page.locator('text=Foto atualizada com sucesso')).toBeVisible();
  });

  test('should validate required fields', async ({ page }) => {
    await page.fill('input[name="name"]', '');
    await page.click('button:has-text("Salvar")');
    await expect(page.locator('text=Nome é obrigatório')).toBeVisible();
  });

  test('should validate phone format', async ({ page }) => {
    await page.fill('input[name="phone"]', 'invalid');
    await page.click('button:has-text("Salvar")');
    await expect(page.locator(/telefone inválido/i)).toBeVisible();
  });
});

test.describe('Provider Web App - Profile Visibility Settings', () => {
  test.beforeEach(async ({ page }) => {
    await loginAsProvider(page);
    await page.goto('/provider/perfil/visibilidade');
  });

  test('should display visibility toggles', async ({ page }) => {
    await expect(page.locator('[data-testid="visibility-settings"]')).toBeVisible();
    await expect(page.locator('input[type="checkbox"]')).toBeVisible();
  });

  test('should toggle profile visibility', async ({ page }) => {
    const visibilityToggle = page.locator('[data-testid="profile-visibility-toggle"]');
    const isChecked = await visibilityToggle.isChecked();
    
    await visibilityToggle.click();
    
    if (isChecked) {
      await expect(page.locator('text=Perfil ocultado dos clientes')).toBeVisible();
    } else {
      await expect(page.locator('text=Perfil visível para clientes')).toBeVisible();
    }
  });

  test('should toggle phone visibility', async ({ page }) => {
    const phoneToggle = page.locator('[data-testid="phone-visibility-toggle"]');
    const isChecked = await phoneToggle.isChecked();
    
    await phoneToggle.click();
    
    if (isChecked) {
      await expect(page.locator('text=Telefone oculto')).toBeVisible();
    } else {
      await expect(page.locator('text=Telefone visível')).toBeVisible();
    }
  });

  test('should toggle WhatsApp visibility', async ({ page }) => {
    const whatsappToggle = page.locator('[data-testid="whatsapp-visibility-toggle"]');
    const isChecked = await whatsappToggle.isChecked();
    
    await whatsappToggle.click();
    
    if (isChecked) {
      await expect(page.locator('text=WhatsApp oculto')).toBeVisible();
    } else {
      await expect(page.locator('text=WhatsApp visível')).toBeVisible();
    }
  });

  test('should save visibility settings', async ({ page }) => {
    await page.click('button:has-text("Salvar Configurações")');
    await expect(page.locator('text=Configurações salvas com sucesso')).toBeVisible();
  });
});

test.describe('Provider Web App - LGPD Account Deletion', () => {
  test.beforeEach(async ({ page }) => {
    await loginAsProvider(page);
    await page.goto('/provider/perfil/privacidade');
  });

  test('should display LGPD options', async ({ page }) => {
    await expect(page.locator('[data-testid="lgpd-section"]')).toBeVisible();
    await expect(page.locator('text=Excluir minha conta')).toBeVisible();
    await expect(page.locator('text=LGPD')).toBeVisible();
  });

  test('should initiate account deletion flow', async ({ page }) => {
    await page.click('button:has-text("Excluir minha conta")');
    await expect(page.locator('[data-testid="deletion-confirmation"]')).toBeVisible();
  });

  test('should require confirmation for deletion', async ({ page }) => {
    await page.click('button:has-text("Excluir minha conta")');
    await page.click('button:has-text("Confirmar Exclusão")');
    
    await expect(page.locator(/confirme digitando/i)).toBeVisible();
  });

  test('should delete account with correct confirmation', async ({ page }) => {
    await page.click('button:has-text("Excluir minha conta")');
    
    const confirmationInput = page.locator('input[name="confirmationText"]');
    await confirmationInput.fill('EXCLUIR');
    
    await page.click('button:has-text("Confirmar Exclusão")');
    
    await expect(page).toHaveURL(/.*\/login/);
    await expect(page.locator('text=Conta excluída com sucesso')).toBeVisible();
  });

  test('should cancel deletion flow', async ({ page }) => {
    await page.click('button:has-text("Excluir minha cuenta")');
    await page.click('button:has-text("Cancelar")');
    
    await expect(page.locator('[data-testid="deletion-confirmation"]')).not.toBeVisible();
  });

  test('should display data export option', async ({ page }) => {
    await expect(page.locator('text=Exportar meus dados')).toBeVisible();
  });

  test('should request data export', async ({ page }) => {
    await page.click('button:has-text("Exportar meus dados")');
    await expect(page.locator('text=Solicitação de exportação enviada')).toBeVisible();
  });
});

test.describe('Provider Web App - Password Management', () => {
  test.beforeEach(async ({ page }) => {
    await loginAsProvider(page);
    await page.goto('/provider/perfil/senha');
  });

  test('should display password change form', async ({ page }) => {
    await expect(page.locator('input[name="currentPassword"]')).toBeVisible();
    await expect(page.locator('input[name="newPassword"]')).toBeVisible();
    await expect(page.locator('input[name="confirmPassword"]')).toBeVisible();
  });

  test('should update password successfully', async ({ page }) => {
    await page.fill('input[name="currentPassword"]', 'Senha@123');
    await page.fill('input[name="newPassword"]', 'NovaSenha@456');
    await page.fill('input[name="confirmPassword"]', 'NovaSenha@456');
    await page.click('button:has-text("Alterar Senha")');
    
    await expect(page.locator('text=Senha alterada com sucesso')).toBeVisible();
  });

  test('should validate password mismatch', async ({ page }) => {
    await page.fill('input[name="currentPassword"]', 'Senha@123');
    await page.fill('input[name="newPassword"]', 'NovaSenha@456');
    await page.fill('input[name="confirmPassword"]', 'SenhaDiferente@789');
    await page.click('button:has-text("Alterar Senha")');
    
    await expect(page.locator('text=Senhas não conferem')).toBeVisible();
  });

  test('should validate password strength', async ({ page }) => {
    await page.fill('input[name="currentPassword"]', 'Senha@123');
    await page.fill('input[name="newPassword"]', 'fraca');
    await page.fill('input[name="confirmPassword"]', 'fraca');
    await page.click('button:has-text("Alterar Senha")');
    
    await expect(page.locator(/senha fraca|caracteres mínimos/i)).toBeVisible();
  });
});
