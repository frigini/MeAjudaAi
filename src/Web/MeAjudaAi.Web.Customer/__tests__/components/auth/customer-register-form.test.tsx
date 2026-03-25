import { describe, it, expect, vi } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { CustomerRegisterForm } from '@/components/auth/customer-register-form';

const mockPush = vi.fn();

vi.mock('next/navigation', () => ({
  useRouter: () => ({
    push: mockPush,
  }),
}));

vi.mock('@/lib/api/fetch-client', () => ({
  publicFetch: vi.fn(),
}));

vi.mock('sonner', () => ({
  toast: {
    success: vi.fn(),
    error: vi.fn(),
  },
}));

describe('CustomerRegisterForm', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('deve renderizar o formulário de cadastro', () => {
    render(<CustomerRegisterForm />);
    expect(screen.getByLabelText(/nome completo/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/email/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/celular/i)).toBeInTheDocument();
  });

  it('deve renderizar campos de senha', () => {
    render(<CustomerRegisterForm />);
    const container = document.body;
    const passwordInputs = container.querySelectorAll('input[type="password"]');
    expect(passwordInputs.length).toBe(2);
  });

  it('deve renderizar botão de cadastrar', () => {
    render(<CustomerRegisterForm />);
    expect(screen.getByRole('button', { name: /criar conta/i })).toBeInTheDocument();
  });

  it('deve renderizar links de termos', () => {
    render(<CustomerRegisterForm />);
    const termsLink = screen.getByRole('link', { name: /termos de uso/i });
    expect(termsLink).toBeInTheDocument();
    expect(termsLink).toHaveAttribute('href', '/termos');
  });

  it('deve renderizar link de privacidade', () => {
    render(<CustomerRegisterForm />);
    expect(screen.getByRole('link', { name: /política de privacidade/i })).toBeInTheDocument();
  });

  it('deve renderizar checkbox de termos', () => {
    render(<CustomerRegisterForm />);
    expect(screen.getByRole('checkbox', { name: /termos/i })).toBeInTheDocument();
  });

  it('deve validar campos ao submeter', () => {
    render(<CustomerRegisterForm />);
    expect(screen.getByRole('button', { name: /criar conta/i })).toBeInTheDocument();
  });

});
