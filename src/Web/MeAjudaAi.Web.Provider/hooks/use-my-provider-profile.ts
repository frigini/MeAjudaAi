import { useQuery } from "@tanstack/react-query";
import { useSession } from "next-auth/react";
import { ProviderDto } from "@/types/api/provider";
import { authenticatedFetch } from "@/lib/api/fetch-client";

export function useMyProviderProfile() {
    const { data: session } = useSession();

    return useQuery({
        queryKey: ["myProviderProfile", session?.user?.id],
        queryFn: async (): Promise<ProviderDto | null> => {
            if (!session?.accessToken) return null;

            try {
                const result = await authenticatedFetch<ProviderDto>("/api/v1/providers/me", {
                    token: session.accessToken,
                });

                if (result === undefined) {
                    // Treat undefined as no profile found.
                    return null;
                }
                return result;
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
