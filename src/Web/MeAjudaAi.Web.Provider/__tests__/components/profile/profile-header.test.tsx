import { describe, it, expect, vi } from 'vitest';
import { render, screen } from 'test-support';

vi.mock('next/link', () => ({
  default: ({ children, href }: { children: React.ReactNode; href: string }) => (
    <a href={href}>{children}</a>
  ),
}));

import { ProfileHeader } from '@/components/profile/profile-header';

const defaultProps = {
  name: 'Marcos Silva',
  email: 'marcos@exemplo.com',
  isOnline: true,
  phones: ['(32) 99999-1111'],
  rating: 4,
};

describe('ProfileHeader', () => {
  it('deve renderizar saudação com nome do prestador', () => {
    render(<ProfileHeader {...defaultProps} />);
    expect(screen.getByText(/Marcos Silva/i)).toBeInTheDocument();
  });

  it('deve exibir status online', () => {
    render(<ProfileHeader {...defaultProps} />);
    expect(screen.getByText(/online/i)).toBeInTheDocument();
  });

  it('deve exibir status desativado quando isOnline=false', () => {
    render(<ProfileHeader {...defaultProps} isOnline={false} />);
    expect(screen.getByText(/desativado/i)).toBeInTheDocument();
  });

  it('deve exibir email do prestador', () => {
    render(<ProfileHeader {...defaultProps} />);
    expect(screen.getByText('marcos@exemplo.com')).toBeInTheDocument();
  });

  it('deve exibir telefone', () => {
    render(<ProfileHeader {...defaultProps} />);
    expect(screen.getByText('(32) 99999-1111')).toBeInTheDocument();
  });

  it('deve ter link para editar perfil', () => {
    render(<ProfileHeader {...defaultProps} />);
    expect(screen.getByRole('link', { name: /editar perfil/i })).toBeInTheDocument();
  });

  it('deve ter link para configurações', () => {
    render(<ProfileHeader {...defaultProps} />);
    expect(screen.getByRole('link', { name: /configurações/i })).toBeInTheDocument();
  });
});
