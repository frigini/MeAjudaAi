export interface ProviderDto {
  id: string;
  name: string;
  slug: string;
  email: string;
  phone?: string | null;
  verificationStatus: string;
}

export interface UserDto {
  id: string;
  name: string;
  email: string;
}

export interface ServiceDto {
  id: string;
  name: string;
  categoryId: string;
}

export interface ReviewDto {
  id: string;
  rating: number;
  text: string;
  reviewerName: string;
  createdAt: string;
}
