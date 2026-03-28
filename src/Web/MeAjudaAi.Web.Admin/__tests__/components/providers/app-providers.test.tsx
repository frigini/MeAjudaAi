import React from 'react';
import { describe, it, expect, vi } from 'vitest';
import { render, screen } from 'test-support';
import { AppProviders } from '@/components/providers/app-providers';

vi.mock('next-auth/react', async (importOriginal) => {
  const actual = await importOriginal<typeof import('next-auth/react')>();
  return {
    ...actual,
    SessionProvider: ({ children }: { children: React.ReactNode }) => <>{children}</>,
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

  it('deve renderizar com session inicial', () => {
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
  });
});
