import { useMutation, useQueryClient } from "@tanstack/react-query";
import { useSession } from "next-auth/react";
import { client } from "@/lib/api/client";
import { ProviderDto } from "@/types/api/provider"; // Assuming ProviderDto is here or "@/types/provider"
import { ApiResponse } from "@/types/api";

// Needs a request type that matches UpdateProviderProfileRequest structure
interface UpdateProviderProfileRequest {
    name: string;
    businessProfile: {
        legalName: string;
        fantasyName: string | null;
        description: string | null;
        logoUrl?: string | null; // Optional in DTO but might be used
        contactInfo: {
            email: string;
            phoneNumber: string | null;
            website: string | null;
        };
        primaryAddress: {
            street: string;
            number: string;
            complement: string | null;
            neighborhood: string;
            city: string;
            state: string;
            zipCode: string;
            country: string;
        };
    };
}

export function useUpdateProviderProfile() {
    const { data: session } = useSession();
    const queryClient = useQueryClient();

    return useMutation({
        mutationFn: async (data: UpdateProviderProfileRequest): Promise<ProviderDto> => {
            const config = client.getConfig();
            const baseUrl = config.baseUrl || process.env.NEXT_PUBLIC_API_URL || "http://localhost:7002";

            if (!session?.accessToken) {
                throw new Error("Missing access token");
            }

            const response = await fetch(`${baseUrl}/api/v1/providers/me`, {
                method: "PUT", // Endpoint uses PUT
                headers: {
                    Authorization: `Bearer ${session.accessToken}`,
                    "Content-Type": "application/json",
                },
                body: JSON.stringify(data),
            });

            if (!response.ok) {
                const error = await response.json().catch(() => ({}));
                throw new Error(error.message || error.detail || `Failed to update provider profile: ${response.statusText}`);
            }

            const json = await response.json() as ApiResponse<ProviderDto>;

            // Handle Result wrapper or direct object if necessary, usually PUT returns the updated object or Result<T>
            if (json && typeof json === 'object' && 'value' in json) {
                // @ts-ignore
                return json.value;
            }
            // @ts-ignore
            return json.data || json;
        },
        onSuccess: (data) => {
            queryClient.invalidateQueries({ queryKey: ["providerStatus"] });
            queryClient.invalidateQueries({ queryKey: ["myProviderProfile"] });
        }
    });
}
