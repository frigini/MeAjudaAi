import { client } from './generated/client';
import { auth } from '@/auth';

// Configuração global do cliente
client.setConfig({
    baseUrl: process.env.NEXT_PUBLIC_API_URL || 'http://localhost:7524',
    headers: {
        'Content-Type': 'application/json',
    },
});

// Helper para obter headers com autenticação (Server Components)
export async function getAuthHeaders() {
    const session = await auth();
    const headers: HeadersInit = {
        'Content-Type': 'application/json',
    };

    if (session?.accessToken) {
        headers['Authorization'] = `Bearer ${session.accessToken}`;
    }

    return headers;
}

// Configurar interceptors para injetar token (Client Components / fetch direto)
// NOTA: O client do @hey-api usa fetch nativo.
// Para Server Components, chamamos getAuthHeaders() manualmente.
// Para Client Components com useQuery, precisamos garantir que o token seja injetado.
// Como @hey-api/client-fetch não tem interceptors assíncronos robustos ainda, 
// a estratégia recomendada é passar o token via options ou usar um wrapper.
// Por enquanto, vamos exportar o cliente base.

export { client };
