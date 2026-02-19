import { useQuery } from "@tanstack/react-query";
import { useSession } from "next-auth/react";
import { client } from "@/lib/api/client";
import { ProviderDto } from "@/types/api/provider";
import { ApiResponse } from "@/types/api";

export function useMyProviderProfile() {
    const { data: session } = useSession();

    return useQuery({
        queryKey: ["myProviderProfile", session?.user?.id],
        queryFn: async (): Promise<ProviderDto | null> => {
            if (!session?.accessToken) return null;

            const config = client.getConfig();
            const baseUrl = config.baseUrl || process.env.NEXT_PUBLIC_API_URL || "http://localhost:7002";

            const response = await fetch(`${baseUrl}/api/v1/providers/me`, {
                headers: {
                    Authorization: `Bearer ${session.accessToken}`,
                    "Content-Type": "application/json",
                },
            });

            if (response.status === 404) {
                return null;
            }

            if (!response.ok) {
                const error = new Error(`Failed to fetch provider profile: ${response.statusText}`);
                (error as any).status = response.status;
                throw error;
            }

            const json = await response.json() as ApiResponse<ProviderDto>;

            // Handle Result wrapper or direct object
            if (json && typeof json === 'object' && 'value' in json) {
                // @ts-ignore - API sometimes returns Result<T> style
                return json.value;
            }
            // @ts-ignore - Handle direct return or ApiResponse wrapper
            return json.data || json;
        },
        enabled: !!session?.accessToken,
        staleTime: 1000 * 60 * 5, // 5 minutes cache
    });
}
