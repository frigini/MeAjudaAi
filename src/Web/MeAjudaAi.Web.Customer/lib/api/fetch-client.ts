import { client } from "@/lib/api/client";
import { ApiResponse } from "@/types/api";

type HttpMethod = "GET" | "POST" | "PUT" | "DELETE" | "PATCH";

export class ApiError extends Error {
    public status?: number;

    constructor(message: string, status?: number) {
        super(message);
        this.name = "ApiError";
        this.status = status;
    }
}

interface FetchOptions {
    method?: HttpMethod;
    body?: unknown;
    headers?: Record<string, string>;
    token?: string | null;
}

async function mapErrorResponse(response: Response): Promise<never> {
    const error = await response.json().catch(() => ({}));

    let userMessage = error.message || error.detail;

    // Handle Result<T> failure pattern from .NET Minimal APIs
    if (!userMessage && error.error && typeof error.error === 'object' && error.error.description) {
        userMessage = error.error.description;
    }

    // Handle .NET ValidationProblemDetails
    if (!userMessage && error.errors && typeof error.errors === 'object') {
        const errorMessages = Object.values(error.errors).flat();
        if (errorMessages.length > 0) {
            userMessage = errorMessages.join(", ");
        }
    }

    if (!userMessage) {
        userMessage = error.title || `Request failed: ${response.statusText}`;
    }

    throw new ApiError(userMessage, response.status);
}

async function normalizeResponse(response: Response): Promise<unknown> {
    // Check for empty body responses before parsing JSON
    const contentLength = response.headers.get("Content-Length");
    if (
        response.status === 204 ||
        response.status === 205 ||
        contentLength === "0"
    ) {
        return undefined;
    }

    let json;
    try {
        json = await response.json();
    } catch {
        return undefined;
    }

    // Normalize Result<T> wrapper
    if (json && typeof json === 'object' && 'value' in json) {
        const value = (json as Record<string, unknown>).value;
        if (value === null || value === undefined) {
            throw new Error("Response contained null/undefined value for expected Result<T>");
        }
        return value;
    }

    // Handle ApiResponse<T> wrapper
    if (json && typeof json === 'object' && 'data' in json) {
        const apiRes = json as ApiResponse<unknown>;
        if (apiRes.data === null || apiRes.data === undefined) {
            throw new Error(apiRes.message || "API interaction failed");
        }
        return apiRes.data;
    }

    return json;
}

interface BaseFetchOptions extends FetchOptions {
    requireAuth?: boolean;
}

async function baseFetch<T>(endpoint: string, options: BaseFetchOptions): Promise<T | undefined> {
    const { method = "GET", body, headers = {}, token, requireAuth = false } = options;
    const config = client.getConfig();
    const baseUrl = config.baseUrl || process.env.NEXT_PUBLIC_API_URL || "http://localhost:7002";

    if (requireAuth && !token) {
        throw new Error("Missing access token");
    }

    const requestHeaders: Record<string, string> = { ...headers };

    if (requireAuth && token) {
        requestHeaders.Authorization = `Bearer ${token}`;
    }

    if (body) {
        requestHeaders["Content-Type"] = "application/json";
    }

    const response = await fetch(`${baseUrl}${endpoint}`, {
        method,
        headers: requestHeaders,
        body: body ? JSON.stringify(body) : undefined,
    });

    if (!response.ok) {
        await mapErrorResponse(response);
    }

    return (await normalizeResponse(response)) as T | undefined;
}

export async function authenticatedFetch<T>(endpoint: string, options: FetchOptions = {}): Promise<T | undefined> {
    return baseFetch<T>(endpoint, { ...options, requireAuth: true });
}

export async function publicFetch<T>(endpoint: string, options: FetchOptions = {}): Promise<T | undefined> {
    return baseFetch<T>(endpoint, { ...options, requireAuth: false });
}
