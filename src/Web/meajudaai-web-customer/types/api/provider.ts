// Temporary mock types until OpenAPI generator is setup
export interface ProviderDto {
    id: string;
    name: string;
    email: string;
    avatarUrl?: string | null;
    averageRating: number;
    reviewCount: number;
    services: ServiceDto[];
    city: string;
    state: string;
    description?: string;
    phone?: string;
    providerType: "Individual" | "Company";
}

export interface ServiceDto {
    id: string;
    name: string;
    category: string;
}

export interface ReviewDto {
    id: string;
    providerId: string;
    customerName: string;
    rating: number;
    comment: string;
    createdAt: string;
}
