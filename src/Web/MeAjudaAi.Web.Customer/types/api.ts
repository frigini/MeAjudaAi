export interface ApiResponse<T> {
    data: T | null;
    isSuccess: boolean;
    message?: string;
    errors?: string[];
}

export interface ApiError {
    message: string;
    status: number;
    errors?: string[];
}
