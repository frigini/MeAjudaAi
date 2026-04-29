import { useRouter } from 'next/navigation';
import { useSse } from './use-sse';
import { toast } from 'sonner';
import { EVerificationStatus, VERIFICATION_STATUS_LABELS } from '@/types/api/provider';

export interface ProviderVerificationSseDto {
  providerId: string;
  status: string;
  updatedAt: string;
  rejectionReason?: string;
}

export function useProviderVerificationEvents(providerId: string | undefined) {
  const router = useRouter();
  const apiUrl = process.env.NEXT_PUBLIC_API_URL || '';
  const url = providerId ? `${apiUrl}/api/v1/providers/${providerId}/verification-events` : '';

  const { lastMessage, isConnected } = useSse<ProviderVerificationSseDto>(url, {
    enabled: !!providerId,
    onMessage: (data) => {
      router.refresh();
      
      const statusKey = data.status as EVerificationStatus;
      const label = VERIFICATION_STATUS_LABELS[statusKey] || data.status;
      
      toast.success(`Status de verificação atualizado: ${label}`, {
        description: data.rejectionReason ? `Motivo: ${data.rejectionReason}` : 'Seu perfil foi atualizado.',
        duration: 5000,
      });
    }
  });

  return { lastMessage, isConnected };
}
