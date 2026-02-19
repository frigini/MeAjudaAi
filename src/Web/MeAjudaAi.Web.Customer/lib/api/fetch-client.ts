import { client } from "@/lib/api/client";
import { ApiResponse } from "@/types/api";

type HttpMethod = "GET" | "POST" | "PUT" | "DELETE" | "PATCH";

interface FetchOptions {
    method?: HttpMethod;
    body?: unknown;
    headers?: Record<string, string>;
    token?: string | null;
}

export async function authenticatedFetch<T>(endpoint: string, options: FetchOptions = {}): Promise<T | undefined> {
    const { method = "GET", body, headers = {}, token } = options;

    const config = client.getConfig();
    const baseUrl = config.baseUrl || process.env.NEXT_PUBLIC_API_URL || "http://localhost:7002"; // Default call

    if (!token) {
        throw new Error("Missing access token");
    }

    const requestHeaders: Record<string, string> = {
        Authorization: `Bearer ${token}`,
        ...headers,
    };

    if (body) {
        requestHeaders["Content-Type"] = "application/json";
    }

    const response = await fetch(`${baseUrl}${endpoint}`, {
        method,
        headers: requestHeaders,
        body: body ? JSON.stringify(body) : undefined,
    });

    if (!response.ok) {
        const error = await response.json().catch(() => ({}));
        const userMessage = error.message || error.detail || `Request failed: ${response.statusText}`;
        const err = new Error(userMessage);
        (err as any).status = response.status;
        throw err;
    }

    // Handle 204 No Content
    if (response.status === 204) {
        return undefined;
    }

    const json = await response.json();

    // Normalize Result<T> wrapper
    if (json && typeof json === 'object' && 'value' in json) {
        const value = (json as any).value;
        if (value === null || value === undefined) {
            throw new Error("Response contained null/undefined value for expected Result<T>");
        }
        return value as T;
    }

    // Handle ApiResponse<T> wrapper
    if (json && typeof json === 'object' && 'data' in json) {
        const apiRes = json as ApiResponse<T>;
        if (apiRes.data === null) {
            throw new Error(apiRes.message || "API interaction failed");
        }
        return apiRes.data as T;
    }

    return json as T;
}
