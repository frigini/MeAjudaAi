"use client";

import { useSearchParams } from "next/navigation";
import { signIn } from "next-auth/react";
import { Suspense, useState } from "react";
import { Hammer, AlertCircle } from "lucide-react";
import { Button } from "../../components/ui/button";

function LoginContent() {
  const searchParams = useSearchParams();
  const error = searchParams.get("error");
  const [isLoading, setIsLoading] = useState(false);

  const handleSignIn = async () => {
    try {
      setIsLoading(true);
      await signIn("keycloak", { callbackUrl: "/" });
    } catch (err) {
      console.error(err);
      setIsLoading(false);
    }
  };

  return (
    <div className="flex min-h-screen items-center justify-center bg-gradient-to-br from-background to-muted">
      <div className="w-full max-w-md space-y-8 rounded-xl border border-border bg-surface p-8 shadow-lg">
        <div className="text-center">
          <div className="mx-auto mb-4 flex h-16 w-16 items-center justify-center rounded-full bg-primary">
            <Hammer className="h-8 w-8 text-primary-foreground" />
          </div>
          <h1 className="text-2xl font-bold text-foreground">MeAjudaAí</h1>
          <p className="mt-1 text-sm text-muted-foreground">Portal do Prestador</p>
        </div>

        {error && (
          <div className="flex items-center gap-2 rounded-lg bg-destructive/10 p-3 text-sm text-destructive">
            <AlertCircle className="h-4 w-4" />
            Erro de autenticação. Tente novamente.
          </div>
        )}

        <div className="space-y-4">
          <Button
            onClick={handleSignIn}
            className="w-full"
            size="lg"
            disabled={isLoading}
          >
            {isLoading ? "Redirecionando..." : "Entrar com Keycloak"}
          </Button>
        </div>
      </div>
    </div>
  );
}

export default function LoginPage() {
  return (
    <Suspense fallback={<div>Carregando...</div>}>
      <LoginContent />
    </Suspense>
  );
}
