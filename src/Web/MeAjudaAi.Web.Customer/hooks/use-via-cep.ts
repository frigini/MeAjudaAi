import { useState } from "react";

interface ViaCepResponse {
    cep: string;
    logradouro: string;
    complemento: string;
    bairro: string;
    localidade: string;
    uf: string;
    erro?: boolean;
}

export function useViaCep() {
    const [isLoading, setIsLoading] = useState(false);
    const [error, setError] = useState<string | null>(null);

    const fetchAddress = async (cep: string): Promise<ViaCepResponse | null> => {
        // Clean CEP
        const cleanCep = cep.replace(/\D/g, "");
        if (cleanCep.length !== 8) {
            return null;
        }

        setIsLoading(true);
        setError(null);

        try {
            const response = await fetch(`https://viacep.com.br/ws/${cleanCep}/json/`);
            if (!response.ok) {
                throw new Error("Erro ao buscar CEP");
            }

            const data: ViaCepResponse = await response.json();

            if (data.erro) {
                setError("CEP não encontrado");
                return null;
            }

            return data;
        } catch (err) {
            setError("Falha ao consultar CEP");
            return null;
        } finally {
            setIsLoading(false);
        }
    };

    return { fetchAddress, isLoading, error };
}
