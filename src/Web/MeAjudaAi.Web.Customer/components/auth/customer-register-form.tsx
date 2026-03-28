"use client";

import { useState, useRef, useEffect } from "react";
import { useForm } from "react-hook-form";
import { Loader2, Eye, EyeOff } from "lucide-react";
import { useRouter } from "next/navigation";
import { toast } from "sonner";
import { Checkbox } from "@/components/ui/checkbox";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { type RegisterCustomerInput } from "@/lib/schemas/auth";
import { publicFetch, ApiError } from "@/lib/api/fetch-client";
import Link from "next/link";
import { cn } from "@/lib/utils";

function maskPhone(value: string) {
    const numbers = value.replace(/\D/g, "");
    if (numbers.length === 10) {
        return numbers
            .replace(/(\d{2})(\d{4})(\d{4})/, "($1) $2-$3");
    }
    if (numbers.length === 11) {
        return numbers
            .replace(/(\d{2})(\d{5})(\d{4})/, "($1) $2-$3");
    }
    if (numbers.length > 11) {
        return numbers
            .replace(/(\d{2})(\d{4})(\d)/, "($1) $2")
            .replace(/(\d{4})\d+?$/, "$1");
    }
    return numbers
        .replace(/(\d{2})(\d)/, "($1) $2")
        .replace(/(\d{5})(\d)/, "$1-$2")
        .replace(/(-\d{4})\d+?$/, "$1");
}

export function CustomerRegisterForm() {
    const router = useRouter();
    const [isLoading, setIsLoading] = useState(false);
    const [showPassword, setShowPassword] = useState(false);
    const [showConfirmPassword, setShowConfirmPassword] = useState(false);
    const timerRef = useRef<NodeJS.Timeout | null>(null);

    useEffect(() => {
        return () => {
            if (timerRef.current) clearTimeout(timerRef.current);
        };
    }, []);

    const form = useForm<RegisterCustomerInput>({
        // NO resolver here to avoid JSDOM crashes
        defaultValues: {
            name: "",
            email: "",
            phoneNumber: "",
            password: "",
            confirmPassword: "",
            acceptedTerms: false,
        },
    });

    const { register, handleSubmit, setValue, formState: { errors }, watch, setError, clearErrors } = form;
    const acceptedTerms = watch("acceptedTerms");

    async function onSubmit(data: RegisterCustomerInput) {
        // Manual validation for CI stability
        clearErrors();
        let hasError = false;
        let payload: Record<string, unknown> = {};

        const trimmedName = data.name?.trim() || "";
        if (!trimmedName || trimmedName.length < 4) {
            setError("name", { message: "Nome deve ter pelo menos 4 caracteres" });
            hasError = true;
        }

        if (!data.email || !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(data.email)) {
            setError("email", { message: "Email inválido" });
            hasError = true;
        }

        const phoneDigits = (data.phoneNumber?.replace(/\D/g, "") || "").slice(0, 11);
        if (!phoneDigits || phoneDigits.length < 10 || phoneDigits.length > 11) {
            setError("phoneNumber", { message: "Telefone inválido (10-11 dígitos)" });
            hasError = true;
        }

        if (!data.password || data.password.length < 8) {
            setError("password", { message: "Senha deve ter pelo menos 8 caracteres" });
            hasError = true;
        }

        if (data.password !== data.confirmPassword) {
            setError("confirmPassword", { message: "As senhas não coincidem" });
            hasError = true;
        }

        if (!data.acceptedTerms) {
            setError("acceptedTerms", { message: "Você deve aceitar os termos de uso" });
            hasError = true;
        }

        if (hasError) return;

        setIsLoading(true);
        try {
            payload = {
                name: data.name?.trim(),
                email: data.email?.trim(),
                phoneNumber: phoneDigits,
                password: data.password,
                termsAccepted: data.acceptedTerms,
                acceptedPrivacyPolicy: data.acceptedTerms,
            };
            await publicFetch("/api/v1/users/register", {
                method: "post",
                body: JSON.stringify(payload),
            });

            toast.success("Conta criada com sucesso!", {
                description: "Você será redirecionado para o login.",
            });

            timerRef.current = setTimeout(() => {
                router.push("/auth/login");
                setIsLoading(false);
            }, 2000);
            return;

        } catch (error) {
            setIsLoading(false);
            if (error instanceof ApiError) {
                toast.error(error.message);
                const msg = error.message.toLowerCase().replace(/[-\s]/g, '');
                if (msg.includes('email') || msg.includes('e-mail') || msg.includes('mail')) {
                    setError("email", { message: error.message });
                }
            } else {
                toast.error("Ocorreu um erro ao criar sua conta. Tente novamente.");
            }
        }
    }

    return (
        <div className="grid gap-6">
            <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
                <div className="grid gap-4">
                    <div className="space-y-2">
                        <label htmlFor="name" className="text-sm font-medium leading-none peer-disabled:cursor-not-allowed peer-disabled:opacity-70">Nome Completo</label>
                        <Input 
                            id="name" 
                            placeholder="Seu nome" 
                            aria-invalid={!!errors.name}
                            aria-describedby="name-error"
                            {...register("name")}
                        />
                        {errors.name && <p id="name-error" className="text-sm font-medium text-destructive" role="alert">{errors.name.message}</p>}
                    </div>

                    <div className="space-y-2">
                        <label htmlFor="email" className="text-sm font-medium leading-none peer-disabled:cursor-not-allowed peer-disabled:opacity-70">Email</label>
                        <Input
                            id="email"
                            placeholder="exemplo@email.com"
                            type="email"
                            aria-invalid={!!errors.email}
                            aria-describedby="email-error"
                            {...register("email")}
                        />
                        {errors.email && <p id="email-error" className="text-sm font-medium text-destructive" role="alert">{errors.email.message}</p>}
                    </div>

                    <div className="space-y-2">
                        <label htmlFor="phoneNumber" className="text-sm font-medium leading-none peer-disabled:cursor-not-allowed peer-disabled:opacity-70">Celular</label>
                        <Input
                            id="phoneNumber"
                            placeholder="(00) 00000-0000"
                            aria-invalid={!!errors.phoneNumber}
                            aria-describedby="phoneNumber-error"
                            {...register("phoneNumber")}
                            onChange={(e) => {
                                const masked = maskPhone(e.target.value);
                                setValue("phoneNumber", masked);
                            }}
                        />
                        {errors.phoneNumber && <p id="phoneNumber-error" className="text-sm font-medium text-destructive" role="alert">{errors.phoneNumber.message}</p>}
                    </div>

                    <div className="space-y-2">
                        <div className="flex items-center justify-between">
                            <label htmlFor="password" className="text-sm font-medium leading-none peer-disabled:cursor-not-allowed peer-disabled:opacity-70">Senha</label>
                        </div>
                        <div className="relative">
                            <Input
                                id="password"
                                type={showPassword ? "text" : "password"}
                                aria-invalid={!!errors.password}
                                aria-describedby="password-error"
                                {...register("password")}
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
                                    <EyeOff className="h-4 w-4" />
                                ) : (
                                    <Eye className="h-4 w-4" />
                                )}
                            </Button>
                        </div>
                        {errors.password && <p id="password-error" className="text-sm font-medium text-destructive" role="alert">{errors.password.message}</p>}
                    </div>

                    <div className="space-y-2">
                        <label htmlFor="confirmPassword" className="text-sm font-medium leading-none peer-disabled:cursor-not-allowed peer-disabled:opacity-70">Confirmar Senha</label>
                        <div className="relative">
                            <Input
                                id="confirmPassword"
                                type={showConfirmPassword ? "text" : "password"}
                                aria-invalid={!!errors.confirmPassword}
                                aria-describedby="confirmPassword-error"
                                {...register("confirmPassword")}
                            />
                            <Button
                                type="button"
                                variant="ghost"
                                size="sm"
                                className="absolute right-0 top-0 h-full px-3 py-2 hover:bg-transparent"
                                onClick={() => setShowConfirmPassword(!showConfirmPassword)}
                                aria-label={showConfirmPassword ? "Ocultar senha" : "Mostrar senha"}
                            >
                                {showConfirmPassword ? (
                                    <EyeOff className="h-4 w-4" />
                                ) : (
                                    <Eye className="h-4 w-4" />
                                )}
                            </Button>
                        </div>
                        {errors.confirmPassword && <p id="confirmPassword-error" className="text-sm font-medium text-destructive" role="alert">{errors.confirmPassword.message}</p>}
                    </div>
                </div>

                <div className="flex flex-row items-start space-x-3 space-y-0 p-1">
                    <Checkbox
                        id="acceptedTerms"
                        checked={acceptedTerms}
                        aria-invalid={!!errors.acceptedTerms}
                        aria-describedby="acceptedTerms-error"
                        onCheckedChange={(checked) => setValue("acceptedTerms", checked === true)}
                    />
                    <div className="space-y-1 leading-none">
                        <label htmlFor="acceptedTerms" className={cn("text-sm font-normal", errors.acceptedTerms && "text-destructive")}>
                            Eu aceito os{" "}
                            <a href="/termos" className="text-primary hover:underline" target="_blank">
                                termos de uso
                            </a>{" "}
                            e{" "}
                            <a href="/privacidade" className="text-primary hover:underline" target="_blank">
                                política de privacidade
                            </a>
                        </label>
                        {errors.acceptedTerms && <p id="acceptedTerms-error" className="text-sm font-medium text-destructive" role="alert">{errors.acceptedTerms.message}</p>}
                    </div>
                </div>

                <Button type="submit" className="w-full" disabled={isLoading}>
                    {isLoading && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
                    Criar Conta
                </Button>

                <div className="text-center text-sm">
                    Já tem uma conta?{" "}
                    <Link href="/auth/login" className="underline underline-offset-4">
                        Faça login
                    </Link>
                </div>
            </form>
        </div>
    );
}
