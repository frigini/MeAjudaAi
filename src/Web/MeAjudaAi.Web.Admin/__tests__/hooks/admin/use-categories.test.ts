import { describe, it, expect, vi, beforeEach } from 'vitest';
import { renderHook, waitFor } from 'test-support';
import { 
  useCategories, 
  useCategoryById, 
  useCreateCategory, 
  useUpdateCategory, 
  useDeleteCategory, 
  useActivateCategory, 
  useDeactivateCategory 
} from '@/hooks/admin/use-categories';
import * as api from '@/lib/api/generated';

vi.mock('@/lib/api/generated', () => ({
  apiCategoriesGet: vi.fn(),
  apiCategoriesGet2: vi.fn(),
  apiCategoriesPost: vi.fn(),
  apiCategoriesPut: vi.fn(),
  apiCategoriesDelete: vi.fn(),
  apiActivatePost2: vi.fn(),
  apiDeactivatePost2: vi.fn(),
}));

describe('useCategories Hook (Admin)', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('deve buscar todas as categorias', async () => {
    vi.mocked(api.apiCategoriesGet).mockResolvedValue({ data: [{ id: '1', name: 'Cat 1' }] });
    const { result } = renderHook(() => useCategories());
    
    await waitFor(() => expect(result.current.isSuccess).toBe(true));
    expect(result.current.data).toHaveLength(1);
    expect(api.apiCategoriesGet).toHaveBeenCalled();
  });

  it('deve buscar categoria por ID', async () => {
    vi.mocked(api.apiCategoriesGet2).mockResolvedValue({ data: { id: 'c-1', name: 'Cat Name' } });
    const { result } = renderHook(() => useCategoryById('c-1'));
    
    await waitFor(() => expect(result.current.isSuccess).toBe(true));
    expect(result.current.data.name).toBe('Cat Name');
    expect(api.apiCategoriesGet2).toHaveBeenCalledWith({ path: { id: 'c-1' } });
  });

  it('deve criar uma categoria', async () => {
    vi.mocked(api.apiCategoriesPost).mockResolvedValue({ data: { id: 'new' } });
    const { result } = renderHook(() => useCreateCategory());
    
    await result.current.mutateAsync({ name: 'New Cat', description: 'Desc', displayOrder: 1 });
    expect(api.apiCategoriesPost).toHaveBeenCalledWith({ 
      body: { name: 'New Cat', description: 'Desc', displayOrder: 1 } 
    });
  });

  it('deve atualizar uma categoria', async () => {
    vi.mocked(api.apiCategoriesPut).mockResolvedValue({ data: { id: '1' } });
    const { result } = renderHook(() => useUpdateCategory());
    
    await result.current.mutateAsync({ id: '1', name: 'Updated', description: 'New Desc' });
    expect(api.apiCategoriesPut).toHaveBeenCalledWith({ 
      path: { id: '1' }, 
      body: { name: 'Updated', description: 'New Desc' } 
    });
  });

  it('deve deletar uma categoria', async () => {
    vi.mocked(api.apiCategoriesDelete).mockResolvedValue({ data: { success: true } });
    const { result } = renderHook(() => useDeleteCategory());
    
    await result.current.mutateAsync('c-1');
    expect(api.apiCategoriesDelete).toHaveBeenCalledWith({ path: { id: 'c-1' } });
  });

  it('deve ativar uma categoria', async () => {
    vi.mocked(api.apiActivatePost2).mockResolvedValue({ data: { success: true } });
    const { result } = renderHook(() => useActivateCategory());
    
    await result.current.mutateAsync('c-1');
    expect(api.apiActivatePost2).toHaveBeenCalledWith({ path: { id: 'c-1' } });
  });

  it('deve desativar uma categoria', async () => {
    vi.mocked(api.apiDeactivatePost2).mockResolvedValue({ data: { success: true } });
    const { result } = renderHook(() => useDeactivateCategory());
    
    await result.current.mutateAsync('c-1');
    expect(api.apiDeactivatePost2).toHaveBeenCalledWith({ path: { id: 'c-1' } });
  });
});
