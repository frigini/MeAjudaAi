import { describe, it, expect, vi } from 'vitest';
import { render, screen } from '@testing-library/react';
import { EditProfileForm } from '@/components/profile/edit-profile-form';

vi.mock('next/navigation', () => ({
  useRouter: () => ({
    push: vi.fn(),
  }),
}));

vi.mock('next-auth/react', () => ({
  useSession: vi.fn(() => ({
    data: { accessToken: 'mock-token' },
    status: 'authenticated',
  })),
}));

vi.mock('@/lib/api/generated/sdk.gen', () => ({
  apiProfilePut: vi.fn(),
}));

vi.mock('sonner', () => ({
  toast: {
    success: vi.fn(),
    error: vi.fn(),
  },
}));

describe('EditProfileForm', () => {
  it('deve renderizar o formulário de edição de perfil', () => {
    render(
      <EditProfileForm
        userId="user-1"
        initialData={{ firstName: 'João', lastName: 'Silva', email: 'joao@test.com' }}
      />
    );
    expect(screen.getByText('Nome')).toBeInTheDocument();
    expect(screen.getByText('Sobrenome')).toBeInTheDocument();
  });

  it('deve renderizar botão salvar', () => {
    render(
      <EditProfileForm
        userId="user-1"
        initialData={{ firstName: 'João', lastName: 'Silva', email: 'joao@test.com' }}
      />
    );
    expect(screen.getByRole('button', { name: /salvar/i })).toBeInTheDocument();
  });
});
