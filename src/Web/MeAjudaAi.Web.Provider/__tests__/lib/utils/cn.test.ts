import { describe, it, expect } from 'vitest';
import { cn } from '@/lib/utils/cn';

describe('cn utility', () => {
  it('should merge class names', () => {
    expect(cn('foo', 'bar')).toBe('foo bar');
  });

  it('should handle conditional classes', () => {
    const condition = true;
    expect(cn('foo', condition && 'bar')).toBe('foo bar');
    expect(cn('foo', condition && null)).toBe('foo');
  });

  it('should handle arrays', () => {
    expect(cn(['foo', 'bar'])).toBe('foo bar');
  });

  it('should handle objects', () => {
    expect(cn({ foo: true, bar: false })).toBe('foo');
  });
});
