import { useQuery } from "@tanstack/react-query";
import { apiServicesGet } from "@/lib/api/generated/sdk.gen";
import { MeAjudaAiModulesServiceCatalogsApplicationDtosServiceListDto } from "@/lib/api/generated/types.gen";

export type ServiceListDto = MeAjudaAiModulesServiceCatalogsApplicationDtosServiceListDto;

export const SERVICES_STALE_TIME = 1000 * 60 * 60; // 1 hour

export function useServices() {
    return useQuery({
        queryKey: ["services"],
        queryFn: async () => {
            const { data } = await apiServicesGet({
                query: {
                    activeOnly: true
                },
                throwOnError: true
            });
            // The API returns a wrapper, data.data contains the list
            return (data?.data || []) as ServiceListDto[];
        },
        staleTime: SERVICES_STALE_TIME,
        gcTime: SERVICES_STALE_TIME,
    });
}
