"use client";

import { CustomerRegisterForm } from "@/components/auth/customer-register-form";
import Link from "next/link";
import { Button } from "@/components/ui/button";
import { signIn } from "next-auth/react";
import { ShieldCheck, Info } from "lucide-react";
import { useState } from "react";

function GoogleIcon({ className }: { className?: string }) {
    return (
        <svg className={className} viewBox="0 0 24 24" aria-hidden="true">
            <path d="M22.56 12.25c0-.78-.07-1.53-.2-2.25H12v4.26h5.92a5.06 5.06 0 0 1-2.2 3.32v2.77h3.57c2.08-1.92 3.28-4.74 3.28-8.1z" fill="#4285F4" />
            <path d="M12 23c2.97 0 5.46-.98 7.28-2.66l-3.57-2.77c-.98.66-2.23 1.06-3.71 1.06-2.86 0-5.29-1.93-6.16-4.53H2.18v2.84C3.99 20.53 7.7 23 12 23z" fill="#34A853" />
            <path d="M5.84 14.09c-.22-.66-.35-1.36-.35-2.09s.13-1.43.35-2.09V7.07H2.18A10.96 10.96 0 0 0 1 12c0 1.78.42 3.46 1.18 4.93l3.66-2.84z" fill="#FBBC05" />
            <path d="M12 5.38c1.62 0 3.06.56 4.21 1.64l3.15-3.15C17.45 2.09 14.97 1 12 1 7.7 1 3.99 3.47 2.18 7.07l3.66 2.84c.87-2.6 3.3-4.53 6.16-4.53z" fill="#EA4335" />
        </svg>
    );
}

function FacebookIcon({ className }: { className?: string }) {
    return (
        <svg className={className} viewBox="0 0 24 24" aria-hidden="true">
            <path d="M24 12.073c0-6.627-5.373-12-12-12s-12 5.373-12 12c0 5.99 4.388 10.954 10.125 11.854v-8.385H7.078v-3.47h3.047V9.43c0-3.007 1.792-4.669 4.533-4.669 1.312 0 2.686.235 2.686.235v2.953H15.83c-1.491 0-1.956.925-1.956 1.874v2.25h3.328l-.532 3.47h-2.796v8.385C19.612 23.027 24 18.062 24 12.073z" fill="#1877F2" />
        </svg>
    );
}

export default function CustomerRegisterPage() {
    const [showPrivacyInfo, setShowPrivacyInfo] = useState(false);

    const handleSocialLogin = (provider: string) => {
        signIn("keycloak", { callbackUrl: "/" }, { kc_idp_hint: provider });
    };

    return (
        <div className="w-full max-w-md mx-auto space-y-6 px-4 py-8">
            {/* Header */}
            <div>
                <h1 className="text-2xl font-bold tracking-tight">
                    Crie sua conta grátis de forma rápida e segura
                </h1>
            </div>

            {/* Registration Form */}
            <CustomerRegisterForm />

            {/* Privacy & Security Badge */}
            <div className="relative">
                <button
                    type="button"
                    onClick={() => setShowPrivacyInfo(!showPrivacyInfo)}
                    className="w-full flex items-center gap-3 rounded-lg border border-green-200 bg-green-50 px-4 py-3 text-left transition-colors hover:bg-green-100"
                >
                    <ShieldCheck className="h-5 w-5 text-green-600 shrink-0" />
                    <span className="text-sm font-medium text-green-800 flex-1">Privacidade e segurança</span>
                    <Info className="h-4 w-4 text-green-600 shrink-0" />
                </button>
                {showPrivacyInfo && (
                    <div className="absolute z-10 mt-2 w-full rounded-lg border border-green-200 bg-white p-4 shadow-lg">
                        <div className="flex items-start gap-3">
                            <ShieldCheck className="h-5 w-5 text-green-600 shrink-0 mt-0.5" />
                            <div className="flex-1">
                                <p className="text-sm text-foreground">
                                    Seu número é verificado, mas permanece privado.{" "}
                                    <strong>Nenhum outro usuário</strong> poderá ver seu telefone.
                                </p>
                            </div>
                            <button
                                type="button"
                                onClick={() => setShowPrivacyInfo(false)}
                                className="text-muted-foreground hover:text-foreground text-lg leading-none"
                            >
                                ×
                            </button>
                        </div>
                    </div>
                )}
            </div>

            {/* Divider */}
            <div className="relative">
                <div className="absolute inset-0 flex items-center">
                    <span className="w-full border-t" />
                </div>
                <div className="relative flex justify-center text-xs uppercase">
                    <span className="bg-surface-raised px-2 text-muted-foreground">ou</span>
                </div>
            </div>

            {/* Social Login */}
            <div className="flex flex-col gap-3">
                <Button
                    variant="outline"
                    className="w-full"
                    onClick={() => handleSocialLogin("google")}
                >
                    <GoogleIcon className="mr-2 h-5 w-5" />
                    Criar conta com Google
                </Button>
                <Button
                    variant="outline"
                    className="w-full"
                    onClick={() => handleSocialLogin("facebook")}
                >
                    <FacebookIcon className="mr-2 h-5 w-5" />
                    Criar conta com Facebook
                </Button>
            </div>

            {/* Already have an account */}
            <div className="text-center">
                <Link
                    href="/auth/signin"
                    className="text-sm font-medium underline underline-offset-4 hover:text-primary"
                >
                    Já tenho uma conta
                </Link>
            </div>

            {/* Passive consent footer */}
            <p className="text-center text-xs text-muted-foreground">
                Ao criar sua conta, você concorda com nossos{" "}
                <Link href="/termos" className="underline hover:text-primary">Termos de Uso</Link>{" "}
                e{" "}
                <Link href="/privacidade" className="underline hover:text-primary">Política de Privacidade</Link>.
            </p>
        </div>
    );
}
