export interface ProviderDto {
  id: string;
  name: string;
  slug: string;
  email: string;
  phone?: string | null;
  verificationStatus: string;
  [key: string]: any;
}

export interface UserDto {
  id: string;
  name: string;
  email: string;
  [key: string]: any;
}

export interface ServiceDto {
  id: string;
  name: string;
  categoryId: string;
  [key: string]: any;
}

export interface ReviewDto {
  id: string;
  rating: number;
  text: string;
  reviewerName: string;
  createdAt: string;
  [key: string]: any;
}
