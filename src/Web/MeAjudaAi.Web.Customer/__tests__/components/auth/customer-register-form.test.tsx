import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { CustomerRegisterForm } from '@/components/auth/customer-register-form';
import { useRouter } from 'next/navigation';
import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest';
import { act } from '@testing-library/react';

// Mocks for dependencies that are already proven stable
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

vi.mock('@/components/ui/input', () => ({
  Input: (props: React.InputHTMLAttributes<HTMLInputElement>) => <input {...props} />,
}));

vi.mock('@/components/ui/button', () => ({
  Button: ({ children, ...props }: React.ButtonHTMLAttributes<HTMLButtonElement>) => <button {...props}>{children}</button>,
}));

vi.mock('@/components/ui/checkbox', () => ({
  Checkbox: ({ checked, onCheckedChange, id }: { checked: boolean; onCheckedChange: (checked: boolean) => void; id: string }) => (
    <input 
      type="checkbox" 
      id={id} 
      checked={checked} 
      onChange={(e) => onCheckedChange(e.target.checked)} 
    />
  ),
}));

vi.mock('lucide-react', () => ({
  Eye: () => <div data-testid="eye-icon" />,
  EyeOff: () => <div data-testid="eye-off-icon" />,
  Loader2: () => <div data-testid="loader-icon" />,
}));


describe('CustomerRegisterForm (Stabilized for CI)', () => {
  const mockPush = vi.fn();

  beforeEach(() => {
    vi.clearAllMocks();
    vi.mocked(useRouter).mockReturnValue({ push: mockPush } as unknown as ReturnType<typeof useRouter>);
  });

  afterEach(() => {
    vi.useRealTimers();
  });

  it('deve renderizar todos os campos do formulário', () => {
    render(<CustomerRegisterForm />);
    expect(screen.getByLabelText(/nome completo/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/email/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/celular/i)).toBeInTheDocument();
  });

  it('deve alternar a visibilidade da senha', () => {
    render(<CustomerRegisterForm />);
    const passwordInput = screen.getByLabelText(/^senha$/i);
    const toggleButton = screen.getAllByLabelText(/mostrar senha/i)[0];

    expect(passwordInput).toHaveAttribute('type', 'password');
    fireEvent.click(toggleButton);
    expect(passwordInput).toHaveAttribute('type', 'text');
  });

  it('deve aplicar máscara no campo de telefone', () => {
    render(<CustomerRegisterForm />);
    const phoneInput = screen.getByLabelText(/celular/i);
    
    fireEvent.change(phoneInput, { target: { value: '11988887777' } });
    expect(phoneInput).toHaveValue('(11) 98888-7777');
  });

  it('deve aplicar máscara de 10 dígitos no campo de telefone', () => {
    render(<CustomerRegisterForm />);
    const phoneInput = screen.getByLabelText(/celular/i);
    
    fireEvent.change(phoneInput, { target: { value: '1198887777' } });
    expect(phoneInput).toHaveValue('(11) 9888-7777');
  });

  it('deve validar termos de uso', async () => {
    render(<CustomerRegisterForm />);
    const submitBtn = screen.getByRole('button', { name: /criar conta/i });
    
    // Fill all but terms
    fireEvent.change(screen.getByLabelText(/nome completo/i), { target: { value: 'João da Silva' } });
    fireEvent.change(screen.getByLabelText(/email/i), { target: { value: 'joao@email.com' } });
    fireEvent.change(screen.getByLabelText(/celular/i), { target: { value: '11988887777' } });
    fireEvent.change(screen.getByLabelText(/^senha$/i), { target: { value: 'Senha123' } });
    fireEvent.change(screen.getByLabelText(/confirmar senha/i), { target: { value: 'Senha123' } });
    
    fireEvent.click(submitBtn);

    // Note: This test works because it triggers the BUILT-IN browser/RHF validation 
    // rather than the crashing Zod resolver in this specific JSDOM environment.
    await waitFor(() => {
      expect(screen.getByText(/você deve aceitar os termos de uso/i)).toBeInTheDocument();
    });
  });

  it('deve exibir erro da API ao falhar registro', async () => {
    const { publicFetch, ApiError } = await import('@/lib/api/fetch-client');
    vi.mocked(publicFetch).mockRejectedValueOnce(new ApiError('Email já cadastrado'));
    
    render(<CustomerRegisterForm />);
    
    // Fill all fields
    fireEvent.change(screen.getByLabelText(/nome completo/i), { target: { value: 'João da Silva' } });
    fireEvent.change(screen.getByLabelText(/email/i), { target: { value: 'joao@email.com' } });
    fireEvent.change(screen.getByLabelText(/celular/i), { target: { value: '11988887777' } });
    fireEvent.change(screen.getByLabelText(/^senha$/i), { target: { value: 'Senha123' } });
    fireEvent.change(screen.getByLabelText(/confirmar senha/i), { target: { value: 'Senha123' } });
    fireEvent.click(screen.getByLabelText(/eu aceito os termos/i));
    
    fireEvent.click(screen.getByRole('button', { name: /criar conta/i }));

    // Verify the API was called
    await waitFor(() => {
      expect(publicFetch).toHaveBeenCalled();
    });
    
    // Verify the error message is shown in the form
    await waitFor(() => {
      expect(screen.getByText(/email já cadastrado/i)).toBeInTheDocument();
    });
  });

  it('deve redirecionar o usuário após sucesso no registro', async () => {
    vi.useFakeTimers();
    
    const { publicFetch } = await import('@/lib/api/fetch-client');
    const fetchMock = vi.mocked(publicFetch).mockResolvedValueOnce({ 
      success: true, 
      data: { id: '123', name: 'João da Silva', email: 'joao@email.com' } 
    });
    
    render(<CustomerRegisterForm />);
    
    // Fill all fields
    fireEvent.change(screen.getByLabelText(/nome completo/i), { target: { value: 'João da Silva' } });
    fireEvent.change(screen.getByLabelText(/email/i), { target: { value: 'joao@email.com' } });
    fireEvent.change(screen.getByLabelText(/celular/i), { target: { value: '11988887777' } });
    fireEvent.change(screen.getByLabelText(/^senha$/i), { target: { value: 'Senha123' } });
    fireEvent.change(screen.getByLabelText(/confirmar senha/i), { target: { value: 'Senha123' } });
    fireEvent.click(screen.getByLabelText(/eu aceito os termos/i));
    
    await act(async () {
      fireEvent.click(screen.getByRole('button', { name: /criar conta/i }));
    });

    // Wait for the network call to complete
    await waitFor(() => expect(fetchMock).toHaveBeenCalled());

    // Advance timers to trigger the redirect
    await act(async () {
      vi.advanceTimersByTime(2000);
    });

    expect(mockPush).toHaveBeenCalledWith('/auth/login');
    
    vi.useRealTimers();
  });
});
    
    render(<CustomerRegisterForm />);
    
    // Fill all fields
    fireEvent.change(screen.getByLabelText(/nome completo/i), { target: { value: 'João da Silva' } });
    fireEvent.change(screen.getByLabelText(/email/i), { target: { value: 'joao@email.com' } });
    fireEvent.change(screen.getByLabelText(/celular/i), { target: { value: '11988887777' } });
    fireEvent.change(screen.getByLabelText(/^senha$/i), { target: { value: 'Senha123' } });
    fireEvent.change(screen.getByLabelText(/confirmar senha/i), { target: { value: 'Senha123' } });
    fireEvent.click(screen.getByLabelText(/eu aceito os termos/i));
    
    await act(async () => {
      fireEvent.click(screen.getByRole('button', { name: /criar conta/i }));
    });

    // Advance timers to trigger the redirect
    await act(async () => {
      vi.advanceTimersByTime(2000);
    });

    expect(mockPush).toHaveBeenCalledWith('/auth/login');
    
    vi.useRealTimers();
  });
});
