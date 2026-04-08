import { describe, it, expect } from 'vitest';
import { unwrapResponse } from '@/lib/api/response-utils';

describe('unwrapResponse', () => {
  it('should return undefined for null', () => {
    expect(unwrapResponse(null)).toBeUndefined();
  });

  it('should return undefined for undefined', () => {
    expect(unwrapResponse(undefined)).toBeUndefined();
  });

  it('should unwrap value property', () => {
    const response = { value: { data: 'test' } };
    expect(unwrapResponse(response)).toEqual({ data: 'test' });
  });

  it('should unwrap result property', () => {
    const response = { result: { data: 'test' } };
    expect(unwrapResponse(response)).toEqual({ data: 'test' });
  });

  it('should return response as-is when not wrapped', () => {
    const response = { data: 'test' };
    expect(unwrapResponse(response)).toEqual({ data: 'test' });
  });

  it('should handle primitive values', () => {
    expect(unwrapResponse('string')).toBe('string');
    expect(unwrapResponse(123)).toBe(123);
  });
});
