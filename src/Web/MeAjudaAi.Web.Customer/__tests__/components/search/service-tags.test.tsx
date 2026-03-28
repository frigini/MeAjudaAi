import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { ServiceTags } from '@/components/search/service-tags';
import { getPopularServices } from '@/lib/services/service-catalog';

const { mockPush, mockUseSearchParams } = vi.hoisted(() => ({
  mockPush: vi.fn(),
  mockUseSearchParams: vi.fn(() => new URLSearchParams()),
}));

vi.mock('next/navigation', () => ({
  useRouter: () => ({
    push: mockPush,
  }),
  useSearchParams: mockUseSearchParams,
}));

vi.mock('@/lib/services/service-catalog', () => ({
  getPopularServices: vi.fn(),
}));

describe('ServiceTags', () => {
  beforeEach(() => {
    vi.clearAllMocks();
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
    expect(screen.queryByRole('button')).toBeNull();
  });

  it('deve atualizar o filtro "q" ao clicar em uma tag', async () => {
    vi.mocked(getPopularServices).mockResolvedValueOnce([{ id: '1', name: 'Elétrica' }]);
    render(<ServiceTags />);
    
    const tag = await screen.findByText('Elétrica');
    fireEvent.click(tag);
    
    await waitFor(() => {
      expect(mockPush).toHaveBeenCalledWith(expect.stringContaining('q=El%C3%A9trica'));
    });
  });

  it('deve remover o filtro "q" ao clicar em uma tag já ativa', async () => {
    mockUseSearchParams.mockReturnValue(new URLSearchParams('q=Elétrica'));

    vi.mocked(getPopularServices).mockResolvedValueOnce([{ id: '1', name: 'Elétrica' }]);
    render(<ServiceTags />);
    
    const tag = await screen.findByText('Elétrica');
    fireEvent.click(tag);
    
    await waitFor(() => {
      expect(mockPush).toHaveBeenCalledWith(expect.not.stringContaining('q=El%C3%A9trica'));
    });
  });
});
