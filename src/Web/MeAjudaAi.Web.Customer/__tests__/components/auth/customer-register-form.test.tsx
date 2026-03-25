import { describe, it, expect, vi } from 'vitest';
import { render, screen } from '@testing-library/react';
import { CustomerRegisterForm } from '@/components/auth/customer-register-form';

vi.mock('next/navigation', () => ({
  useRouter: () => ({
    push: vi.fn(),
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
  it('deve renderizar o formulário de cadastro', () => {
    render(<CustomerRegisterForm />);
    expect(screen.getByLabelText(/nome completo/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/email/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/celular/i)).toBeInTheDocument();
  });

  it('deve renderizar campos de senha', () => {
    render(<CustomerRegisterForm />);
    expect(screen.getAllByText(/senha/i).length).toBeGreaterThan(0);
  });

  it('deve renderizar botão de cadastrar', () => {
    render(<CustomerRegisterForm />);
    expect(screen.getByRole('button', { name: /criar conta/i })).toBeInTheDocument();
  });

  it('deve renderizar links de termos', () => {
    render(<CustomerRegisterForm />);
    expect(screen.getByRole('link', { name: /termos de uso/i })).toBeInTheDocument();
  });
});
