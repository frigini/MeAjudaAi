import { describe, it, expect } from 'vitest';
import { render, screen } from '@testing-library/react';
import { VerifiedBadge } from '@/components/ui/verified-badge';
import { EVerificationStatus } from '@/types/api/provider';

describe('VerifiedBadge', () => {
  it('deve renderizar badge verificado', () => {
    render(<VerifiedBadge status={EVerificationStatus.Verified} />);
    expect(screen.getByTitle('Prestador Verificado')).toBeInTheDocument();
  });

  it('deve renderizar badge rejeitado', () => {
    render(<VerifiedBadge status={EVerificationStatus.Rejected} />);
    expect(screen.getByTitle('Prestador Rejeitado')).toBeInTheDocument();
  });

  it('deve renderizar badge suspenso', () => {
    render(<VerifiedBadge status={EVerificationStatus.Suspended} />);
    expect(screen.getByTitle('Conta Suspensa')).toBeInTheDocument();
  });

  it('deve renderizar badge em progresso', () => {
    render(<VerifiedBadge status={EVerificationStatus.InProgress} />);
    expect(screen.getByTitle('Verificação em Andamento')).toBeInTheDocument();
  });

  it('deve renderizar badge pendente', () => {
    render(<VerifiedBadge status={EVerificationStatus.Pending} />);
    expect(screen.getByTitle('Pendente de Verificação')).toBeInTheDocument();
  });

  it('deve renderizar com label', () => {
    render(<VerifiedBadge status={EVerificationStatus.Verified} showLabel />);
    expect(screen.getByText('Verificado')).toBeInTheDocument();
  });

  it('deve renderizar com tamanho small', () => {
    render(<VerifiedBadge status={EVerificationStatus.Verified} size="sm" />);
    expect(screen.getByTitle('Prestador Verificado')).toBeInTheDocument();
  });

  it('deve renderizar com tamanho large', () => {
    render(<VerifiedBadge status={EVerificationStatus.Verified} size="lg" />);
    expect(screen.getByTitle('Prestador Verificado')).toBeInTheDocument();
  });

  it('não deve renderizar quando status undefined', () => {
    const { container } = render(<VerifiedBadge status={undefined} />);
    expect(container.firstChild).toBeNull();
  });

  it('não deve renderizar quando status null', () => {
    const { container } = render(<VerifiedBadge status={null as any} />);
    expect(container.firstChild).toBeNull();
  });
});
