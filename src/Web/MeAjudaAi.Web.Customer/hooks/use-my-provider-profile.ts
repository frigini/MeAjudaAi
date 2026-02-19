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
                const result = await authenticatedFetch<ProviderDto>("/api/v1/providers/me", {
                    token: session.accessToken,
                });

                if (result === undefined) {
                    // authenticatedFetch returns undefined if the response status is 204 No Content
                    // or if the response body is empty/unparseable for a successful status.
                    // For a profile fetch, this might indicate no profile found,
                    // or an unexpected empty response.
                    // Given the original code returned `null` for 404,
                    // we can treat an undefined result similarly if it implies no profile.
                    // However, the instruction specifically asks to "handle `undefined` return"
                    // and the example shows throwing an error for registration.
                    // For a GET request, an undefined result for a successful status
                    // is usually an unexpected scenario if a DTO is expected.
                    // Let's assume `undefined` here means no profile found, similar to 404.
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
