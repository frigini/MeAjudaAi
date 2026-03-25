import { describe, it, expect, vi } from 'vitest';
import { renderHook, waitFor } from 'test-support';

vi.mock('@/lib/api/generated', () => ({
  apiUsersGet: vi.fn().mockResolvedValue({ data: [
    { id: 'user-1', name: 'Admin Teste', email: 'admin@teste.com' },
  ]}),
  apiUsersGet2: vi.fn().mockResolvedValue({ data: { id: 'user-1', name: 'Admin Teste' } }),
  apiUsersPost: vi.fn().mockResolvedValue({ data: {} }),
  apiUsersDelete: vi.fn().mockResolvedValue({}),
}));

import { useUsers, useUserById, userKeys } from '@/hooks/admin/use-users';

describe('useUsers', () => {
  it('deve retornar lista de usuários', async () => {
    const { result } = renderHook(() => useUsers());

    await waitFor(() => expect(result.current.isSuccess).toBe(true));

    expect(result.current.data).toBeDefined();
  });
});

describe('useUserById', () => {
  it('deve estar desabilitado quando id está vazio', () => {
    const { result } = renderHook(() => useUserById(''));
    expect(result.current.fetchStatus).toBe('idle');
  });

  it('deve buscar usuário por id', async () => {
    const { result } = renderHook(() => useUserById('user-1'));

    await waitFor(() => expect(result.current.isSuccess).toBe(true));

    expect(result.current.data).toBeDefined();
  });
});

describe('userKeys', () => {
  it('deve gerar query keys corretamente', () => {
    expect(userKeys.all).toEqual(['users']);
    expect(userKeys.lists()).toEqual(['users', 'list']);
    expect(userKeys.detail('abc')).toEqual(['users', 'detail', 'abc']);
  });
});
