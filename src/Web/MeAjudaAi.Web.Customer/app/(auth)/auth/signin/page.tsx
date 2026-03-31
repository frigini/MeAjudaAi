import { Suspense } from "react"
import { LoginForm } from "@/components/auth/login-form"
import { auth } from "@/auth"
import { redirect } from "next/navigation"

export const dynamic = "force-dynamic";

export default async function LoginPage() {
    const session = await auth() as { user?: { id?: string } } | null
    if (session?.user) {
        redirect("/")
    }

    return (
        <div className="w-full max-w-[400px] mx-auto px-4">
            {/* Header */}
            <div>
                <h1 className="text-2xl font-bold tracking-tight">
                    Entrar na Me Ajuda Aí
                </h1>
                <p className="text-sm text-muted-foreground mt-1">
                    Use sua conta para continuar
                </p>
            </div>

            {/* Login Form */}
            <Suspense fallback={<div className="text-center text-muted-foreground">Carregando...</div>}>
                <LoginForm />
            </Suspense>
        </div>
    )
}
