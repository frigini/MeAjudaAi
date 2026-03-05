import { useMutation, useQueryClient } from "@tanstack/react-query";
import { useSession } from "next-auth/react";
import { RegisterProviderRequest, ProviderDto } from "@/types/provider";
import { authenticatedFetch } from "@/lib/api/fetch-client";

export function useRegisterProvider() {
    const { data: session } = useSession();
    const queryClient = useQueryClient();

    return useMutation({
        mutationFn: async (data: RegisterProviderRequest): Promise<ProviderDto> => {
            if (!session?.accessToken) {
                throw new Error("Missing access token");
            }

            // Backend derives email from token and expects specific field names
            const payload = {
                name: data.name,
                type: data.type,
                documentNumber: data.documentNumber,
                phoneNumber: data.phoneNumber,
            };

            const result = await authenticatedFetch<ProviderDto>("/api/v1/providers/become", {
                method: "post",
                body: payload,
                token: session.accessToken,
            });

            if (!result) throw new Error("Failed to register provider");
            return result;
        },
        onSuccess: (data) => {
            queryClient.invalidateQueries({ queryKey: ["providerStatus"] });
            queryClient.invalidateQueries({ queryKey: ["myProviderProfile"] });
        }
    });
}
