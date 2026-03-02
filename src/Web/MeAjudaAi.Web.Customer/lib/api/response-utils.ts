/**
 * Safely unwraps an OpenAPI response by checking explicitly for common wrapper properties
 * without resorting to 'any'.
 */
export function unwrapResponse<T>(response: unknown): T | undefined {
    if (!response) {
        return undefined;
    }

    if (typeof response === 'object') {
        // Handle { value: T } wrapper
        if ('value' in response) {
            return (response as { value: T }).value;
        }

        // Handle { result: T } wrapper
        if ('result' in response) {
            return (response as { result: T }).result;
        }
    }

    // Default to casting the response itself if it's not wrapped
    return response as T;
}
