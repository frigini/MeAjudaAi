import { describe, it, expect, vi } from 'vitest';
import { render, screen } from 'test-support';

vi.mock('next/link', () => ({
  default: ({ children, href }: { children: React.ReactNode; href: string }) => (
    <a href={href}>{children}</a>
  ),
}));

import { VerificationCard } from '@/components/dashboard/verification-card';

describe('VerificationCard', () => {
  it('deve renderizar o título "Documentos"', () => {
    render(<VerificationCard />);
    expect(screen.getByText('Documentos')).toBeInTheDocument();
  });

  it('deve exibir seção de Documentos de Identidade', () => {
    render(<VerificationCard />);
    expect(screen.getByText('Documentos de Identidade')).toBeInTheDocument();
  });

  it('deve exibir link para upload de documentos', () => {
    render(<VerificationCard />);
    const link = screen.getByRole('link', { name: /fazer upload/i });
    expect(link).toBeInTheDocument();
    expect(link).toHaveAttribute('href', '/onboarding/documents');
  });

  it('deve mencionar RG ou CNH', () => {
    render(<VerificationCard />);
    expect(screen.getByText(/rg ou cnh/i)).toBeInTheDocument();
  });
});
