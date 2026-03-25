import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import { LoginForm } from '@/components/auth/login-form';

vi.mock('next-auth/react', () => ({
  signIn: vi.fn(() => Promise.resolve({ error: null, url: null })),
  useSession: vi.fn(() => ({ data: null, status: 'unauthenticated' })),
}));

vi.mock('@/lib/api/fetch-client', () => ({
  baseFetch: vi.fn(() => Promise.resolve(['google', 'facebook'])),
}));

vi.mock('next/navigation', () => ({
  useSearchParams: vi.fn(() => new URLSearchParams()),
}));

describe('LoginForm', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('deve renderizar formulário de login', async () => {
    render(<LoginForm />);
    
    await waitFor(() => {
      expect(screen.getByLabelText(/e-mail/i)).toBeInTheDocument();
    });
    
    expect(screen.getByRole('button', { name: /^entrar$/i })).toBeInTheDocument();
  });

  it('deve renderizar link para criar conta', async () => {
    render(<LoginForm />);
    
    await waitFor(() => {
      expect(screen.getByText(/crie sua conta grátis/i)).toBeInTheDocument();
    });
  });

  it('deve renderizar botão de login com Google', async () => {
    render(<LoginForm />);
    
    await waitFor(() => {
      expect(screen.getByRole('button', { name: /entrar com o google/i })).toBeInTheDocument();
    });
  });
});
