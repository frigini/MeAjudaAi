import { describe, it, expect, vi } from 'vitest';
import { renderHook, waitFor } from 'test-support';

vi.mock('@/lib/api/generated', () => ({
  apiCategoriesGet: vi.fn().mockResolvedValue({ data: [
    { id: 'cat-1', name: 'Elétrica', isActive: true },
    { id: 'cat-2', name: 'Hidráulica', isActive: false },
  ]}),
  apiCategoriesGet2: vi.fn().mockResolvedValue({ data: { id: 'cat-1', name: 'Elétrica' } }),
  apiCategoriesPost: vi.fn().mockResolvedValue({ data: {} }),
  apiCategoriesPut: vi.fn().mockResolvedValue({ data: {} }),
  apiCategoriesDelete: vi.fn().mockResolvedValue({}),
  apiActivatePost2: vi.fn().mockResolvedValue({}),
  apiDeactivatePost2: vi.fn().mockResolvedValue({}),
}));

import { useCategories, useCategoryById, categoryKeys } from '@/hooks/admin/use-categories';

describe('useCategories', () => {
  it('deve retornar lista de categorias', async () => {
    const { result } = renderHook(() => useCategories());

    await waitFor(() => expect(result.current.isSuccess).toBe(true));

    expect(result.current.data).toHaveLength(2);
    expect(result.current.data?.[0].name).toBe('Elétrica');
  });
});

describe('useCategoryById', () => {
  it('deve estar desabilitado quando id está vazio', () => {
    const { result } = renderHook(() => useCategoryById(''));
    expect(result.current.fetchStatus).toBe('idle');
  });

  it('deve buscar categoria por id', async () => {
    const { result } = renderHook(() => useCategoryById('cat-1'));

    await waitFor(() => expect(result.current.isSuccess).toBe(true));

    expect(result.current.data?.name).toBe('Elétrica');
  });
});

describe('categoryKeys', () => {
  it('deve gerar query keys corretamente', () => {
    expect(categoryKeys.all).toEqual(['categories']);
    expect(categoryKeys.lists()).toEqual(['categories', 'list']);
    expect(categoryKeys.detail('abc')).toEqual(['categories', 'detail', 'abc']);
  });
});
