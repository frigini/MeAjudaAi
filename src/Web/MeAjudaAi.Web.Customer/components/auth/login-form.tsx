"use client"

import { Button } from "@/components/ui/button"
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card"
import { signIn } from "next-auth/react"
import { useSearchParams } from "next/navigation"
import { useState } from "react"
import { Loader2 } from "lucide-react"
import Link from "next/link"

export function LoginForm({
    className,
    ...props
}: React.ComponentPropsWithoutRef<"div">) {
    const searchParams = useSearchParams()
    const callbackUrl = searchParams.get("callbackUrl") || "/"
    const [isLoading, setIsLoading] = useState(false)

    const handleLogin = async () => {
        setIsLoading(true)
        try {
            await signIn("keycloak", { callbackUrl })
        } catch (error) {
            console.error("Login failed", error)
        } finally {
            // Reset loading state if redirect doesn't happen immediately (e.g. error or delay)
            // Note: SignIn usually redirects, but if it fails/returns logic execution continues.
            setIsLoading(false)
        }
    }

    return (
        <div className={className} {...props}>
            <Card>
                <CardHeader className="text-center">
                    <CardTitle className="text-xl">Bem-vindo de volta</CardTitle>
                    <CardDescription>
                        Faça login com sua conta para continuar
                    </CardDescription>
                </CardHeader>
                <CardContent>
                    <div className="grid gap-6">
                        <Button onClick={handleLogin} className="w-full" disabled={isLoading}>
                            {isLoading ? (
                                <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                            ) : null}
                            Entrar com Me Ajuda Aí (Keycloak)
                        </Button>

                        <div className="relative text-center text-sm after:absolute after:inset-0 after:top-1/2 after:z-0 after:flex after:items-center after:border-t after:border-border">
                            {/* Visual separator if we had more options */}
                        </div>

                        <div className="text-center text-sm text-balance text-muted-foreground">
                            Não tem uma conta?{" "}
                            {/* Note: Keycloak registration is handled on the login page usually, but we link to the same flow */}
                            <Button variant="link" className="p-0 h-auto font-normal underline" onClick={handleLogin}>
                                Cadastre-se
                            </Button>
                        </div>
                    </div>
                </CardContent>
            </Card>
            <div className="text-balance text-center text-xs text-muted-foreground [&_a]:underline [&_a]:underline-offset-4 [&_a]:hover:text-primary  ">
                Ao clicar em entrar, você concorda com nossos <span className="underline cursor-not-allowed text-muted-foreground">Termos de Serviço</span>{" "}
                e <span className="underline cursor-not-allowed text-muted-foreground">Política de Privacidade</span>.
            </div>
        </div>
    )
}
