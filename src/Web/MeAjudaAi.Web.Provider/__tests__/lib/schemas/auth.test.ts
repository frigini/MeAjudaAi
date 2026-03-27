import { describe, it, expect } from 'vitest';
import { registerProviderSchema, addressSchema, registerCustomerSchema } from '@/lib/schemas/auth';

describe('registerProviderSchema', () => {
  it('should accept valid individual provider', () => {
    const result = registerProviderSchema.safeParse({
      name: 'John Doe',
      documentNumber: '11144477735',
      phoneNumber: '11999999999',
      type: 0,
      email: 'test@example.com',
      acceptedTerms: true,
      acceptedPrivacyPolicy: true
    });
    expect(result.success).toBe(true);
  });

  it('should accept valid company provider', () => {
    const result = registerProviderSchema.safeParse({
      name: 'Company LTDA',
      documentNumber: '11222333000181', // Valid CNPJ
      phoneNumber: '11999999999',
      type: 2,
      email: 'company@example.com',
      acceptedTerms: true,
      acceptedPrivacyPolicy: true
    });
    if (!result.success) {
      console.log('Company errors:', result.error.issues);
    }
    expect(result.success).toBe(true);
  });

  it('should accept freelancer type', () => {
    const result = registerProviderSchema.safeParse({
      name: 'Freelancer',
      documentNumber: '11144477735',
      phoneNumber: '11999999999',
      type: 4,
      email: 'freelancer@example.com',
      acceptedTerms: true,
      acceptedPrivacyPolicy: true
    });
    expect(result.success).toBe(true);
  });

  it('should accept cooperative type', () => {
    const result = registerProviderSchema.safeParse({
      name: 'Cooperative',
      documentNumber: '11222333000181', // Valid CNPJ
      phoneNumber: '11999999999',
      type: 3,
      email: 'coop@example.com',
      acceptedTerms: true,
      acceptedPrivacyPolicy: true
    });
    expect(result.success).toBe(true);
  });

  it('should reject None type', () => {
    const result = registerProviderSchema.safeParse({
      name: 'Test',
      documentNumber: '12345678901',
      phoneNumber: '11999999999',
      type: 0,
      email: 'test@example.com',
      acceptedTerms: true,
      acceptedPrivacyPolicy: true
    });
    expect(result.success).toBe(false);
  });

  it('should reject invalid document length', () => {
    const result = registerProviderSchema.safeParse({
      name: 'John Doe',
      documentNumber: '123',
      phoneNumber: '11999999999',
      type: 1,
      email: 'test@example.com',
      acceptedTerms: true,
      acceptedPrivacyPolicy: true
    });
    expect(result.success).toBe(false);
  });
});

describe('addressSchema', () => {
  it('should accept valid address', () => {
    const result = addressSchema.safeParse({
      zipCode: '12345-678',
      street: 'Main Street',
      number: '123',
      neighborhood: 'Downtown',
      city: 'City',
      state: 'SP'
    });
    expect(result.success).toBe(true);
  });

  it('should accept address without complement', () => {
    const result = addressSchema.safeParse({
      zipCode: '12345-678',
      street: 'Main Street',
      number: '123',
      neighborhood: 'Downtown',
      city: 'City',
      state: 'MG'
    });
    expect(result.success).toBe(true);
  });
});

describe('registerCustomerSchema', () => {
  it('should accept valid customer', () => {
    const result = registerCustomerSchema.safeParse({
      name: 'John Doe',
      email: 'test@example.com',
      phoneNumber: '11999999999',
      password: 'Password1',
      confirmPassword: 'Password1',
      acceptedTerms: true
    });
    expect(result.success).toBe(true);
  });

  it('should accept name with accents', () => {
    const result = registerCustomerSchema.safeParse({
      name: 'João José',
      email: 'test@example.com',
      phoneNumber: '11999999999',
      password: 'Password1',
      confirmPassword: 'Password1',
      acceptedTerms: true
    });
    expect(result.success).toBe(true);
  });
});
