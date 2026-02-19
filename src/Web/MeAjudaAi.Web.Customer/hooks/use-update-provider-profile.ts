import { useMutation, useQueryClient } from "@tanstack/react-query";
import { useSession } from "next-auth/react";
import { client } from "@/lib/api/client";
import { ProviderDto, ContactInfoDto, AddressDto } from "@/types/api/provider";
import { ApiResponse } from "@/types/api";
import { authenticatedFetch } from "@/lib/api/fetch-client";

// Composed type from shared DTOs
export interface UpdateProviderProfileRequest {
    name: string;
    businessProfile: {
        legalName: string;
        fantasyName?: string | null;
        description?: string | null;
        contactInfo: ContactInfoDto;
        primaryAddress: AddressDto;
    };
}

export function useUpdateProviderProfile() {
    const { data: session } = useSession();
    const queryClient = useQueryClient();

    return useMutation({
        mutationFn: async (data: UpdateProviderProfileRequest): Promise<ProviderDto> => {
            if (!session?.accessToken) {
                throw new Error("Missing access token");
            }

            return await authenticatedFetch<ProviderDto>("/api/v1/providers/me", {
                method: "PUT",
                body: data,
                token: session.accessToken,
            });
        },
        onSuccess: (data) => {
            queryClient.invalidateQueries({ queryKey: ["providerStatus"] });
            queryClient.invalidateQueries({ queryKey: ["myProviderProfile"] });
        }
    });
}
