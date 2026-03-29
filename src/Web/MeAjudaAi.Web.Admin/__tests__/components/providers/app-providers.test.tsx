import React from 'react';
import { describe, it, expect, vi } from 'vitest';
import { render, screen } from 'test-support';
import { AppProviders } from '@/components/providers/app-providers';

vi.mock('next-auth/react', async (importOriginal) => {
  const actual = await importOriginal<typeof import('next-auth/react')>();
  let capturedSession: unknown = null;
  return {
    ...actual,
    SessionProvider: ({ session, children }: { session?: unknown; children: React.ReactNode }) => {
      capturedSession = session;
      return <>{children}</>;
    },
    useSession: () => [capturedSession, false] as const,
  };
});

describe('AppProviders (Admin)', () => {
  it('deve renderizar children corretamente', () => {
    render(
      <AppProviders>
        <div data-testid="test-child">Conteúdo de Teste</div>
      </AppProviders>
    );

    expect(screen.getByTestId('test-child')).toBeInTheDocument();
    expect(screen.getByText('Conteúdo de Teste')).toBeInTheDocument();
  });

  it('deve renderizar com session inicial', async () => {
    const mockSession = {
      user: { name: 'Admin Test' },
      expires: new Date(Date.now() + 3600 * 1000).toISOString(),
    };

    render(
      <AppProviders session={mockSession}>
        <div>Authenticated Content</div>
      </AppProviders>
    );

    expect(screen.getByText('Authenticated Content')).toBeInTheDocument();
    const { useSession } = await import('next-auth/react');
    expect(useSession()[0]).toEqual(mockSession);
  });
});
