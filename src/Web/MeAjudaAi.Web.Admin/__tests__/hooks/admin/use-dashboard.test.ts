import { describe, it, expect, vi, beforeEach } from 'vitest';
import { renderHook, waitFor } from 'test-support';
import { useDashboardStats } from '@/hooks/admin/use-dashboard';
import * as api from '@/lib/api/generated';
import { EVerificationStatus, EProviderType } from '@/lib/types';

vi.mock('@/lib/api/generated', () => ({
  apiProvidersGet2: vi.fn(),
}));

describe('useDashboardStats Hook (Admin)', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('deve calcular estatísticas do dashboard corretamente', async () => {
    const mockProviders = [
      { id: '1', verificationStatus: EVerificationStatus.Verified, type: EProviderType.Individual },
      { id: '2', verificationStatus: EVerificationStatus.Pending, type: EProviderType.Company },
      { id: '3', verificationStatus: EVerificationStatus.Rejected, type: EProviderType.Freelancer },
      { id: '4', verificationStatus: EVerificationStatus.InProgress, type: EProviderType.Cooperative },
      { id: '5', verificationStatus: EVerificationStatus.Suspended, type: EProviderType.Company },
    ];

    vi.mocked(api.apiProvidersGet2).mockResolvedValue({ 
      data: { 
        value: {
          items: mockProviders,
          totalPages: 1 
        }
      } 
    } as any);

    const { result } = renderHook(() => useDashboardStats());
    
    await waitFor(() => expect(result.current.isSuccess).toBe(true));
    
    expect(result.current.data?.total).toBe(5);
    expect(result.current.data?.approved).toBe(1);
    expect(result.current.data?.pending).toBe(1);
    expect(result.current.data?.rejected).toBe(1);
    expect(result.current.data?.underReview).toBe(1);
    expect(result.current.data?.suspended).toBe(1);
    
    expect(result.current.data?.individual).toBe(1);
    expect(result.current.data?.company).toBe(2);
    expect(result.current.data?.freelancer).toBe(1);
    expect(result.current.data?.cooperative).toBe(1);
    
    expect(api.apiProvidersGet2).toHaveBeenCalled();
  });

  it('deve lidar com múltiplas páginas de provedores', async () => {
    vi.mocked(api.apiProvidersGet2)
      .mockResolvedValueOnce({ data: { items: [{ id: '1', verificationStatus: EVerificationStatus.Verified, type: EProviderType.Individual }], totalPages: 2 } } as any)
      .mockResolvedValueOnce({ data: { items: [{ id: '2', verificationStatus: EVerificationStatus.Verified, type: EProviderType.Company }], totalPages: 2 } } as any);

    const { result } = renderHook(() => useDashboardStats());
    
    await waitFor(() => expect(result.current.isSuccess).toBe(true));
    expect(result.current.data?.total).toBe(2);
    expect(api.apiProvidersGet2).toHaveBeenCalledTimes(2);
  });

  it('deve lidar com falha na resposta da API', async () => {
    vi.mocked(api.apiProvidersGet2).mockResolvedValue({ data: null } as any);

    const { result } = renderHook(() => useDashboardStats());
    
    await waitFor(() => expect(result.current.isSuccess).toBe(true));
    expect(result.current.data?.total).toBe(0);
  });
});
