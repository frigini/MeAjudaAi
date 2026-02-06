import { auth } from '@/auth';

// Helper para obter headers com autenticação (Server Components)
export async function getAuthHeaders() {
    const session = await auth();
    const headers: HeadersInit = {
        'Content-Type': 'application/json',
    };

    if (session?.accessToken) {
        headers['Authorization'] = `Bearer ${session.accessToken}`;
    } else {
        console.warn('[API Client] No access token found in session — request will be unauthenticated');
    }

    return headers;
}
