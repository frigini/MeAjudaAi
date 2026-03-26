import { describe, it, expect, vi, beforeEach } from 'vitest';
import { renderHook, waitFor } from 'test-support';
import { useUsers, useUserById, useDeleteUser } from '@/hooks/admin/use-users';
import * as api from '@/lib/api/generated';

vi.mock('@/lib/api/generated', () => ({
  apiUsersGet: vi.fn(),
  apiUsersGet2: vi.fn(),
  apiUsersDelete: vi.fn(),
}));

describe('useUsers Hook (Admin)', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('deve buscar todos os usuários', async () => {
    vi.mocked(api.apiUsersGet).mockResolvedValue({ data: [{ id: '1', name: 'User 1' }] } as any);
    const { result } = renderHook(() => useUsers());
    
    await waitFor(() => expect(result.current.isSuccess).toBe(true));
    expect(result.current.data).toHaveLength(1);
    expect(api.apiUsersGet).toHaveBeenCalled();
  });

  it('deve buscar usuário por ID', async () => {
    vi.mocked(api.apiUsersGet2).mockResolvedValue({ data: { id: 'u-1', name: 'User Name' } } as any);
    const { result } = renderHook(() => useUserById('u-1'));
    
    await waitFor(() => expect(result.current.isSuccess).toBe(true));
    expect(result.current.data.name).toBe('User Name');
    expect(api.apiUsersGet2).toHaveBeenCalledWith({ path: { id: 'u-1' } });
  });

  it('deve deletar um usuário', async () => {
    vi.mocked(api.apiUsersDelete).mockResolvedValue({ success: true } as any);
    const { result } = renderHook(() => useDeleteUser());
    
    await result.current.mutateAsync('u-1');
    expect(api.apiUsersDelete).toHaveBeenCalledWith({ path: { id: 'u-1' } });
  });
});
