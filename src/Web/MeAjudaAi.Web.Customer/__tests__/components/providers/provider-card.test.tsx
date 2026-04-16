import { describe, it, expect } from 'vitest';
import { render, screen } from '@testing-library/react';
import { ProviderCard } from '@/components/providers/provider-card';
import { EProviderType, EProviderStatus, EVerificationStatus, EProviderTier, ProviderDto } from '@/types/api/provider';

const mockProvider: ProviderDto = {
  id: 'provider-1',
  name: 'João Silva',
  userId: 'user-1',
  email: 'joao@teste.com',
  type: EProviderType.Individual,
  status: EProviderStatus.Active,
  verificationStatus: EVerificationStatus.Verified,
  tier: EProviderTier.Standard,
  averageRating: 4.5,
  reviewCount: 25,
  city: 'Rio de Janeiro',
  state: 'RJ',
  avatarUrl: '/avatar.jpg',
  services: [
    { serviceId: 's1', serviceName: 'Elétrica' },
    { serviceId: 's2', serviceName: 'Hidráulica' },
  ],
  businessProfile: {
    legalName: 'João Silva ME',
    contactInfo: { email: 'joao@teste.com' },
    primaryAddress: { city: 'RJ', state: 'RJ', street: '', neighborhood: '', zipCode: '', country: '' },
  },
  documents: [],
  qualifications: [],
  createdAt: new Date().toISOString(),
};

describe('ProviderCard', () => {
  it('deve renderizar nome do prestador', () => {
    render(<ProviderCard provider={mockProvider} />);
    expect(screen.getByText('João Silva')).toBeInTheDocument();
  });

  it('deve renderizar avaliação com número de avaliações', () => {
    render(<ProviderCard provider={mockProvider} />);
    expect(screen.getByText('(provider.review_other)')).toBeInTheDocument();
  });

  it('deve renderizar avaliação singular', () => {
    const providerWithOneReview: ProviderDto = {
      ...mockProvider,
      reviewCount: 1,
      averageRating: 5,
    };
    render(<ProviderCard provider={providerWithOneReview} />);
    expect(screen.getByText('(provider.review_one)')).toBeInTheDocument();
  });

  it('deve renderizar serviços', () => {
    render(<ProviderCard provider={mockProvider} />);
    expect(screen.getByText('Elétrica')).toBeInTheDocument();
    expect(screen.getByText('Hidráulica')).toBeInTheDocument();
  });

  it('deve renderizar localização', () => {
    render(<ProviderCard provider={mockProvider} />);
    expect(screen.getByText(/rio de janeiro, rj/i)).toBeInTheDocument();
  });

  it('deve renderizar badge de mais serviços quando há mais de 3', () => {
    const providerWithManyServices: ProviderDto = {
      ...mockProvider,
      services: [
        { serviceId: 's1', serviceName: 'Elétrica' },
        { serviceId: 's2', serviceName: 'Hidráulica' },
        { serviceId: 's3', serviceName: 'Pintura' },
        { serviceId: 's4', serviceName: 'Gesso' },
      ],
    };
    render(<ProviderCard provider={providerWithManyServices} />);
    expect(screen.getByText('+1')).toBeInTheDocument();
  });

  it('deve renderizar link para perfil do prestador', () => {
    render(<ProviderCard provider={mockProvider} />);
    const link = screen.getByRole('link');
    expect(link).toHaveAttribute('href', '/prestador/provider-1');
  });
});
