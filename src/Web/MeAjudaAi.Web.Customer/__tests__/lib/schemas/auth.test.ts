import { describe, it, expect } from 'vitest';
import { registerProviderSchema, registerCustomerSchema, addressSchema } from '@/lib/schemas/auth';
import { EProviderType } from '@/types/api/provider';

describe('registerProviderSchema', () => {
  it('deve validar dados válidos para PF com CPF válido', () => {
    const validPfData = {
      name: 'João Silva',
      documentNumber: '11144477735',
      phoneNumber: '21999999999',
      type: EProviderType.Individual,
      email: 'joao@teste.com',
      acceptedTerms: true,
      acceptedPrivacyPolicy: true,
    };
    
    const result = registerProviderSchema.safeParse(validPfData);
    expect(result.success).toBe(true);
  });

  it('deve rejeitar CPF inválido', () => {
    const validPfData = {
      name: 'João Silva',
      documentNumber: '12345678901',
      phoneNumber: '21999999999',
      type: EProviderType.Individual,
      email: 'joao@teste.com',
      acceptedTerms: true,
      acceptedPrivacyPolicy: true,
    };
    
    const result = registerProviderSchema.safeParse(validPfData);
    expect(result.success).toBe(false);
  });

  it('deve rejeitar nome muito curto', () => {
    const data = {
      name: 'J',
      documentNumber: '12345678901',
      phoneNumber: '21999999999',
      type: EProviderType.Individual,
      email: 'joao@teste.com',
      acceptedTerms: true,
      acceptedPrivacyPolicy: true,
    };
    
    const result = registerProviderSchema.safeParse(data);
    expect(result.success).toBe(false);
  });

  it('deve rejeitar email inválido', () => {
    const data = {
      name: 'João Silva',
      documentNumber: '12345678901',
      phoneNumber: '21999999999',
      type: EProviderType.Individual,
      email: 'email-invalido',
      acceptedTerms: true,
      acceptedPrivacyPolicy: true,
    };
    
    const result = registerProviderSchema.safeParse(data);
    expect(result.success).toBe(false);
  });

  it('deve rejeitar termos não aceitos', () => {
    const data = {
      name: 'João Silva',
      documentNumber: '12345678901',
      phoneNumber: '21999999999',
      type: EProviderType.Individual,
      email: 'joao@teste.com',
      acceptedTerms: false,
      acceptedPrivacyPolicy: true,
    };
    
    const result = registerProviderSchema.safeParse(data);
    expect(result.success).toBe(false);
  });

  it('deve rejeitar telefone com poucos dígitos', () => {
    const data = {
      name: 'João Silva',
      documentNumber: '12345678901',
      phoneNumber: '21999',
      type: EProviderType.Individual,
      email: 'joao@teste.com',
      acceptedTerms: true,
      acceptedPrivacyPolicy: true,
    };
    
    const result = registerProviderSchema.safeParse(data);
    expect(result.success).toBe(false);
  });

  it('deve validar dados válidos para PJ com CNPJ válido', () => {
    const validPjData = {
      name: 'Empresa Teste LTDA',
      documentNumber: '11222333000181',
      phoneNumber: '21999999999',
      type: EProviderType.Company,
      email: 'empresa@teste.com',
      acceptedTerms: true,
      acceptedPrivacyPolicy: true,
    };
    
    const result = registerProviderSchema.safeParse(validPjData);
    expect(result.success).toBe(true);
  });

  it('deve rejeitar CNPJ inválido', () => {
    const data = {
      name: 'Empresa Teste',
      documentNumber: '12345678901234',
      phoneNumber: '21999999999',
      type: EProviderType.Company,
      email: 'empresa@teste.com',
      acceptedTerms: true,
      acceptedPrivacyPolicy: true,
    };
    
    const result = registerProviderSchema.safeParse(data);
    expect(result.success).toBe(false);
  });

  it('deve validar freelancer com CPF válido', () => {
    const validFreelancerData = {
      name: 'João Freelancer',
      documentNumber: '11144477735',
      phoneNumber: '21999999999',
      type: EProviderType.Freelancer,
      email: 'freelancer@teste.com',
      acceptedTerms: true,
      acceptedPrivacyPolicy: true,
    };
    
    const result = registerProviderSchema.safeParse(validFreelancerData);
    expect(result.success).toBe(true);
  });

  it('deve rejeitar tipo None', () => {
    const data = {
      name: 'João Silva',
      documentNumber: '11144477735',
      phoneNumber: '21999999999',
      type: EProviderType.None,
      email: 'joao@teste.com',
      acceptedTerms: true,
      acceptedPrivacyPolicy: true,
    };
    
    const result = registerProviderSchema.safeParse(data);
    expect(result.success).toBe(false);
  });

  it('deve rejeitar privacy policy não aceita', () => {
    const data = {
      name: 'João Silva',
      documentNumber: '11144477735',
      phoneNumber: '21999999999',
      type: EProviderType.Individual,
      email: 'joao@teste.com',
      acceptedTerms: true,
      acceptedPrivacyPolicy: false,
    };
    
    const result = registerProviderSchema.safeParse(data);
    expect(result.success).toBe(false);
  });
});

describe('registerCustomerSchema', () => {
  it('deve validar dados válidos', () => {
    const validData = {
      name: 'Maria Santos',
      email: 'maria@teste.com',
      phoneNumber: '21999999999',
      password: 'Senha123',
      confirmPassword: 'Senha123',
      acceptedTerms: true,
    };
    
    const result = registerCustomerSchema.safeParse(validData);
    expect(result.success).toBe(true);
  });

  it('deve rejeitar senhas diferentes', () => {
    const data = {
      name: 'Maria Santos',
      email: 'maria@teste.com',
      phoneNumber: '21999999999',
      password: 'Senha123',
      confirmPassword: 'Senha456',
      acceptedTerms: true,
    };
    
    const result = registerCustomerSchema.safeParse(data);
    expect(result.success).toBe(false);
  });

  it('deve rejeitar senha sem maiúscula', () => {
    const data = {
      name: 'Maria Santos',
      email: 'maria@teste.com',
      phoneNumber: '21999999999',
      password: 'senha123',
      confirmPassword: 'senha123',
      acceptedTerms: true,
    };
    
    const result = registerCustomerSchema.safeParse(data);
    expect(result.success).toBe(false);
  });
});

describe('addressSchema', () => {
  it('deve validar endereço completo', () => {
    const validAddress = {
      zipCode: '20550160',
      street: 'Rua Teste',
      number: '123',
      complement: 'Apto 1',
      neighborhood: 'Bairro Teste',
      city: 'Rio de Janeiro',
      state: 'RJ',
    };
    
    const result = addressSchema.safeParse(validAddress);
    expect(result.success).toBe(true);
  });

  it('deve aceitar CEP sem hífen', () => {
    const address = {
      zipCode: '20550160',
      street: 'Rua Teste',
      number: '123',
      neighborhood: 'Bairro Teste',
      city: 'Rio de Janeiro',
      state: 'RJ',
    };
    
    const result = addressSchema.safeParse(address);
    expect(result.success).toBe(true);
  });

  it('deve rejeitar estado com mais de 2 letras', () => {
    const address = {
      zipCode: '20550160',
      street: 'Rua Teste',
      number: '123',
      neighborhood: 'Bairro Teste',
      city: 'Rio de Janeiro',
      state: 'RJO',
    };
    
    const result = addressSchema.safeParse(address);
    expect(result.success).toBe(false);
  });
});
