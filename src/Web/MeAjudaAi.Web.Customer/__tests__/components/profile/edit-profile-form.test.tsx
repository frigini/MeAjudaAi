import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { EditProfileForm } from '@/components/profile/edit-profile-form';
import { useRouter } from 'next/navigation';
import { useSession } from 'next-auth/react';
import { apiProfilePut } from '@/lib/api/generated/sdk.gen';
import { toast } from 'sonner';

// Mock high-level dependencies
vi.mock('next/navigation', () => ({
  useRouter: vi.fn(),
}));

vi.mock('next-auth/react', () => ({
  useSession: vi.fn(),
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

describe('EditProfileForm (Customer)', () => {
  const mockRouter = {
    push: vi.fn(),
    replace: vi.fn(),
    refresh: vi.fn(),
    back: vi.fn(),
  };

  const userId = 'user-123';
  const initialData = {
    firstName: 'João',
    lastName: 'Silva',
    email: 'joao.silva@example.com',
    phoneNumber: '(11) 98765-4321',
  };

  beforeEach(() => {
    vi.clearAllMocks();
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    vi.mocked(useRouter).mockReturnValue(mockRouter as any);
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    vi.mocked(useSession).mockReturnValue({
      data: { accessToken: 'valid-token' },
      status: 'authenticated',
    } as any);
  });

  it('deve renderizar os dados iniciais corretamente', () => {
    render(<EditProfileForm userId={userId} initialData={initialData} />);

    expect(screen.getByLabelText(/nome/i)).toHaveValue(initialData.firstName);
    expect(screen.getByLabelText(/sobrenome/i)).toHaveValue(initialData.lastName);
    expect(screen.getByLabelText(/email/i)).toHaveValue(initialData.email);
    expect(screen.getByLabelText(/telefone/i)).toHaveValue(initialData.phoneNumber);
  });

  it('deve exibir erro de validação para campos obrigatórios vazios', async () => {
    render(<EditProfileForm userId={userId} initialData={{}} />);

    const submitBtn = screen.getByRole('button', { name: /salvar alterações/i });
    fireEvent.click(submitBtn);

    expect(await screen.findByText(/nome deve ter pelo menos 2 caracteres/i)).toBeInTheDocument();
    expect(await screen.findByText(/sobrenome deve ter pelo menos 2 caracteres/i)).toBeInTheDocument();
  });

  it('deve validar formato de telefone inválido', async () => {
    render(<EditProfileForm userId={userId} initialData={initialData} />);

    const phoneInput = screen.getByLabelText(/telefone/i);
    fireEvent.change(phoneInput, { target: { value: '11987654321' } }); // Missing parens and hyphen

    const submitBtn = screen.getByRole('button', { name: /salvar alterações/i });
    fireEvent.click(submitBtn);

    expect(await screen.findByText(/formato inválido/i)).toBeInTheDocument();
  });

  it('deve chamar apiProfilePut e redirecionar em caso de sucesso', async () => {
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    vi.mocked(apiProfilePut).mockResolvedValue({ data: { success: true }, error: null } as any);

    render(<EditProfileForm userId={userId} initialData={initialData} />);

    fireEvent.change(screen.getByLabelText(/nome/i), { target: { value: 'João Atualizado' } });
    fireEvent.click(screen.getByRole('button', { name: /salvar alterações/i }));

    await waitFor(() => {
      expect(apiProfilePut).toHaveBeenCalledWith(expect.objectContaining({
        path: { id: userId },
        body: expect.objectContaining({
          firstName: 'João Atualizado',
          lastName: 'Silva',
        })
      }));
    });

    await waitFor(() => {
      expect(toast.success).toHaveBeenCalledWith("Perfil atualizado com sucesso!");
    });

    await waitFor(() => {
      expect(mockRouter.replace).toHaveBeenCalledWith("/perfil");
    });
  });

  it('deve exibir erro se o usuário não estiver autenticado', async () => {
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    vi.mocked(useSession).mockReturnValue({ data: null, status: 'unauthenticated' } as any);

    render(<EditProfileForm userId={userId} initialData={initialData} />);
    fireEvent.click(screen.getByRole('button', { name: /salvar alterações/i }));

    await waitFor(() => {
      expect(toast.error).toHaveBeenCalledWith("Erro de autenticação", expect.anything());
    });
    expect(apiProfilePut).not.toHaveBeenCalled();
  });

  it('deve exibir erro se a API retornar erro', async () => {
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    vi.mocked(apiProfilePut).mockResolvedValue({ data: null, error: { message: 'API Error' } } as any);

    render(<EditProfileForm userId={userId} initialData={initialData} />);
    fireEvent.click(screen.getByRole('button', { name: /salvar alterações/i }));

    await waitFor(() => {
      expect(toast.error).toHaveBeenCalledWith("Erro ao atualizar perfil");
    });
  });

  it('deve voltar para a página anterior ao clicar em cancelar', () => {
    render(<EditProfileForm userId={userId} initialData={initialData} />);
    fireEvent.click(screen.getByRole('button', { name: /cancelar/i }));

    expect(mockRouter.back).toHaveBeenCalled();
  });
});
