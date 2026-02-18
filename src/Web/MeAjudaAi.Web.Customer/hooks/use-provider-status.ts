"use client";

import { useQuery } from "@tanstack/react-query";
import { useSession } from "next-auth/react";
import { client } from "@/lib/api/client";
import { ProviderStatusDto } from "@/types/provider";

interface ApiResponse<T> {
    data: T;
    statusCode: number;
    message?: string;
    isSuccess: boolean;
}

export function useProviderStatus() {
    const { data: session } = useSession();

    return useQuery({
        queryKey: ["providerStatus", session?.user?.id],
        queryFn: async (): Promise<ProviderStatusDto | null> => {
            if (!session?.accessToken) return null;

            // Get base URL from initialized client or fallback to env/default
            const config = client.getConfig();
            const baseUrl = config.baseUrl || process.env.NEXT_PUBLIC_API_URL || "http://localhost:7002";

            const response = await fetch(`${baseUrl}/api/v1/providers/me/status`, {
                headers: {
                    Authorization: `Bearer ${session.accessToken}`,
                    "Content-Type": "application/json",
                },
            });

            if (response.status === 404) {
                return null; // User is not a provider
            }

            if (!response.ok) {
                throw new Error(`Failed to fetch provider status: ${response.statusText}`);
            }

            const json = await response.json() as ApiResponse<ProviderStatusDto>;
            return json.data;
        },
        enabled: !!session?.accessToken,
        retry: (failureCount, error: any) => {
            // Don't retry on 404 or 401/403
            if (error?.message?.includes("404") || error?.message?.includes("401") || error?.message?.includes("403")) {
                return false;
            }
            return failureCount < 2;
        },
        staleTime: 1000 * 60 * 5, // 5 minutes cache
    });
}
