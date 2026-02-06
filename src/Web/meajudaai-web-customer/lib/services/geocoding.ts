import { apiSearchGet } from "@/lib/api/generated";
import { getAuthHeaders } from "@/lib/api/auth-headers";

export interface GeocodingResult {
    latitude: number;
    longitude: number;
    displayName: string;
}

export async function geocodeCity(query: string): Promise<GeocodingResult | null> {
    try {
        const headers = await getAuthHeaders(); // Retrieve auth headers if needed, though search might be public

        // Use apiSearchGet from generated SDK
        const { data } = await apiSearchGet({
            query: {
                query: query
            },
            headers: headers
        });

        if (data && data.length > 0) {
            const firstMatch = data[0];
            if (
                firstMatch &&
                firstMatch.latitude != null &&
                firstMatch.longitude != null
            ) {
                return {
                    latitude: firstMatch.latitude,
                    longitude: firstMatch.longitude,
                    displayName: firstMatch.displayName || query,
                };
            }
        }

        return null;
    } catch (error) {
        console.error("Geocoding failed for query:", query, error);
        return null;
    }
}
