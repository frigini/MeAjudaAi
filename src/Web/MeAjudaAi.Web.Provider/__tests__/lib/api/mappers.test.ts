import { describe, it, expect } from 'vitest';
import { mapSearchableProviderToProvider, mapApiProviderToProvider } from '../../../lib/api/mappers';

describe('mappers', () => {
  describe('mapSearchableProviderToProvider', () => {
    it('should map searchable provider to provider', () => {
      const input = {
        providerId: '123',
        name: 'Test Provider',
        averageRating: 4.5,
        totalReviews: 10,
        serviceIds: ['1', '2'],
        city: 'Muriaé',
        state: 'MG'
      };

      const result = mapSearchableProviderToProvider(input as any);

      expect(result.id).toBe('123');
      expect(result.name).toBe('Test Provider');
      expect(result.averageRating).toBe(4.5);
      expect(result.reviewCount).toBe(10);
      expect(result.city).toBe('Muriaé');
      expect(result.state).toBe('MG');
    });

    it('should handle null values', () => {
      const input = {
        providerId: null,
        name: null,
        averageRating: null,
        totalReviews: null,
        serviceIds: null,
        city: null,
        state: null
      };

      const result = mapSearchableProviderToProvider(input as any);

      expect(result.id).toBe('');
      expect(result.name).toBe('');
    });
  });

  describe('mapApiProviderToProvider', () => {
    it('should map API provider to provider', () => {
      const input = {
        id: 'provider-1',
        userId: 'user-1',
        name: 'Provider Name',
        businessProfile: {
          fantasyName: 'Fantasy Name',
          legalName: 'Legal Name',
          description: 'Description',
          contactInfo: {
            email: 'test@email.com',
            phoneNumber: '11999999999'
          },
          primaryAddress: {
            street: 'Street',
            number: '123',
            neighborhood: 'Neighborhood',
            city: 'City',
            state: 'ST',
            zipCode: '00000000',
            country: 'Brasil'
          }
        },
        type: 1,
        status: 3,
        verificationStatus: 3,
        tier: 0,
        services: [
          { serviceId: '1', serviceName: 'Service 1', price: 100 }
        ],
        averageRating: 4.5,
        reviewCount: 10,
        documents: [],
        qualifications: []
      };

      const result = mapApiProviderToProvider(input as any);

      expect(result.id).toBe('provider-1');
      expect(result.name).toBe('Fantasy Name');
      expect(result.email).toBe('test@email.com');
    });

    it('should handle missing businessProfile', () => {
      const input = {
        id: 'provider-1',
        name: 'Provider Name'
      };

      const result = mapApiProviderToProvider(input as any);

      expect(result.name).toBe('Provider Name');
      expect(result.email).toBe('');
    });

    it('should use legalName when fantasyName missing', () => {
      const input = {
        id: 'provider-1',
        name: 'Provider Name',
        businessProfile: {
          legalName: 'Legal'
        }
      };

      const result = mapApiProviderToProvider(input as any);

      expect(result.name).toBe('Legal');
    });

    it('should map extended properties and nested lists', () => {
      const input = {
        id: 'ext-1',
        tier: 2, // Gold
        rejectionReason: 'Invalid docs',
        suspensionReason: 'Inactive',
        documents: [
          { id: 'doc-1', documentType: 1, fileName: 'test.pdf', status: 2 }
        ],
        qualifications: [
          { name: 'Cert 1', description: 'Some cert' }
        ],
        services: [
          { serviceId: 'svc-1', serviceName: 'Plumbing' }
        ]
      };

      const result = mapApiProviderToProvider(input as any);

      expect(result.tier).toBe(2);
      expect(result.rejectionReason).toBe('Invalid docs');
      expect(result.suspensionReason).toBe('Inactive');
      expect(result.documents).toHaveLength(1);
      expect(result.documents[0].fileName).toBe('test.pdf');
      expect(result.qualifications).toHaveLength(1);
      expect(result.qualifications[0].name).toBe('Cert 1');
      expect(result.services[0].serviceName).toBe('Plumbing');
    });

    it('should provide default values for missing nested properties', () => {
        const input = { id: 'def-1' };
        const result = mapApiProviderToProvider(input as any);
        
        expect(result.businessProfile.primaryAddress.country).toBe('');
        expect(result.documents).toEqual([]);
        expect(result.qualifications).toEqual([]);
        expect(result.services).toEqual([]);
    });
  });
});
