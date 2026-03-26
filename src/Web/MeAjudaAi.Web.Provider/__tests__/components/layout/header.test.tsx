import { describe, it, expect, vi } from 'vitest';
import { render, screen, fireEvent } from 'test-support';
import { Header } from '@/components/layout/header';
import { signIn, signOut, useSession } from 'next-auth/react';

vi.mock('next-auth/react', () => ({
  useSession: vi.fn(),
  signIn: vi.fn(),
  signOut: vi.fn(),
}));

describe('Header (Provider)', () => {
  it('deve renderizar o logo e imagens principais', () => {
    vi.mocked(useSession).mockReturnValue({ data: null, status: 'unauthenticated' } as any);
    render(<Header />);
    expect(screen.getByAltText('MeAjudaAí')).toBeInTheDocument();
  });

  it('deve mostrar botão de entrar quando deslogado', () => {
    vi.mocked(useSession).mockReturnValue({ data: null, status: 'unauthenticated' } as any);
    render(<Header />);
    expect(screen.getByText(/entrar/i)).toBeInTheDocument();
  });

  it('deve chamar signIn ao clicar em Entrar', () => {
    vi.mocked(useSession).mockReturnValue({ data: null, status: 'unauthenticated' } as any);
    render(<Header />);
    fireEvent.click(screen.getByText(/entrar/i));
    expect(signIn).toHaveBeenCalledWith('keycloak');
  });

  it('deve mostrar link de configurações e botão sair quando logado', () => {
    vi.mocked(useSession).mockReturnValue({ 
      data: { user: { name: 'João' } }, 
      status: 'authenticated' 
    } as any);
    render(<Header />);
    expect(screen.getByText(/configurações/i)).toBeInTheDocument();
    expect(screen.getByText(/sair/i)).toBeInTheDocument();
  });

  it('deve chamar signOut ao clicar em Sair', () => {
    vi.mocked(useSession).mockReturnValue({ 
      data: { user: { name: 'João' } }, 
      status: 'authenticated' 
    } as any);
    render(<Header />);
    fireEvent.click(screen.getByText(/sair/i));
    expect(signOut).toHaveBeenCalledWith({ callbackUrl: '/' });
  });

  it('deve mostrar estado de carregamento', () => {
    vi.mocked(useSession).mockReturnValue({ data: null, status: 'loading' } as any);
    const { container } = render(<Header />);
    expect(container.querySelector('.animate-pulse')).toBeInTheDocument();
  });
});
