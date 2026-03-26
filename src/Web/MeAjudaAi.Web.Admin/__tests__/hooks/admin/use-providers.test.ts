import { describe, it, expect, vi, beforeEach } from 'vitest';
import { renderHook, waitFor } from 'test-support';
import { 
  useProviders, 
  useProviderById, 
  useCreateProvider, 
  useUpdateProvider, 
  useDeleteProvider, 
  useActivateProvider, 
  useDeactivateProvider 
} from '@/hooks/admin/use-providers';
import * as api from '@/lib/api/generated';

vi.mock('@/lib/api/generated', () => ({
  apiProvidersGet2: vi.fn(),
  apiProvidersGet3: vi.fn(),
  apiProvidersPost: vi.fn(),
  apiProvidersPut: vi.fn(),
  apiProvidersDelete: vi.fn(),
  apiActivatePost: vi.fn(),
  apiDeactivatePost: vi.fn(),
}));

describe('useProviders Hook (Admin)', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('deve buscar todos os provedores', async () => {
    vi.mocked(api.apiProvidersGet2).mockResolvedValue({ data: [{ id: '1', name: 'Provider 1' }] });
    const { result } = renderHook(() => useProviders());
    
    await waitFor(() => expect(result.current.isSuccess).toBe(true));
    expect(result.current.data).toBeDefined();
    expect(result.current.data?.length).toBeGreaterThan(0);
    expect(api.apiProvidersGet2).toHaveBeenCalled();
  });

  it('deve buscar provedor por ID', async () => {
    vi.mocked(api.apiProvidersGet3).mockResolvedValue({ data: { id: 'p-1', name: 'Provider Name' } });
    const { result } = renderHook(() => useProviderById('p-1'));
    
    await waitFor(() => expect(result.current.isSuccess).toBe(true));
    expect(result.current.data).toBeDefined();
    expect(result.current.data?.name).toBe('Provider Name');
    expect(api.apiProvidersGet3).toHaveBeenCalledWith({ path: { id: 'p-1' } });
  });

  it('deve criar um provedor', async () => {
    vi.mocked(api.apiProvidersPost).mockResolvedValue({ data: { id: 'new' } });
    const { result } = renderHook(() => useCreateProvider());
    
    await result.current.mutateAsync({ name: 'New Provider' });
    expect(api.apiProvidersPost).toHaveBeenCalledWith({ body: { name: 'New Provider' } });
  });

  it('deve atualizar um provedor', async () => {
    vi.mocked(api.apiProvidersPut).mockResolvedValue({ data: { id: '1' } });
    const { result } = renderHook(() => useUpdateProvider());
    
    await result.current.mutateAsync({ id: '1', data: { name: 'Updated' } });
    expect(api.apiProvidersPut).toHaveBeenCalledWith({ path: { id: '1' }, body: { name: 'Updated' } });
  });

  it('deve deletar um provedor', async () => {
    vi.mocked(api.apiProvidersDelete).mockResolvedValue({ data: { success: true } });
    const { result } = renderHook(() => useDeleteProvider());
    
    await result.current.mutateAsync('p-1');
    expect(api.apiProvidersDelete).toHaveBeenCalledWith({ path: { id: 'p-1' } });
  });

  it('deve ativar um provedor', async () => {
    vi.mocked(api.apiActivatePost).mockResolvedValue({ data: { success: true } });
    const { result } = renderHook(() => useActivateProvider());
    
    await result.current.mutateAsync('p-1');
    expect(api.apiActivatePost).toHaveBeenCalledWith({ path: { id: 'p-1' } });
  });

  it('deve desativar um provedor', async () => {
    vi.mocked(api.apiDeactivatePost).mockResolvedValue({ data: { success: true } });
    const { result } = renderHook(() => useDeactivateProvider());
    
    await result.current.mutateAsync('p-1');
    expect(api.apiDeactivatePost).toHaveBeenCalledWith({ path: { id: 'p-1' } });
  });
});
