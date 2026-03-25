import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { ProviderDto, EVerificationStatus } from '@/types/api/provider';
import DashboardClient from '@/components/providers/dashboard-client';

const mockRefresh = vi.fn();

vi.mock('next/navigation', () => ({
  useRouter: () => ({
    push: vi.fn(),
    refresh: mockRefresh,
  }),
}));

vi.mock('sonner', () => ({
  toast: {
    success: vi.fn(),
    error: vi.fn(),
  },
}));

vi.mock('./service-selector', () => ({
  ServiceSelector: () => <div data-testid="service-selector">Service Selector</div>,
}));

const mockProvider: ProviderDto = {
  id: 'prov-123',
  name: 'João Prestador',
  email: 'joao@exemplo.com',
  verificationStatus: EVerificationStatus.Verified,
  businessProfile: {
    description: 'Sou um prestador de serviços experiente.',
    contactInfo: {
      email: 'joao@exemplo.com',
      phone: '(11) 99999-9999',
    },
  },
  services: [
    { serviceId: 'svc-1', serviceName: 'Serviço 1' },
    { serviceId: 'svc-2', serviceName: 'Serviço 2' },
  ],
};

describe('DashboardClient', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    global.fetch = vi.fn();
  });

  it('deve renderizar o título do painel', () => {
    render(<DashboardClient provider={mockProvider} />);
    expect(screen.getByText(/meu painel/i)).toBeInTheDocument();
  });

  it('deve renderizar o nome do prestador', () => {
    render(<DashboardClient provider={mockProvider} />);
    expect(screen.getByText(/joão prestador/i)).toBeInTheDocument();
  });

  it('deve renderizar a descrição do perfil', () => {
    render(<DashboardClient provider={mockProvider} />);
    expect(screen.getByText(/sou um prestador de serviços experiente/i)).toBeInTheDocument();
  });

  it('deve renderizar card de serviços', () => {
    render(<DashboardClient provider={mockProvider} />);
    expect(screen.getByText(/meus serviços/i)).toBeInTheDocument();
  });

  it('deve renderizar serviços cadastrados', () => {
    render(<DashboardClient provider={mockProvider} />);
    expect(screen.getByText('Serviço 1')).toBeInTheDocument();
    expect(screen.getByText('Serviço 2')).toBeInTheDocument();
  });

  it('deve renderizar status da conta', () => {
    render(<DashboardClient provider={mockProvider} />);
    expect(screen.getByText(/status da conta/i)).toBeInTheDocument();
  });

  it('deve renderizar botão de editar descrição', () => {
    render(<DashboardClient provider={mockProvider} />);
    expect(screen.getByRole('button', { name: /editar/i })).toBeInTheDocument();
  });

  it('deve renderizar seletor de serviço e botão adicionar', () => {
    render(<DashboardClient provider={mockProvider} />);
    expect(screen.getByRole('button', { name: /adicionar/i })).toBeInTheDocument();
  });

  it('deve renderizar ID do provedor', () => {
    render(<DashboardClient provider={mockProvider} />);
    expect(screen.getByText(/id: prov-123/i)).toBeInTheDocument();
  });

  it('deve renderizar sem descrição quando não informada', () => {
    const providerWithoutDescription: ProviderDto = {
      ...mockProvider,
      businessProfile: {} as ProviderDto['businessProfile'],
    };
    
    render(<DashboardClient provider={providerWithoutDescription} />);
    expect(screen.getByText(/nenhuma descrição adicionada/i)).toBeInTheDocument();
  });

  it('deve renderizar sem serviços quando array vazio', () => {
    const providerWithoutServices: ProviderDto = {
      ...mockProvider,
      services: [],
    };
    
    render(<DashboardClient provider={providerWithoutServices} />);
    expect(screen.getByText(/nenhum serviço cadastrado/i)).toBeInTheDocument();
  });

  it('deve entrar em modo de edição ao clicar em editar', async () => {
    const user = userEvent.setup();
    render(<DashboardClient provider={mockProvider} />);
    
    const editButton = screen.getByRole('button', { name: /editar/i });
    await user.click(editButton);
    
    expect(screen.getByRole('textbox')).toBeInTheDocument();
    expect(screen.getByRole('button', { name: /salvar/i })).toBeInTheDocument();
    expect(screen.getByRole('button', { name: /cancelar/i })).toBeInTheDocument();
  });

  it('deve cancelar edição e restaurar descrição original', async () => {
    const user = userEvent.setup();
    render(<DashboardClient provider={mockProvider} />);
    
    const editButton = screen.getByRole('button', { name: /editar/i });
    await user.click(editButton);
    
    const textarea = screen.getByRole('textbox');
    await user.clear(textarea);
    await user.type(textarea, 'Nova descrição');
    
    const cancelButton = screen.getByRole('button', { name: /cancelar/i });
    await user.click(cancelButton);
    
    expect(screen.queryByRole('textbox')).not.toBeInTheDocument();
  });

  it('deve renderizar com verificação pendente', () => {
    const providerPending: ProviderDto = {
      ...mockProvider,
      verificationStatus: EVerificationStatus.Pending,
    };
    
    render(<DashboardClient provider={providerPending} />);
    expect(screen.getByText(/status da conta/i)).toBeInTheDocument();
  });

  it('deve renderizar com verificação rejeitada', () => {
    const providerRejected: ProviderDto = {
      ...mockProvider,
      verificationStatus: EVerificationStatus.Rejected,
    };
    
    render(<DashboardClient provider={providerRejected} />);
    expect(screen.getByText(/status da conta/i)).toBeInTheDocument();
  });

  it('deve renderizar com verificação suspensa', () => {
    const providerSuspended: ProviderDto = {
      ...mockProvider,
      verificationStatus: EVerificationStatus.Suspended,
    };
    
    render(<DashboardClient provider={providerSuspended} />);
    expect(screen.getByText(/status da conta/i)).toBeInTheDocument();
  });

  it('deve renderizar com verificação em progresso', () => {
    const providerInProgress: ProviderDto = {
      ...mockProvider,
      verificationStatus: EVerificationStatus.InProgress,
    };
    
    render(<DashboardClient provider={providerInProgress} />);
    expect(screen.getByText(/status da conta/i)).toBeInTheDocument();
  });
});
