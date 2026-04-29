import { useRouter } from 'next/navigation';
import { useSse } from './use-sse';
import { toast } from 'sonner';

export interface ProviderVerificationSseDto {
  providerId: string;
  status: string;
  updatedAt: string;
  rejectionReason?: string;
}

/**
 * Hook para ouvir atualizações de status de verificação do prestador.
 * @param providerId ID do prestador
 */
export function useProviderVerificationEvents(providerId: string | undefined) {
  const router = useRouter();
  const apiUrl = process.env.NEXT_PUBLIC_API_URL || '';
  const url = providerId ? `${apiUrl}/api/v1/providers/${providerId}/verification-events` : '';

  const { lastMessage, isConnected } = useSse<ProviderVerificationSseDto>(url, {
    enabled: !!providerId,
    onMessage: (data) => {
      // Força o refresh da página atual para atualizar o status visual
      router.refresh();
      
      const statusLabels: Record<string, string> = {
        'Verified': 'Verificado',
        'Rejected': 'Rejeitado',
        'Pending': 'Pendente',
        'UnderReview': 'Em Análise'
      };

      const label = statusLabels[data.status] || data.status;
      
      toast.success(`Status de verificação atualizado: ${label}`, {
        description: data.rejectionReason ? `Motivo: ${data.rejectionReason}` : 'Seu perfil foi atualizado.',
        duration: 5000,
      });
    }
  });

  return { lastMessage, isConnected };
}
