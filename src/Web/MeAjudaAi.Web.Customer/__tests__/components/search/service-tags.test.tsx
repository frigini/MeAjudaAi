import { describe, it, expect } from 'vitest';
import { render, screen } from '@testing-library/react';
import { ServiceTags } from '@/components/search/service-tags';

vi.mock('next/navigation', () => ({
  useRouter: () => ({
    push: vi.fn(),
  }),
  useSearchParams: vi.fn(() => new URLSearchParams()),
}));

vi.mock('@/lib/services/service-catalog', () => ({
  getPopularServices: vi.fn(() => Promise.resolve([
    { id: '1', name: 'Elétrica' },
    { id: '2', name: 'Hidráulica' },
  ])),
}));

describe('ServiceTags', () => {
  it('deve renderizar skeleton de loading', () => {
    vi.mock('@/lib/services/service-catalog', () => ({
      getPopularServices: vi.fn(() => new Promise(() => {})),
    }));
    
    render(<ServiceTags />);
  });
});
