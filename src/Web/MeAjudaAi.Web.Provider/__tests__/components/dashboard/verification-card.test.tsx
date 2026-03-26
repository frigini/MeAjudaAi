import { describe, it, expect, vi } from 'vitest';
import { render, screen } from 'test-support';
import { VerificationCard } from '@/components/dashboard/verification-card';

vi.mock('next/link', () => ({
  default: ({ children, href }: { children: React.ReactNode; href: string }) => (
    <a href={href}>{children}</a>
  ),
}));

describe('VerificationCard', () => {
  it('deve renderizar o título Documentos', () => {
    render(<VerificationCard />);
    expect(screen.getByText('Documentos')).toBeInTheDocument();
  });

  it('deve exibir mensagem sobre envio de documentos', () => {
    render(<VerificationCard />);
    expect(screen.getByText(/Sua conta requer envio de documentos/i)).toBeInTheDocument();
  });

  it('deve conter link para upload', () => {
    render(<VerificationCard />);
    const link = screen.getByRole('link', { name: /Fazer Upload/i });
    expect(link).toBeInTheDocument();
    expect(link).toHaveAttribute('href', '/onboarding/documents');
  });
});
