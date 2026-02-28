"use client"

import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { signIn } from "next-auth/react"
import { useSearchParams } from "next/navigation"
import { useState } from "react"
import { Loader2, Eye, EyeOff, Mail, Lock } from "lucide-react"
import Link from "next/link"

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

export function LoginForm({
    className,
    ...props
}: React.ComponentPropsWithoutRef<"div">) {
    const searchParams = useSearchParams()
    const rawCallbackUrl = searchParams.get("callbackUrl") || "/"
    const callbackUrl = (rawCallbackUrl.startsWith("/") && !rawCallbackUrl.startsWith("//"))
        ? rawCallbackUrl
        : "/"

    const [email, setEmail] = useState("")
    const [password, setPassword] = useState("")
    const [showPassword, setShowPassword] = useState(false)
    const [isLoading, setIsLoading] = useState(false)
    const [error, setError] = useState("")

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault()
        setError("")
        setIsLoading(true)

        try {
            const result = await signIn("credentials", {
                email,
                password,
                callbackUrl,
                redirect: false,
            })

            if (result?.error) {
                setError("Email ou senha inválidos. Tente novamente.")
            } else if (result?.url) {
                window.location.href = result.url
            }
        } catch {
            setError("Ocorreu um erro. Tente novamente.")
        } finally {
            setIsLoading(false)
        }
    }

    const handleSocialLogin = (provider: string) => {
        signIn("keycloak", { callbackUrl }, { kc_idp_hint: provider })
    }

    return (
        <div className={className} {...props}>
            <form onSubmit={handleSubmit} className="space-y-3.5 mt-6">
                {/* Email */}
                <div className="relative">
                    <Mail className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-muted-foreground" />
                    <Input
                        type="email"
                        placeholder="Insira seu e-mail"
                        value={email}
                        onChange={(e) => setEmail(e.target.value)}
                        className="pl-10 shadow-sm"
                        autoComplete="email"
                        required
                    />
                </div>

                {/* Password */}
                <div className="relative">
                    <Lock className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-muted-foreground" />
                    <Input
                        type={showPassword ? "text" : "password"}
                        placeholder="Insira sua senha"
                        value={password}
                        onChange={(e) => setPassword(e.target.value)}
                        className="pl-10 pr-10 shadow-sm"
                        autoComplete="current-password"
                        required
                    />
                    <Button
                        type="button"
                        variant="ghost"
                        size="sm"
                        className="absolute right-0 top-0 h-full px-3 py-2 hover:bg-transparent"
                        onClick={() => setShowPassword(!showPassword)}
                        aria-label={showPassword ? "Ocultar senha" : "Mostrar senha"}
                    >
                        {showPassword ? (
                            <EyeOff className="h-4 w-4 text-muted-foreground" />
                        ) : (
                            <Eye className="h-4 w-4 text-muted-foreground" />
                        )}
                    </Button>
                </div>

                {/* Error message */}
                {error && (
                    <p className="text-sm text-red-600 text-center">{error}</p>
                )}

                {/* Forgot password */}
                <div className="text-center pt-1">
                    <Link
                        href={`${process.env.NEXT_PUBLIC_KEYCLOAK_URL || "http://localhost:8080"}/realms/meajudaai/login-actions/reset-credentials?client_id=customer-app`}
                        className="text-sm font-medium underline underline-offset-4 hover:text-primary"
                    >
                        Esqueci minha senha
                    </Link>
                </div>

                {/* Submit */}
                <Button type="submit" className="w-full shadow-sm mt-2" size="lg" disabled={isLoading}>
                    {isLoading ? (
                        <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                    ) : null}
                    Entrar
                </Button>
            </form>

            <div className="space-y-5 mt-5">
                {/* Create Account Link */}
                <div className="text-center">
                    <Link
                        href="/cadastro/cliente"
                        className="text-sm font-semibold underline underline-offset-4 hover:text-primary"
                    >
                        Crie sua conta grátis
                    </Link>
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
                <div className="flex flex-col gap-2.5">
                    <Button
                        variant="outline"
                        className="w-full shadow-sm"
                        onClick={() => handleSocialLogin("google")}
                    >
                        <GoogleIcon className="mr-2 h-5 w-5" />
                        Entrar com o Google
                    </Button>
                    <Button
                        variant="outline"
                        className="w-full shadow-sm"
                        onClick={() => handleSocialLogin("facebook")}
                    >
                        <FacebookIcon className="mr-2 h-5 w-5" />
                        Entrar com o Facebook
                    </Button>
                </div>

                {/* Terms footer */}
                <p className="text-center text-xs text-muted-foreground pt-2">
                    Ao clicar em entrar, você concorda com nossos{" "}
                    <Link href="/termos" className="underline hover:text-primary">Termos de Serviço</Link>{" "}
                    e{" "}
                    <Link href="/privacidade" className="underline hover:text-primary">Política de Privacidade</Link>.
                </p>
            </div>
        </div>
    )
}
