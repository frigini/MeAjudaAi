import { useState, useCallback, useRef, useEffect } from "react";

// Use discriminated union for better type safety
export type ViaCepSuccessResponse = {
    erro?: false;
    cep: string;
    logradouro: string;
    complemento: string;
    bairro: string;
    localidade: string;
    uf: string;
};

export type ViaCepResponse =
    | { erro: true }
    | ViaCepSuccessResponse;

const VIACEP_BASE_URL = "https://viacep.com.br/ws";

export function useViaCep() {
    const [isLoading, setIsLoading] = useState(false);
    const [error, setError] = useState<string | null>(null);
    const abortControllerRef = useRef<AbortController | null>(null);

    // Cleanup on unmount
    useEffect(() => {
        return () => {
            if (abortControllerRef.current) {
                abortControllerRef.current.abort();
            }
        };
    }, []);

    const fetchAddress = useCallback(async (cep: string): Promise<ViaCepSuccessResponse | null> => {
        // Cancel previous request immediately
        if (abortControllerRef.current) {
            abortControllerRef.current.abort();
            setIsLoading(false); // Ensure loading is cleared when previous request is aborted
            abortControllerRef.current = null;
        }

        // Clean CEP
        const cleanCep = cep.replace(/\D/g, "");

        // If invalid, clear error and return
        if (cleanCep.length !== 8) {
            setError(null);
            return null;
        }

        const controller = new AbortController();
        abortControllerRef.current = controller;

        setIsLoading(true);
        setError(null);

        try {
            const response = await fetch(`${VIACEP_BASE_URL}/${cleanCep}/json/`, {
                signal: controller.signal
            });

            if (!response.ok) {
                throw new Error("Erro ao buscar CEP");
            }

            const data = (await response.json()) as ViaCepResponse;

            if ('erro' in data && data.erro) {
                setError("CEP não encontrado");
                return null;
            }

            return data as ViaCepSuccessResponse;
        } catch (err: unknown) {
            if (err instanceof DOMException && err.name === 'AbortError') {
                setIsLoading(false); // Clear loading on abort
                return null;
            }

            // Log to console only in dev/test, or use a proper logger
            if (process.env.NODE_ENV !== 'production') {
                console.error(err);
            }

            setError("Falha ao consultar CEP");
            return null;
        } finally {
            if (abortControllerRef.current === controller) {
                setIsLoading(false);
            }
        }
    }, []);

    return { fetchAddress, isLoading, error };
}
