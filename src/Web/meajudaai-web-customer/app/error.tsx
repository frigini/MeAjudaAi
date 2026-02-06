'use client';

import { useEffect } from 'react';
import { Button } from '@/components/ui/button';

export default function CustomerError({
    error,
    reset,
}: {
    error: Error & { digest?: string };
    reset: () => void;
}) {
    useEffect(() => {
        console.error(error);
    }, [error]);

    return (
        <div className="flex min-h-[400px] flex-col items-center justify-center px-4">
            <div className="text-center">
                <h2 className="mb-4 text-2xl font-bold text-foreground">
                    Algo deu errado!
                </h2>
                <p className="mb-6 text-foreground-subtle">
                    Não foi possível carregar os dados. Por favor, tente novamente.
                </p>
                <Button onClick={() => reset()}>Tentar novamente</Button>
            </div>
        </div>
    );
}
