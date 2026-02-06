import { client } from './generated/client.gen';

// Configuração global do cliente
client.setConfig({
    baseUrl: process.env.NEXT_PUBLIC_API_URL || 'http://localhost:7524',
    headers: {
        'Content-Type': 'application/json',
    },
});

// See auth-headers.ts for authentication token handling

export { client };
