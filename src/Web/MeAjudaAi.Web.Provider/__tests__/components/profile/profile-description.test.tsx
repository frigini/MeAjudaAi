import { describe, it, expect, vi } from 'vitest';
import { render, screen } from 'test-support';
import { ProfileDescription } from '@/components/profile/profile-description';

vi.mock('next/link', () => ({
  default: ({ children, href }: { children: React.ReactNode; href: string }) => (
    <a href={href}>{children}</a>
  ),
}));

describe('ProfileDescription', () => {
  it('deve renderizar a descrição fornecida', () => {
    const desc = 'Sou um profissional dedicado com 10 anos de experiência.';
    render(<ProfileDescription description={desc} />);
    expect(screen.getByText(desc)).toBeInTheDocument();
  });

  it('deve conter link para editar descrição', () => {
    render(<ProfileDescription description="Teste" />);
    const link = screen.getByRole('link', { name: /Editar descrição/i });
    expect(link).toBeInTheDocument();
    expect(link).toHaveAttribute('href', '/alterar-dados#description');
  });
});
