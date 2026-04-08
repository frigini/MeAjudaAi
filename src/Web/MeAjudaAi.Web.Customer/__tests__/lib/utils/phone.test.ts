import { describe, it, expect } from 'vitest';
import { getWhatsappLink } from '@/lib/utils/phone';

describe('getWhatsappLink', () => {
  it('deve gerar link válido para telefone com DDD e 9 dígitos', () => {
    expect(getWhatsappLink('21999999999')).toBe('https://wa.me/5521999999999');
  });

  it('deve gerar link válido para telefone com DDD e 8 dígitos', () => {
    expect(getWhatsappLink('2133334444')).toBe('https://wa.me/552133334444');
  });

  it('deve gerar link válido para telefone com DDI', () => {
    expect(getWhatsappLink('5521999999999')).toBe('https://wa.me/5521999999999');
  });

  it('deve gerar link válido para telefone formatado', () => {
    expect(getWhatsappLink('(21) 99999-9999')).toBe('https://wa.me/5521999999999');
  });

  it('deve retornar null para telefone com poucos dígitos', () => {
    expect(getWhatsappLink('219999')).toBeNull();
  });

  it('deve retornar null para string vazia', () => {
    expect(getWhatsappLink('')).toBeNull();
  });
});
