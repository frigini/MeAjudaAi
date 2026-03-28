import { describe, it, expect } from 'vitest';
import { mapSearchableProviderToProvider, mapApiProviderToProvider } from './mappers';
import { EProviderType, EVerificationStatus, EProviderTier, EProviderStatus } from '@/types/api/provider';

describe('mappers (Provider)', () => {
  describe('mapSearchableProviderToProvider', () => {
    it('deve mapear DTO de busca completo', () => {
      const dto = {
        providerId: 'p-1',
        name: 'Provider Name',
        averageRating: 4.5,
        totalReviews: 10,
        serviceIds: ['s1', 's2'],
        city: 'São Paulo',
        state: 'SP'
      };

      const result = mapSearchableProviderToProvider(dto);

      expect(result.id).toBe('p-1');
      expect(result.name).toBe('Provider Name');
      expect(result.averageRating).toBe(4.5);
      expect(result.reviewCount).toBe(10);
      expect(result.services).toHaveLength(2);
      expect(result.city).toBe('São Paulo');
      expect(result.state).toBe('SP');
    });

    it('deve lidar com campos nulos no DTO de busca', () => {
      const dto = {
        providerId: undefined,
        name: null,
        serviceIds: undefined,
      } as any;

      const result = mapSearchableProviderToProvider(dto);

      expect(result.id).toBe('');
      expect(result.name).toBe('');
      expect(result.services).toEqual([]);
      expect(result.averageRating).toBe(0);
    });
  });

  describe('mapApiProviderToProvider', () => {
    it('deve mapear DTO de API completo com perfil de negócio', () => {
      const rawDto = {
        id: 'p-2',
        userId: 'u-1',
        name: 'John Doe',
        profilePictureUrl: 'http://avatar.com/p2.jpg',
        type: 1, // Individual
        status: 2, // Active
        verificationStatus: 1, // Verified
        tier: 2, // Premium
        businessProfile: {
          fantasyName: 'John Services',
          legalName: 'John Doe Ltd',
          description: 'Best services',
          showAddressToClient: true,
          contactInfo: {
            email: 'john@example.com',
            phoneNumber: '11988887777',
            website: 'http://john.com'
          },
          primaryAddress: {
            street: 'Rua A',
            number: '123',
            neighborhood: 'Centro',
            city: 'São Paulo',
            state: 'SP',
            zipCode: '01000-000',
            country: 'Brasil'
          }
        },
        services: [
          { serviceId: 's1', serviceName: 'Eletricista', price: 100 }
        ],
        documents: [
          { id: 'd1', documentType: 1, fileName: 'rg.pdf', status: 1 }
        ],
        qualifications: [
          { name: 'Cert A' }
        ],
        createdAt: '2024-01-01',
        updatedAt: '2024-01-02'
      };

      const result = mapApiProviderToProvider(rawDto as any);

      expect(result.id).toBe('p-2');
      expect(result.name).toBe('John Services'); // Fantasy name priority
      expect(result.email).toBe('john@example.com');
      expect(result.phone).toBe('11988887777');
      expect(result.avatarUrl).toBe('http://avatar.com/p2.jpg');
      expect(result.services[0].serviceName).toBe('Eletricista');
      expect(result.businessProfile.primaryAddress.zipCode).toBe('01000-000');
      expect(result.documents).toHaveLength(1);
      expect(result.qualifications).toHaveLength(1);
    });

    it('deve usar legalName se fantasyName for nulo', () => {
      const dto = {
        businessProfile: {
          legalName: 'Legal Name',
          fantasyName: null
        }
      } as any;
      const result = mapApiProviderToProvider(dto);
      expect(result.name).toBe('Legal Name');
    });

    it('deve usar name do DTO se perfil de negócio for nulo', () => {
      const dto = {
        name: 'DTO Name',
        businessProfile: null
      } as any;
      const result = mapApiProviderToProvider(dto);
      expect(result.name).toBe('DTO Name');
    });

    it('deve lidar com campos nulos e arrays vazios', () => {
      const dto = {
        id: 'p-empty',
        services: null,
        documents: null,
        qualifications: null,
        businessProfile: {
          contactInfo: null,
          primaryAddress: null
        }
      } as any;

      const result = mapApiProviderToProvider(dto);

      expect(result.services).toEqual([]);
      expect(result.documents).toEqual([]);
      expect(result.qualifications).toEqual([]);
      expect(result.email).toBe('');
      expect(result.city).toBe('');
    });

    it('deve suportar fallback de Enums para valores default', () => {
      const dto = {
        type: null,
        status: null,
        verificationStatus: null,
        tier: null
      } as any;

      const result = mapApiProviderToProvider(dto);

      expect(result.type).toBe(EProviderType.Individual);
      expect(result.status).toBe(EProviderStatus.PendingBasicInfo);
      expect(result.verificationStatus).toBe(EVerificationStatus.Pending);
      expect(result.tier).toBe(EProviderTier.Standard);
    });
  });
});
