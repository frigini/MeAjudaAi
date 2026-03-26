import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { CustomerRegisterForm } from '@/components/auth/customer-register-form';
import { useRouter } from 'next/navigation';
import { publicFetch } from '@/lib/api/fetch-client';
import { toast } from 'sonner';
import { describe, it, expect, vi, beforeEach } from 'vitest';

vi.mock('next/navigation', () => ({
  useRouter: vi.fn(),
}));

vi.mock('@/lib/api/fetch-client', () => ({
  publicFetch: vi.fn(),
  ApiError: class extends Error {
    constructor(public message: string) {
      super(message);
    }
  },
}));

vi.mock('sonner', () => ({
  toast: {
    success: vi.fn(),
    error: vi.fn(),
  },
}));

describe('CustomerRegisterForm', () => {
  const mockPush = vi.fn();

  beforeEach(() => {
    vi.clearAllMocks();
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    vi.mocked(useRouter).mockReturnValue({ push: mockPush } as any);
    vi.useFakeTimers();
  });

  afterEach(() => {
    vi.useRealTimers();
  });

  it('deve renderizar todos os campos do formulário', () => {
    render(<CustomerRegisterForm />);
    expect(screen.getByLabelText(/nome completo/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/email/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/celular/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/^senha$/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/confirmar senha/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/li e aceito os termos/i)).toBeInTheDocument();
    expect(screen.getByRole('button', { name: /criar conta/i })).toBeInTheDocument();
  });

  it('deve alternar a visibilidade da senha', () => {
    render(<CustomerRegisterForm />);
    const passwordInput = screen.getByLabelText(/^senha$/i);
    const toggleButton = screen.getByLabelText(/mostrar senha/i);

    expect(passwordInput).toHaveAttribute('type', 'password');
    fireEvent.click(toggleButton);
    expect(passwordInput).toHaveAttribute('type', 'text');
    fireEvent.click(screen.getByLabelText(/ocultar senha/i));
    expect(passwordInput).toHaveAttribute('type', 'password');
  });

  it('deve aplicar máscara no campo de telefone', () => {
    render(<CustomerRegisterForm />);
    const phoneInput = screen.getByLabelText(/celular/i);
    
    fireEvent.change(phoneInput, { target: { value: '11988887777' } });
    expect(phoneInput).toHaveValue('(11) 98888-7777');
  });

  it('deve validar campos obrigatórios ao submeter vazio', async () => {
    render(<CustomerRegisterForm />);
    fireEvent.click(screen.getByRole('button', { name: /criar conta/i }));

    expect(await screen.findByText(/nome deve ter pelo menos 4 caracteres/i)).toBeInTheDocument();
    expect(await screen.findByText(/email inválido/i)).toBeInTheDocument();
    expect(await screen.findByText(/você deve aceitar os termos de uso/i)).toBeInTheDocument();
  });

  it('deve validar se as senhas conferem', async () => {
    render(<CustomerRegisterForm />);
    
    fireEvent.change(screen.getByLabelText(/^senha$/i), { target: { value: 'Senha123' } });
    fireEvent.change(screen.getByLabelText(/confirmar senha/i), { target: { value: 'Senha456' } });
    fireEvent.click(screen.getByRole('button', { name: /criar conta/i }));

    expect(await screen.findByText(/as senhas não conferem/i)).toBeInTheDocument();
  });

  it('deve submeter o formulário com sucesso', async () => {
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    vi.mocked(publicFetch).mockResolvedValue({} as any);
    
    render(<CustomerRegisterForm />);
    
    fireEvent.change(screen.getByLabelText(/nome completo/i), { target: { value: 'João da Silva' } });
    fireEvent.change(screen.getByLabelText(/email/i), { target: { value: 'joao@email.com' } });
    fireEvent.change(screen.getByLabelText(/celular/i), { target: { value: '11988887777' } });
    fireEvent.change(screen.getByLabelText(/^senha$/i), { target: { value: 'Senha123' } });
    fireEvent.change(screen.getByLabelText(/confirmar senha/i), { target: { value: 'Senha123' } });
    fireEvent.click(screen.getByLabelText(/li e aceito os termos/i));
    
    fireEvent.click(screen.getByRole('button', { name: /criar conta/i }));

    await waitFor(() => {
      expect(publicFetch).toHaveBeenCalledWith('/api/v1/users/register', expect.objectContaining({
        method: 'post',
        body: expect.objectContaining({
          email: 'joao@email.com',
          phoneNumber: '11988887777',
        })
      }));
    });

    expect(toast.success).toHaveBeenCalledWith("Conta criada com sucesso!", expect.anything());
    
    vi.advanceTimersByTime(1000);
    expect(mockPush).toHaveBeenCalledWith('/auth/signin');
  });

  it('deve lidar com erro na API ao submeter', async () => {
    const error = new Error('Email já em uso');
    // @ts-ignore - Mocking internal ApiError structure
    error.name = 'ApiError';
    vi.mocked(publicFetch).mockRejectedValue(error);

    render(<CustomerRegisterForm />);
    
    // Fill minimum required for valid submission
    fireEvent.change(screen.getByLabelText(/nome completo/i), { target: { value: 'João da Silva' } });
    fireEvent.change(screen.getByLabelText(/email/i), { target: { value: 'joao@email.com' } });
    fireEvent.change(screen.getByLabelText(/celular/i), { target: { value: '11988887777' } });
    fireEvent.change(screen.getByLabelText(/^senha$/i), { target: { value: 'Senha123' } });
    fireEvent.change(screen.getByLabelText(/confirmar senha/i), { target: { value: 'Senha123' } });
    fireEvent.click(screen.getByLabelText(/li e aceito os termos/i));
    
    fireEvent.click(screen.getByRole('button', { name: /criar conta/i }));

    await waitFor(() => {
      expect(toast.error).toHaveBeenCalledWith("Erro no cadastro", expect.anything());
    });
    
    expect(screen.getByRole('button', { name: /criar conta/i })).not.toBeDisabled();
  });
});
