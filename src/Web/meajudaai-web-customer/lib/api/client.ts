import { client } from './generated/client.gen';

// Configuração global do cliente
client.setConfig({
    baseUrl: process.env.NEXT_PUBLIC_API_URL || 'http://localhost:7524',
    headers: {
        'Content-Type': 'application/json',
    },
});

// Helper para obter headers com autenticação (Server Components)
// MOVIDO PARA lib/api/auth-headers.ts
// export async function getAuthHeaders() { ... }

// Configurar interceptors para injetar token (Client Components / fetch direto)
// NOTA: O client do @hey-api usa fetch nativo.
// Para Server Components, chamamos getAuthHeaders() manualmente em lib/api/auth-headers.ts
// Para Client Components com useQuery, precisamos garantir que o token seja injetado.
// Como @hey-api/client-fetch não tem interceptors assíncronos robustos ainda, 
// a estratégia recomendada é passar o token via options ou usar um wrapper.

export { client };
