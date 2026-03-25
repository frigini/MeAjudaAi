import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, waitFor, fireEvent } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { LoginForm } from '@/components/auth/login-form';

const mockSignIn = vi.fn();

vi.mock('next-auth/react', () => ({
  signIn: (...args: any[]) => mockSignIn(...args),
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
    mockSignIn.mockResolvedValue({ error: null, url: null });
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

  it('deve renderizar com className customizada', async () => {
    render(<LoginForm className="custom-class" />);
    
    await waitFor(() => {
      expect(screen.getByLabelText(/e-mail/i)).toBeInTheDocument();
    });
    
    const form = document.querySelector('.custom-class');
    expect(form).toBeInTheDocument();
  });
});
