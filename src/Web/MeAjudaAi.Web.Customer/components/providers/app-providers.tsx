'use client';

import { SessionProvider } from "next-auth/react";
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { ReactQueryDevtools } from '@tanstack/react-query-devtools';
import { useState, useEffect } from 'react';
import { client } from '@/lib/api/generated/client.gen';

export function AppProviders({ children }: { children: React.ReactNode }) {
    // Sync client config with environment variable or window location
    useEffect(() => {
        const apiUrl = process.env.NEXT_PUBLIC_API_URL || window.location.origin;
        client.setConfig({
            baseUrl: apiUrl
        });
    }, []);
    const [queryClient] = useState(
        () =>
            new QueryClient({
                defaultOptions: {
                    queries: {
                        // Cache para queries do servidor
                        staleTime: 60 * 1000,
                    },
                },
            })
    );

    return (
        <SessionProvider>
            <QueryClientProvider client={queryClient}>
                {children}
                <ReactQueryDevtools initialIsOpen={false} />
            </QueryClientProvider>
        </SessionProvider>
    );
}
