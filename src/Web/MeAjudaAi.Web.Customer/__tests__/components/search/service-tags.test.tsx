import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen } from '@testing-library/react';
import { ServiceTags } from '@/components/search/service-tags';
import { getPopularServices } from '@/lib/services/service-catalog';

vi.mock('next/navigation', () => ({
  useRouter: () => ({
    push: vi.fn(),
  }),
  useSearchParams: vi.fn(() => new URLSearchParams()),
}));

vi.mock('@/lib/services/service-catalog', () => ({
  getPopularServices: vi.fn(),
}));

describe('ServiceTags', () => {
  beforeEach(() => {
    vi.resetAllMocks();
  });

  it('deve renderizar serviços populares', async () => {
    vi.mocked(getPopularServices).mockResolvedValueOnce([
      { id: '1', name: 'Elétrica' },
      { id: '2', name: 'Hidráulica' },
    ]);
    
    render(<ServiceTags />);
    
    await screen.findByText('Elétrica');
    await screen.findByText('Hidráulica');
  });

  it('deve renderizar skeleton de loading', async () => {
    vi.mocked(getPopularServices).mockImplementationOnce(() => new Promise(() => {}));
    
    render(<ServiceTags />);
  });
});
