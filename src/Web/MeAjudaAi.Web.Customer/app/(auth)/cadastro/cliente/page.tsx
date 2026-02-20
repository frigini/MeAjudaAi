import { CustomerRegisterForm } from "@/components/auth/customer-register-form";
import Link from "next/link";
import Image from "next/image";
import { Button } from "@/components/ui/button";

export default function CustomerRegisterPage() {
    return (
        <div className="container relative min-h-screen flex-col items-center justify-center grid lg:max-w-none lg:grid-cols-2 lg:px-0">
            {/* Left Side - Form */}
            <div className="lg:p-8">
                <div className="mx-auto flex w-full flex-col justify-center space-y-6 sm:w-[350px]">
                    <div className="flex flex-col space-y-2 text-center">
                        <h1 className="text-2xl font-semibold tracking-tight">
                            Crie sua conta
                        </h1>
                        <p className="text-sm text-muted-foreground">
                            Preencha seus dados abaixo para começar a contratar serviços
                        </p>
                    </div>

                    <CustomerRegisterForm />

                    <div className="relative">
                        <div className="absolute inset-0 flex items-center">
                            <span className="w-full border-t" />
                        </div>
                        <div className="relative flex justify-center text-xs uppercase">
                            <span className="bg-background px-2 text-muted-foreground">
                                Ou continue com
                            </span>
                        </div>
                    </div>

                    <div className="flex flex-col gap-4">
                        <Button variant="outline" className="w-full" asChild>
                            <Link href="/api/auth/signin">
                                Login com Social / Existente
                            </Link>
                        </Button>
                    </div>

                    <p className="px-8 text-center text-sm text-muted-foreground">
                        Já tem uma conta?{" "}
                        <Link
                            href="/api/auth/signin"
                            className="underline underline-offset-4 hover:text-primary"
                        >
                            Fazer login
                        </Link>
                    </p>
                </div>
            </div>

            {/* Right Side - Image/Hero */}
            <div className="relative hidden h-full flex-col bg-muted p-10 text-white lg:flex dark:border-r">
                <div className="absolute inset-0 bg-primary" />
                <div className="absolute inset-0 bg-[url('/bg-auth.jpg')] bg-cover bg-center opacity-20" /> {/* Placeholder image */}
                <div className="relative z-20 flex items-center text-lg font-medium">
                    <Image
                        src="/logo-white.png"
                        alt="MeAjudaAí"
                        width={140}
                        height={40}
                        className="h-8 w-auto mr-2"
                    />
                    MeAjudaAí
                </div>
                <div className="relative z-20 mt-auto">
                    <blockquote className="space-y-2">
                        <p className="text-lg">
                            &ldquo;Encontrei o profissional perfeito para reformar minha casa em questão de minutos. O MeAjudaAí facilitou muito minha vida!&rdquo;
                        </p>
                        <footer className="text-sm">Sofia Andrade</footer>
                    </blockquote>
                </div>
            </div>
        </div>
    );
}
