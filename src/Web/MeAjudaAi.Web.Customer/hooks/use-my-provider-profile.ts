import { useQuery } from "@tanstack/react-query";
import { useSession } from "next-auth/react";
import { client } from "@/lib/api/client";
import { ProviderDto } from "@/types/api/provider";
import { ApiResponse } from "@/types/api";
import { authenticatedFetch } from "@/lib/api/fetch-client";

export function useMyProviderProfile() {
    const { data: session } = useSession();

    return useQuery({
        queryKey: ["myProviderProfile", session?.user?.id],
        queryFn: async (): Promise<ProviderDto | null> => {
            if (!session?.accessToken) return null;

            try {
                return await authenticatedFetch<ProviderDto>("/api/v1/providers/me", {
                    token: session.accessToken,
                });
            } catch (error: any) {
                if (error.status === 404) {
                    return null;
                }
                throw error;
            }
        },
        enabled: !!session?.accessToken,
        staleTime: 1000 * 60 * 5, // 5 minutes cache
    });
}
