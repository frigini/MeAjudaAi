import { auth } from '@/auth';

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
