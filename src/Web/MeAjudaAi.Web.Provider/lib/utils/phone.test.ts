import { describe, it, expect } from 'vitest';
import { getWhatsappLink } from './phone';

describe('phone utils (Provider)', () => {
  it('deve gerar link do whatsapp para telefone sem formatação', () => {
    expect(getWhatsappLink('11987654321')).toBe('https://wa.me/5511987654321');
  });

  it('deve gerar link do whatsapp para telefone com formatação', () => {
    expect(getWhatsappLink('(11) 98765-4321')).toBe('https://wa.me/5511987654321');
  });

  it('deve remover DDI 55 se já existir e continuar com DDD+número', () => {
    expect(getWhatsappLink('5511987654321')).toBe('https://wa.me/5511987654321');
  });

  it('deve retornar null se o telefone for muito curto', () => {
    expect(getWhatsappLink('12345')).toBe(null);
  });

  it('deve lidar com strings vazias', () => {
    expect(getWhatsappLink('')).toBe(null);
  });
});
