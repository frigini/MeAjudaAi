import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest';
import { renderHook, act, waitFor } from '@testing-library/react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { useDocumentUpload } from '@/hooks/use-document-upload';
import React from 'react';
import { EDocumentType } from '@/types/api/provider';

vi.mock('@/lib/api/fetch-client', () => ({
  authenticatedFetch: vi.fn(),
}));

vi.mock('next-auth/react', () => ({
  useSession: vi.fn(() => ({
    data: { accessToken: 'mock-token', user: { id: 'user-1' } },
    status: 'authenticated',
  })),
}));

vi.mock('sonner', () => ({
  toast: {
    success: vi.fn(),
    error: vi.fn(),
  },
}));

const createWrapper = () => {
  const queryClient = new QueryClient({
    defaultOptions: {
      queries: { retry: false },
      mutations: { retry: false },
    },
  });
  
  return ({ children }: { children: React.ReactNode }) => (
    <QueryClientProvider client={queryClient}>
      {children}
    </QueryClientProvider>
  );
};

describe('useDocumentUpload Hook', () => {
  beforeEach(() => {
    vi.resetAllMocks();
  });

  afterEach(() => {
    vi.unstubAllGlobals();
  });

  it('deve iniciar com estados iniciais', () => {
    const { result } = renderHook(() => useDocumentUpload(), {
      wrapper: createWrapper(),
    });

    expect(result.current.isUploading).toBe(false);
    expect(result.current.progress).toBe(0);
  });

  it('deve retornar função uploadDocument', () => {
    const { result } = renderHook(() => useDocumentUpload(), {
      wrapper: createWrapper(),
    });

    expect(typeof result.current.uploadDocument).toBe('function');
  });

  it('deve mostrar erro quando providerId vazio', async () => {
    const { authenticatedFetch } = await import('@/lib/api/fetch-client');
    const toast = (await import('sonner')).toast;

    const { result } = renderHook(() => useDocumentUpload(), {
      wrapper: createWrapper(),
    });

    const file = new File(['test'], 'test.pdf', { type: 'application/pdf' });
    await act(async () => {
      await result.current.uploadDocument(file, EDocumentType.ID, '');
    });

    expect(result.current.isUploading).toBe(false);
    expect(toast.error).toHaveBeenCalledWith('Erro de autenticação ou ID do prestador inválido.');
    expect(authenticatedFetch).not.toHaveBeenCalled();
  });

  it('deve fazer upload com sucesso', async () => {
    const { authenticatedFetch } = await import('@/lib/api/fetch-client');
    const toast = (await import('sonner')).toast;
    
    vi.mocked(authenticatedFetch)
      .mockResolvedValueOnce({ 
        uploadUrl: 'https://storage.blob.core.windows.net/upload?token',
        blobName: 'test-blob'
      })
      .mockResolvedValueOnce({ success: true });

    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({ ok: true }));

    const { result } = renderHook(() => useDocumentUpload(), {
      wrapper: createWrapper(),
    });

    const file = new File(['test'], 'test.pdf', { type: 'application/pdf' });
    await act(async () => {
      await result.current.uploadDocument(file, EDocumentType.ID, 'provider-123');
    });

    await waitFor(() => {
      expect(result.current.isUploading).toBe(false);
    });

    expect(toast.success).toHaveBeenCalledWith('Documento enviado com sucesso!');
  });

  it('deve tratar erro de API', async () => {
    const { authenticatedFetch } = await import('@/lib/api/fetch-client');
    const toast = (await import('sonner')).toast;
    vi.mocked(authenticatedFetch).mockRejectedValueOnce(new Error('Upload failed'));

    const { result } = renderHook(() => useDocumentUpload(), {
      wrapper: createWrapper(),
    });

    const file = new File(['test'], 'test.pdf', { type: 'application/pdf' });
    await act(async () => {
      await result.current.uploadDocument(file, EDocumentType.ID, 'provider-123');
    });

    expect(result.current.isUploading).toBe(false);
    expect(toast.error).toHaveBeenCalledWith('Upload failed');
  });
});
