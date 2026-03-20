import { client } from './generated/client.gen';

// Configuração global do cliente
client.setConfig({
    baseUrl: process.env.NEXT_PUBLIC_API_URL || 'http://localhost:7002',
    headers: {
        'Content-Type': 'application/json',
    },
});

export { client };
