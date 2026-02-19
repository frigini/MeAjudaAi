import { useMutation, useQueryClient } from "@tanstack/react-query";
import { useSession } from "next-auth/react";
import { client } from "@/lib/api/client";
import { RegisterProviderRequest, ProviderDto } from "@/types/provider";
import { ApiResponse } from "@/types/api";

export function useRegisterProvider() {
    const { data: session } = useSession();
    const queryClient = useQueryClient();

    return useMutation({
        mutationFn: async (data: RegisterProviderRequest): Promise<ProviderDto> => {
            const config = client.getConfig();
            const baseUrl = config.baseUrl || process.env.NEXT_PUBLIC_API_URL || "http://localhost:7002";

            if (!session?.accessToken) {
                throw new Error("Missing access token");
            }

            const response = await fetch(`${baseUrl}/api/v1/providers/register`, {
                method: "POST",
                headers: {
                    Authorization: `Bearer ${session.accessToken}`,
                    "Content-Type": "application/json",
                },
                body: JSON.stringify(data),
            });

            if (!response.ok) {
                const error = await response.json().catch(() => ({}));
                throw new Error(error.message || `Failed to register provider: ${response.statusText}`);
            }

            const json = await response.json() as ApiResponse<ProviderDto>;

            if (!json.data) {
                throw new Error(json.message || "Failed to register provider (no data returned)");
            }

            return json.data;
        },
        onSuccess: (data) => {
            queryClient.invalidateQueries({ queryKey: ["providerStatus"] });
        }
    });
}
