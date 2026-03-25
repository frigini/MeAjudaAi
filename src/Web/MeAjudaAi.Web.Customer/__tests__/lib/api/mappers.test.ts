import { describe, it, expect } from 'vitest';
import { mapSearchableProviderToProvider, mapApiProviderToProvider } from '@/lib/api/mappers';
import { EProviderType, EProviderStatus, EVerificationStatus, EProviderTier } from '@/types/api/provider';

describe('mapSearchableProviderToProvider', () => {
  it('deve mapear SearchableProviderDto para ProviderDto', () => {
    const input = {
      providerId: 'provider-123',
      name: 'João Silva',
      averageRating: 4.5,
      totalReviews: 10,
      serviceIds: ['service-1', 'service-2'],
      city: 'Rio de Janeiro',
      state: 'RJ',
    };

    const result = mapSearchableProviderToProvider(input);

    expect(result.id).toBe('provider-123');
    expect(result.name).toBe('João Silva');
    expect(result.averageRating).toBe(4.5);
    expect(result.reviewCount).toBe(10);
    expect(result.services).toHaveLength(2);
    expect(result.city).toBe('Rio de Janeiro');
    expect(result.state).toBe('RJ');
  });

  it('deve usar valores padrão para campos opcionais', () => {
    const input = {
      providerId: 'provider-123',
      name: 'João Silva',
    };

    const result = mapSearchableProviderToProvider(input);

    expect(result.averageRating).toBe(0);
    expect(result.reviewCount).toBe(0);
    expect(result.services).toEqual([]);
    expect(result.verificationStatus).toBe(EVerificationStatus.Pending);
    expect(result.tier).toBe(EProviderTier.Standard);
  });
});

describe('mapApiProviderToProvider', () => {
  it('deve mapear ProviderDto completo', () => {
    const input = {
      id: 'provider-456',
      userId: 'user-789',
      name: 'Maria Santos',
      type: 1,
      status: 3,
      verificationStatus: 3,
      tier: 2,
      createdAt: '2024-01-01T00:00:00Z',
      businessProfile: {
        legalName: 'Maria Serviços Ltda',
        fantasyName: 'Maria Solutions',
        description: 'Serviços profissionais',
        contactInfo: {
          email: 'maria@teste.com',
          phoneNumber: '21999999999',
        },
        primaryAddress: {
          street: 'Rua Teste',
          number: '123',
          neighborhood: 'Bairro Teste',
          city: 'São Paulo',
          state: 'SP',
          zipCode: '01234567',
          country: 'Brasil',
        },
      },
      services: [
        { serviceId: 's1', serviceName: 'Elétrica' },
      ],
      averageRating: 4.8,
      reviewCount: 25,
    };

    const result = mapApiProviderToProvider(input);

    expect(result.id).toBe('provider-456');
    expect(result.name).toBe('Maria Solutions');
    expect(result.email).toBe('maria@teste.com');
    expect(result.phone).toBe('21999999999');
    expect(result.services).toHaveLength(1);
    expect(result.services[0].serviceName).toBe('Elétrica');
    expect(result.averageRating).toBe(4.8);
    expect(result.tier).toBe(EProviderTier.Gold);
  });

  it('deve usar fantasyName ou legalName como nome de exibição', () => {
    const inputWithFantasyName = {
      id: 'p1',
      businessProfile: {
        legalName: 'Nome Legal',
        fantasyName: 'Nome Fantasia',
        contactInfo: { email: 'test@test.com' },
        primaryAddress: { city: '', state: '', street: '', neighborhood: '', zipCode: '', country: '' },
      },
    } as any;

    const result = mapApiProviderToProvider(inputWithFantasyName);
    expect(result.name).toBe('Nome Fantasia');

    const inputWithoutFantasyName = {
      id: 'p2',
      businessProfile: {
        legalName: 'Nome Legal Apenas',
        contactInfo: { email: 'test@test.com' },
        primaryAddress: { city: '', state: '', street: '', neighborhood: '', zipCode: '', country: '' },
      },
    } as any;

    const result2 = mapApiProviderToProvider(inputWithoutFantasyName);
    expect(result2.name).toBe('Nome Legal Apenas');
  });

  it('deve usar valores padrão para campos ausentes', () => {
    const input = {
      id: 'p1',
    } as any;

    const result = mapApiProviderToProvider(input);

    expect(result.name).toBe('Prestador');
    expect(result.type).toBe(EProviderType.Individual);
    expect(result.status).toBe(EProviderStatus.PendingBasicInfo);
    expect(result.verificationStatus).toBe(EVerificationStatus.Pending);
    expect(result.tier).toBe(EProviderTier.Standard);
  });
});
