import { auth } from '@/auth';

// Helper para obter headers com autenticação (Server Components)
export async function getAuthHeaders() {
    const session = await auth();
    const headers: Record<string, string> = {};

    if (session?.accessToken) {
        headers['Authorization'] = `Bearer ${session.accessToken}`;
    }

    return headers;
}
