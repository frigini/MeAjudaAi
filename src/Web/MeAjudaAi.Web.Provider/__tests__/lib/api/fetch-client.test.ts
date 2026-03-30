import { describe, it, expect, vi, beforeEach } from 'vitest';
import { baseFetch, authenticatedFetch, publicFetch, ApiError } from '@/lib/api/fetch-client';

// Mock the client to provide a base URL
vi.mock('@/lib/api/client', () => ({
    client: {
        getConfig: () => ({
            baseUrl: 'http://api.test',
            headers: new Headers()
        })
    }
}));

describe('fetch-client', () => {
    beforeEach(() => {
        vi.stubGlobal('fetch', vi.fn());
    });

    describe('ApiError', () => {
        it('should create an error with status', () => {
            const error = new ApiError('Test error', 400);
            expect(error.message).toBe('Test error');
            expect(error.status).toBe(400);
            expect(error.name).toBe('ApiError');
        });
    });

    describe('normalizeResponse', () => {
        it('should return undefined for 204 No Content', async () => {
            const mockResponse = new Response(null, { status: 204 });
            vi.mocked(fetch).mockResolvedValue(mockResponse);

            const result = await publicFetch('/test');
            expect(result).toBeUndefined();
        });

        it('should unwrap Result<T> wrapper', async () => {
            const mockData = { id: 1, name: 'Test' };
            const mockResponse = new Response(JSON.stringify({ value: mockData }), { status: 200 });
            vi.mocked(fetch).mockResolvedValue(mockResponse);

            const result = await publicFetch('/test');
            expect(result).toEqual(mockData);
        });

        it('should unwrap ApiResponse<T> wrapper', async () => {
            const mockData = { id: 2, name: 'API' };
            const mockResponse = new Response(JSON.stringify({ data: mockData }), { status: 200 });
            vi.mocked(fetch).mockResolvedValue(mockResponse);

            const result = await publicFetch('/test');
            expect(result).toEqual(mockData);
        });

        it('should throw ApiError if Result<T> value is null', async () => {
            const mockResponse = new Response(JSON.stringify({ value: null }), { status: 200 });
            vi.mocked(fetch).mockResolvedValue(mockResponse);

            await expect(publicFetch('/test')).rejects.toThrow('Response contained null/undefined value');
        });

        it('should return undefined for malformed JSON response', async () => {
            const mockResponse = new Response('not a json', { status: 200 });
            vi.mocked(fetch).mockResolvedValue(mockResponse);

            const result = await publicFetch('/test');
            expect(result).toBeUndefined();
        });

        it('should throw ApiError if ApiResponse data is null', async () => {
            const mockResponse = new Response(JSON.stringify({ data: null, message: 'Object not found' }), { status: 200 });
            vi.mocked(fetch).mockResolvedValue(mockResponse);

            await expect(publicFetch('/test')).rejects.toThrow('Object not found');
        });

        it('should return raw JSON if no wrapper is found', async () => {
            const mockData = { simple: 'data' };
            const mockResponse = new Response(JSON.stringify(mockData), { status: 200 });
            vi.mocked(fetch).mockResolvedValue(mockResponse);

            const result = await publicFetch('/test');
            expect(result).toEqual(mockData);
        });
    });

    describe('error mapping', () => {
        it('should map standard error message', async () => {
            const mockResponse = new Response(JSON.stringify({ message: 'Standard error' }), { 
                status: 400,
                statusText: 'Bad Request'
            });
            vi.mocked(fetch).mockResolvedValue(mockResponse);

            await expect(publicFetch('/test')).rejects.toThrow('Standard error');
        });

        it('should map Result<T> error description', async () => {
            const mockResponse = new Response(JSON.stringify({ 
                error: { description: 'Result error description' } 
            }), { status: 400 });
            vi.mocked(fetch).mockResolvedValue(mockResponse);

            await expect(publicFetch('/test')).rejects.toThrow('Result error description');
        });

        it('should map validation errors from .NET', async () => {
            const mockResponse = new Response(JSON.stringify({ 
                errors: { 
                    Email: ['Invalid email format', 'Email already exists'],
                    Password: ['Too short']
                } 
            }), { status: 400 });
            vi.mocked(fetch).mockResolvedValue(mockResponse);

            // It joins all messages
            try {
                await publicFetch('/test');
            } catch (e: any) {
                expect(e.message).toContain('Invalid email format');
                expect(e.message).toContain('Email already exists');
                expect(e.message).toContain('Too short');
            }
        });

        it('should fall back to statusText if no body message is found', async () => {
            const mockResponse = new Response('', { 
                status: 500,
                statusText: 'Internal Server Error'
            });
            vi.mocked(fetch).mockResolvedValue(mockResponse);

            await expect(publicFetch('/test')).rejects.toThrow('Internal Server Error');
        });
    });

    describe('baseFetch security', () => {
        it('should throw error if auth is required but no token provided', async () => {
            await expect(authenticatedFetch('/secure')).rejects.toThrow('Missing access token');
        });

        it('should include Authorization header when token is provided', async () => {
            const mockResponse = new Response(JSON.stringify({ ok: true }));
            vi.mocked(fetch).mockResolvedValue(mockResponse);

            await authenticatedFetch('/secure', { token: 'mock-token' });

            const lastCall = vi.mocked(fetch).mock.calls[0];
            const headers = lastCall[1]?.headers as Record<string, string>;
            expect(headers['Authorization']).toBe('Bearer mock-token');
        });

        it('should include Content-Type header when body is provided', async () => {
            const mockResponse = new Response(JSON.stringify({ ok: true }));
            vi.mocked(fetch).mockResolvedValue(mockResponse);

            await publicFetch('/post', { method: 'post', body: { name: 'test' } });

            const lastCall = vi.mocked(fetch).mock.calls[0];
            const headers = lastCall[1]?.headers as Record<string, string>;
            expect(headers['Content-Type']).toBe('application/json');
            expect(lastCall[1]?.body).toBe(JSON.stringify({ name: 'test' }));
        });
    });
});
