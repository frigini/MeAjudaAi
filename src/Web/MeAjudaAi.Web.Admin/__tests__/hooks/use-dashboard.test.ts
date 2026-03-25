import { describe, it, expect, vi } from 'vitest';
import { renderHook, waitFor } from 'test-support';

vi.mock('@/lib/api/generated', () => ({
  apiProvidersGet2: vi.fn().mockResolvedValue({
    data: {
      items: [
        { id: '1', verificationStatus: 1, type: 1 }, // Pending, Individual
        { id: '2', verificationStatus: 3, type: 2 }, // Verified, Company
      ],
      totalPages: 1,
    },
  }),
}));

vi.mock('@/lib/types', () => ({
  EVerificationStatus: { 
    None: 0, 
    Pending: 1, 
    InProgress: 2, 
    Verified: 3, 
    Rejected: 4, 
    Suspended: 5 
  },
  EProviderType: { 
    None: 0, 
    Individual: 1, 
    Company: 2, 
    Cooperative: 3, 
    Freelancer: 4 
  },
}));

import { useDashboardStats } from '@/hooks/admin/use-dashboard';

describe('useDashboardStats', () => {
  it('deve calcular estatísticas corretamente', async () => {
    const { result } = renderHook(() => useDashboardStats());

    await waitFor(() => expect(result.current.isSuccess).toBe(true));

    const stats = result.current.data;
    expect(stats?.total).toBe(2);
    expect(stats?.pending).toBe(1);
    expect(stats?.approved).toBe(1);
    expect(stats?.individual).toBe(1);
    expect(stats?.company).toBe(1);
  });

  it('deve iniciar com isLoading true', () => {
    const { result } = renderHook(() => useDashboardStats());
    expect(result.current.isLoading).toBe(true);
  });
});
