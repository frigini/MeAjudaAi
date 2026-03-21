export interface ApiResponse<T = unknown> {
    data?: T;
    error?: {
        code: string;
        message: string;
    };
    message?: string;
    status: number;
}

export * from './provider';
