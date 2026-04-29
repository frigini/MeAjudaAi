import { useEffect, useRef, useState } from 'react';
import { useSession } from 'next-auth/react';
import { fetchEventSource } from '@microsoft/fetch-event-source';

export interface SseOptions<T> {
  onMessage?: (data: T) => void;
  onError?: (error: any) => void;
  enabled?: boolean;
}

/**
 * Hook para consumo de Server-Sent Events (SSE) com suporte a autenticação JWT.
 * @param url URL do endpoint SSE
 * @param options Opções de callback e controle
 */
export function useSse<T>(url: string, options: SseOptions<T> = {}) {
  const { data: session } = useSession();
  const [lastMessage, setLastMessage] = useState<T | null>(null);
  const [isConnected, setIsConnected] = useState(false);
  const ctrlRef = useRef<AbortController | null>(null);

  useEffect(() => {
    const { enabled = true, onMessage, onError } = options;

    if (!enabled || !url || !session?.accessToken) {
      return;
    }

    const ctrl = new AbortController();
    ctrlRef.current = ctrl;

    const connect = async () => {
      try {
        await fetchEventSource(url, {
          headers: {
            'Authorization': `Bearer ${session.accessToken}`,
            'Accept': 'text/event-stream',
          },
          signal: ctrl.signal,
          onopen: async (response) => {
            if (response.ok && response.headers.get('content-type')?.includes('text/event-stream')) {
              setIsConnected(true);
              return;
            }
            throw new Error(`Falha ao abrir conexão SSE: ${response.status} ${response.statusText}`);
          },
          onmessage: (msg) => {
            if (msg.event === 'Close' || msg.data === '[DONE]') {
              ctrl.abort();
              return;
            }

            try {
              const data = JSON.parse(msg.data) as T;
              setLastMessage(data);
              onMessage?.(data);
            } catch (e) {
              console.error('Erro ao processar mensagem SSE:', e, msg.data);
            }
          },
          onclose: () => {
            setIsConnected(false);
          },
          onerror: (err) => {
            setIsConnected(false);
            onError?.(err);
            // O fetchEventSource tenta reconectar automaticamente por padrão, 
            // a menos que lancemos um erro ou abortemos.
          }
        });
      } catch (err) {
        if (!ctrl.signal.aborted) {
          console.error('SSE Connection Error:', err);
          setIsConnected(false);
        }
      }
    };

    connect();

    return () => {
      ctrl.abort();
      setIsConnected(false);
    };
  }, [url, session?.accessToken, options.enabled]); // Re-conecta se a URL ou Token mudar

  return { lastMessage, isConnected };
}
