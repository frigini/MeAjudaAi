import { describe, it, expect, vi, beforeEach } from 'vitest';
import { renderHook, waitFor } from 'test-support';
import { useServices, useServiceById, useCreateService, useUpdateService, useDeleteService } from '@/hooks/admin/use-services';
import * as api from '@/lib/api/generated';

vi.mock('@/lib/api/generated', () => ({
  apiServicesGet: vi.fn(),
  apiServicesGet2: vi.fn(),
  apiServicesPost: vi.fn(),
  apiServicesPut: vi.fn(),
  apiServicesDelete: vi.fn(),
  apiCategoryGet: vi.fn(),
}));

describe('useServices Hook (Admin)', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('deve buscar todos os serviços quando categoryId não for fornecido', async () => {
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    vi.mocked(api.apiServicesGet).mockResolvedValue({ data: [{ id: '1', name: 'Service 1' }] } as any);
    const { result } = renderHook(() => useServices());
    
    await waitFor(() => expect(result.current.isSuccess).toBe(true));
    expect(result.current.data).toHaveLength(1);
    expect(api.apiServicesGet).toHaveBeenCalled();
  });

  it('deve buscar serviços por categoria quando categoryId for fornecido', async () => {
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    vi.mocked(api.apiCategoryGet).mockResolvedValue({ data: [{ id: '1', name: 'Cat Service' }] } as any);
    const { result } = renderHook(() => useServices('cat-123'));
    
    await waitFor(() => expect(result.current.isSuccess).toBe(true));
    expect(result.current.data).toHaveLength(1);
    expect(api.apiCategoryGet).toHaveBeenCalledWith({ path: { categoryId: 'cat-123' } });
  });

  it('deve buscar serviço por ID', async () => {
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    vi.mocked(api.apiServicesGet2).mockResolvedValue({ data: { id: 'svc-1', name: 'Service Name' } } as any);
    const { result } = renderHook(() => useServiceById('svc-1'));
    
    await waitFor(() => expect(result.current.isSuccess).toBe(true));
    expect(result.current.data?.name).toBe('Service Name');
    expect(api.apiServicesGet2).toHaveBeenCalledWith({ path: { id: 'svc-1' } });
  });

  it('deve criar um serviço', async () => {
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    vi.mocked(api.apiServicesPost).mockResolvedValue({ data: { id: 'new' } } as any);
    const { result } = renderHook(() => useCreateService());
    
    await result.current.mutateAsync({ name: 'New Service' });
    expect(api.apiServicesPost).toHaveBeenCalledWith({ body: { name: 'New Service' } });
  });

  it('deve atualizar um serviço', async () => {
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    vi.mocked(api.apiServicesPut).mockResolvedValue({ data: { id: '1' } } as any);
    const { result } = renderHook(() => useUpdateService());
    
    await result.current.mutateAsync({ id: '1', body: { name: 'Updated' } });
    expect(api.apiServicesPut).toHaveBeenCalledWith({ path: { id: '1' }, body: { name: 'Updated' } });
  });

  it('deve deletar um serviço', async () => {
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    vi.mocked(api.apiServicesDelete).mockResolvedValue({ data: { success: true } } as any);
    const { result } = renderHook(() => useDeleteService());
    
    await result.current.mutateAsync('svc-1');
    expect(api.apiServicesDelete).toHaveBeenCalledWith({ path: { id: 'svc-1' } });
  });

  it('deve retornar dados brutos se a propriedade .data estiver ausente no useServices', async () => {
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    vi.mocked(api.apiServicesGet).mockResolvedValue([{ id: 'raw-1' }] as any);
    const { result } = renderHook(() => useServices());
    await waitFor(() => expect(result.current.isSuccess).toBe(true));
    expect(result.current.data).toEqual([{ id: 'raw-1' }]);
  });

  it('deve usar o formato alternativo no useCreateService', async () => {
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    vi.mocked(api.apiServicesPost).mockResolvedValue({ data: { id: 'new' } } as any);
    const { result } = renderHook(() => useCreateService());
    await result.current.mutateAsync({ body: { name: 'Wrapped' } });
    expect(api.apiServicesPost).toHaveBeenCalledWith({ body: { name: 'Wrapped' } });
  });

  it('deve usar o formato alternativo no useUpdateService', async () => {
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    vi.mocked(api.apiServicesPut).mockResolvedValue({ data: { id: '1' } } as any);
    const { result } = renderHook(() => useUpdateService());
    await result.current.mutateAsync({ path: { id: '1' }, body: { name: 'Wrapped' } });
    expect(api.apiServicesPut).toHaveBeenCalledWith({ path: { id: '1' }, body: { name: 'Wrapped' } });
  });

  it('deve suportar formato alternativo (objeto com path) no useDeleteService', async () => {
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    vi.mocked(api.apiServicesDelete).mockResolvedValue({ data: { success: true } } as any);
    const { result } = renderHook(() => useDeleteService());
    
    await result.current.mutateAsync({ path: { id: 'svc-2' } });
    expect(api.apiServicesDelete).toHaveBeenCalledWith({ path: { id: 'svc-2' } });
  });
});
