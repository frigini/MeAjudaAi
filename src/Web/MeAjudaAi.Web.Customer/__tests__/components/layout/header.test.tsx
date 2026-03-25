import { describe, it, expect, vi } from 'vitest';
import { render, screen } from '@testing-library/react';
import { Header } from '@/components/layout/header';

vi.mock('@/components/layout/user-menu', () => ({
  UserMenu: () => <div data-testid="user-menu">User Menu</div>,
}));

vi.mock('next/navigation', () => ({
  useRouter: () => ({
    push: vi.fn(),
  }),
  usePathname: vi.fn(() => '/'),
}));

vi.mock('next/image', () => ({
  default: ({ src, alt, ...props }: { src: string; alt: string }) => (
    <img src={src} alt={alt} {...props} />
  ),
}));

describe('Header', () => {
  it('deve renderizar o logo', () => {
    render(<Header />);
    const logoLink = screen.getByLabelText(/ir para a página inicial/i);
    expect(logoLink).toBeInTheDocument();
  });

  it('deve renderizar o menu do usuário', () => {
    render(<Header />);
    expect(screen.getByTestId('user-menu')).toBeInTheDocument();
  });

  it('deve renderizar campo de busca na página inicial', () => {
    render(<Header />);
    expect(screen.getByPlaceholderText(/buscar serviço/i)).toBeInTheDocument();
  });

  it('deve renderizar sem erros com className customizada', () => {
    render(<Header className="custom-class" />);
    expect(screen.getByLabelText(/ir para a página inicial/i)).toBeInTheDocument();
  });
});
