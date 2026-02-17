import { apiServicesGet } from "@/lib/api/generated/sdk.gen";
import { MeAjudaAiModulesServiceCatalogsApplicationDtosServiceListDto } from "@/lib/api/generated/types.gen";

export type ServiceListDto = MeAjudaAiModulesServiceCatalogsApplicationDtosServiceListDto;

export async function getPopularServices(): Promise<ServiceListDto[]> {
    try {
        const { data } = await apiServicesGet({
            query: {
                activeOnly: true
            }
        });

        // The API returns a wrapper, data.data contains the list
        return data?.data || [];
    } catch (error) {
        console.error("Failed to fetch services:", error);
        return [];
    }
}
