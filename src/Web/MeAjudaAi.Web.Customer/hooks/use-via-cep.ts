import { useState, useCallback, useRef, useEffect } from "react";

// Use discriminated union for better type safety
export type ViaCepResponse =
    | { erro: true }
    | {
        erro?: false;
        cep: string;
        logradouro: string;
        complemento: string;
        bairro: string;
        localidade: string;
        uf: string;
    };

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

    const fetchAddress = useCallback(async (cep: string): Promise<ViaCepResponse | null> => {
        const cleanCep = cep.replace(/\D/g, "");
        if (cleanCep.length !== 8) {
            return null;
        }

        // Cancel previous request
        if (abortControllerRef.current) {
            abortControllerRef.current.abort();
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

            return data;
        } catch (err: unknown) {
            if (err instanceof DOMException && err.name === 'AbortError') {
                return null; // Silent abort
            }
            console.error(err);
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
