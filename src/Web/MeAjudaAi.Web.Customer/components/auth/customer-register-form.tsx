"use client";

import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";

import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Checkbox } from "@/components/ui/checkbox";
import {
    Form,
    FormControl,
    FormField,
    FormItem,
    FormLabel,
    FormMessage,
} from "@/components/ui/form";
import { registerCustomerSchema, RegisterCustomerSchema } from "@/lib/schemas/auth";
import { useState, useEffect, useRef } from "react";
import { useRouter } from "next/navigation";
import { ApiError, publicFetch } from "@/lib/api/fetch-client";
import { toast } from "sonner";
import Link from "next/link";
import { Eye, EyeOff, Loader2 } from "lucide-react";

export function CustomerRegisterForm() {
    const router = useRouter();
    const [isLoading, setIsLoading] = useState(false);
    const [showPassword, setShowPassword] = useState(false);
    const [showConfirmPassword, setShowConfirmPassword] = useState(false);
    const timerRef = useRef<NodeJS.Timeout | null>(null);

    useEffect(() => {
        return () => {
            if (timerRef.current) {
                clearTimeout(timerRef.current);
            }
        };
    }, []);

    const form = useForm<RegisterCustomerSchema>({
        resolver: zodResolver(registerCustomerSchema),
        defaultValues: {
            name: "",
            email: "",
            phoneNumber: "",
            password: "",
            confirmPassword: "",
            termsAccepted: false,
        },
    });

    async function onSubmit(data: RegisterCustomerSchema) {
        setIsLoading(true);
        try {
            // Call API
            await publicFetch("/api/v1/users/register", {
                method: "POST",
                body: {
                    name: data.name,
                    email: data.email,
                    phoneNumber: data.phoneNumber.replace(/\D/g, ""),
                    password: data.password,
                    termsAccepted: data.termsAccepted,
                },
            });

            toast.success("Conta criada com sucesso!", {
                description: "Redirecionando para login...",
            });

            // Delay redirect to allow toast to be visible
            timerRef.current = setTimeout(() => {
                router.replace("/api/auth/signin");
            }, 1000);

        } catch (error) {
            console.error("Erro ao criar conta:", error);
            const message = error instanceof ApiError
                ? error.message
                : "Não foi possível criar sua conta. Tente novamente.";

            toast.error("Erro no cadastro", {
                description: message,
            });
            setIsLoading(false);
        }
    }

    return (
        <Form {...form}>
            <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-4">
                <FormField
                    control={form.control}
                    name="name"
                    render={({ field }) => (
                        <FormItem>
                            <FormLabel>Nome Completo</FormLabel>
                            <FormControl>
                                <Input placeholder="Seu nome" {...field} />
                            </FormControl>
                            <FormMessage />
                        </FormItem>
                    )}
                />

                <FormField
                    control={form.control}
                    name="email"
                    render={({ field }) => (
                        <FormItem>
                            <FormLabel>Email</FormLabel>
                            <FormControl>
                                <Input placeholder="seu@email.com" type="email" {...field} />
                            </FormControl>
                            <FormMessage />
                        </FormItem>
                    )}
                />

                <FormField
                    control={form.control}
                    name="phoneNumber"
                    render={({ field }) => (
                        <FormItem>
                            <FormLabel>Celular (com DDD)</FormLabel>
                            <FormControl>
                                <Input
                                    type="tel"
                                    inputMode="numeric"
                                    placeholder="(00) 00000-0000"
                                    {...field}
                                    onChange={(e) => {
                                        // Simple mask
                                        let v = e.target.value.replace(/\D/g, "");
                                        v = v.replace(/^(\d\d)(\d)/g, "($1) $2");
                                        v = v.replace(/(\d{5})(\d)/, "$1-$2");
                                        field.onChange(v.substring(0, 15));
                                    }}
                                />
                            </FormControl>
                            <FormMessage />
                        </FormItem>
                    )}
                />

                <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                    <FormField
                        control={form.control}
                        name="password"
                        render={({ field }) => (
                            <FormItem>
                                <FormLabel>Senha</FormLabel>
                                <FormControl>
                                    <div className="relative">
                                        <Input
                                            type={showPassword ? "text" : "password"}
                                            placeholder="******"
                                            {...field}
                                        />
                                        <Button
                                            type="button"
                                            variant="ghost"
                                            size="sm"
                                            className="absolute right-0 top-0 h-full px-3 py-2 hover:bg-transparent"
                                            onClick={() => setShowPassword(!showPassword)}
                                            aria-label={showPassword ? "Ocultar senha" : "Mostrar senha"}
                                            title={showPassword ? "Ocultar senha" : "Mostrar senha"}
                                        >
                                            {showPassword ? (
                                                <EyeOff className="h-4 w-4 text-muted-foreground" aria-hidden="true" />
                                            ) : (
                                                <Eye className="h-4 w-4 text-muted-foreground" aria-hidden="true" />
                                            )}
                                        </Button>
                                    </div>
                                </FormControl>
                                <FormMessage />
                            </FormItem>
                        )}
                    />

                    <FormField
                        control={form.control}
                        name="confirmPassword"
                        render={({ field }) => (
                            <FormItem>
                                <FormLabel>Confirmar Senha</FormLabel>
                                <FormControl>
                                    <div className="relative">
                                        <Input
                                            type={showConfirmPassword ? "text" : "password"}
                                            placeholder="******"
                                            {...field}
                                        />
                                        <Button
                                            type="button"
                                            variant="ghost"
                                            size="sm"
                                            className="absolute right-0 top-0 h-full px-3 py-2 hover:bg-transparent"
                                            onClick={() => setShowConfirmPassword(!showConfirmPassword)}
                                            aria-label={showConfirmPassword ? "Ocultar confirmação de senha" : "Mostrar confirmação de senha"}
                                            title={showConfirmPassword ? "Ocultar confirmação de senha" : "Mostrar confirmação de senha"}
                                        >
                                            {showConfirmPassword ? (
                                                <EyeOff className="h-4 w-4 text-muted-foreground" aria-hidden="true" />
                                            ) : (
                                                <Eye className="h-4 w-4 text-muted-foreground" aria-hidden="true" />
                                            )}
                                        </Button>
                                    </div>
                                </FormControl>
                                <FormMessage />
                            </FormItem>
                        )}
                    />
                </div>

                <FormField
                    control={form.control}
                    name="termsAccepted"
                    render={({ field }) => (
                        <FormItem className="flex flex-row items-start space-x-3 space-y-0 p-2">
                            <FormControl>
                                <Checkbox
                                    checked={field.value}
                                    onCheckedChange={field.onChange}
                                />
                            </FormControl>
                            <div className="space-y-1 leading-none">
                                <FormLabel>
                                    Li e aceito os <Link href="/termos" className="text-primary hover:underline">Termos de Uso</Link> e <Link href="/privacidade" className="text-primary hover:underline">Política de Privacidade</Link>
                                </FormLabel>
                                <FormMessage />
                            </div>
                        </FormItem>
                    )}
                />

                <Button type="submit" className="w-full" disabled={isLoading}>
                    {isLoading && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
                    Criar conta
                </Button>
            </form>
        </Form>
    );
}
