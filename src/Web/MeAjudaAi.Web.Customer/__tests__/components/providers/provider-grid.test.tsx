import { describe, it, expect } from 'vitest';
import { render, screen } from '@testing-library/react';
import { ProviderGrid } from '@/components/providers/provider-grid';
import { ProviderDto, EProviderType, EProviderStatus, EVerificationStatus, EProviderTier } from '@/types/api/provider';

const mockProviders: ProviderDto[] = [
  {
    id: 'provider-1',
    name: 'João Silva',
    userId: 'user-1',
    email: 'joao@teste.com',
    type: EProviderType.Individual,
    status: EProviderStatus.Active,
    verificationStatus: EVerificationStatus.Verified,
    tier: EProviderTier.Standard,
    averageRating: 4.5,
    reviewCount: 10,
    city: 'Rio de Janeiro',
    state: 'RJ',
    avatarUrl: null,
    services: [{ serviceId: 's1', serviceName: 'Elétrica' }],
    businessProfile: null,
    documents: [],
    qualifications: [],
    createdAt: new Date().toISOString(),
  },
  {
    id: 'provider-2',
    name: 'Maria Santos',
    userId: 'user-2',
    email: 'maria@teste.com',
    type: EProviderType.Individual,
    status: EProviderStatus.Active,
    verificationStatus: EVerificationStatus.Verified,
    tier: EProviderTier.Standard,
    averageRating: 5,
    reviewCount: 25,
    city: 'São Paulo',
    state: 'SP',
    avatarUrl: null,
    services: [{ serviceId: 's2', serviceName: 'Hidráulica' }],
    businessProfile: null,
    documents: [],
    qualifications: [],
    createdAt: new Date().toISOString(),
  },
];

describe('ProviderGrid', () => {
  it('deve renderizar prestadores', () => {
    render(<ProviderGrid providers={mockProviders} />);
    expect(screen.getByText('João Silva')).toBeInTheDocument();
    expect(screen.getByText('Maria Santos')).toBeInTheDocument();
  });

  it('deve renderizar mensagem vazia quando não há prestadores', () => {
    render(<ProviderGrid providers={[]} />);
    expect(screen.getByText('Nenhum prestador encontrado.')).toBeInTheDocument();
  });

  it('deve renderizar mensagem vazia customizada', () => {
    render(<ProviderGrid providers={[]} emptyMessage="Nenhum resultado." />);
    expect(screen.getByText('Nenhum resultado.')).toBeInTheDocument();
  });
});
