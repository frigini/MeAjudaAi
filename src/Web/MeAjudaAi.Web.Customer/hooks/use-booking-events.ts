import { useQueryClient } from '@tanstack/react-query';
import { useSse } from './use-sse';
import { toast } from 'sonner';

export interface BookingStatusSseDto {
  bookingId: string;
  status: string;
  updatedAt: string;
  message?: string;
}

/**
 * Hook para ouvir atualizações em tempo real de um agendamento específico.
 * @param bookingId ID do agendamento
 */
export function useBookingEvents(bookingId: string | undefined) {
  const queryClient = useQueryClient();
  const apiUrl = process.env.NEXT_PUBLIC_API_URL || '';
  const url = bookingId ? `${apiUrl}/api/v1/bookings/${bookingId}/events` : '';

  const { lastMessage, isConnected } = useSse<BookingStatusSseDto>(url, {
    enabled: !!bookingId,
    onMessage: (data) => {
      // Invalida a query de detalhes do agendamento para forçar o refresh
      queryClient.invalidateQueries({ queryKey: ['booking', data.bookingId] });
      
      // Exibe uma notificação amigável
      toast.info(`Status do agendamento atualizado para: ${data.status}`, {
        description: data.message || 'O status do seu agendamento mudou.',
      });
    }
  });

  return { lastMessage, isConnected };
}
