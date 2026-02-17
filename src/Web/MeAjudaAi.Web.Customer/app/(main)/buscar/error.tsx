'use client';

import { useEffect } from 'react';
import { useRouter } from 'next/navigation';
import { Button } from '@/components/ui/button';
import { Search } from 'lucide-react';

export default function SearchError({
    error,
    reset,
}: {
    error: Error & { digest?: string };
    reset: () => void;
}) {
    const router = useRouter();

    useEffect(() => {
        // Log the error to an error reporting service
        // TODO: Enviar erro para um serviço de observabilidade (ex: Sentry)
        console.error(error);
    }, [error]);

    return (
        <div className="container mx-auto px-4 py-16">
            <div className="flex flex-col items-center justify-center text-center">
                <Search className="mb-4 h-16 w-16 text-foreground-subtle" />
                <h2 className="mb-2 text-2xl font-bold text-foreground">
                    Erro ao buscar prestadores
                </h2>
                <p className="mb-6 max-w-md text-foreground-subtle">
                    Não foi possível carregar os resultados da busca. Verifique sua
                    conexão e tente novamente.
                </p>
                <div className="flex gap-4">
                    <Button onClick={() => reset()}>Tentar novamente</Button>
                    <Button variant="outline" onClick={() => router.push('/')}>
                        Voltar para home
                    </Button>
                </div>
            </div>
        </div>
    );
}
