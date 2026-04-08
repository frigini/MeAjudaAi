import { describe, it, expect } from 'vitest';
import { getWhatsappLink } from '@/lib/utils/phone';

describe('getWhatsappLink', () => {
  it('should return WhatsApp link for valid phone', () => {
    expect(getWhatsappLink('11999999999')).toBe('https://wa.me/5511999999999');
  });

  it('should handle phone with DDI', () => {
    expect(getWhatsappLink('5511999999999')).toBe('https://wa.me/5511999999999');
  });

  it('should return null for phone too short', () => {
    expect(getWhatsappLink('123456789')).toBeNull();
  });

  it('should handle formatted phone', () => {
    expect(getWhatsappLink('(11) 99999-9999')).toBe('https://wa.me/5511999999999');
  });

  it('should handle phone with 10 digits', () => {
    expect(getWhatsappLink('1198887777')).toBe('https://wa.me/551198887777');
  });

  it('should handle empty string', () => {
    expect(getWhatsappLink('')).toBeNull();
  });

  it('should handle phone with special chars', () => {
    expect(getWhatsappLink('+55 11 99999-9999')).toBe('https://wa.me/5511999999999');
  });
});
