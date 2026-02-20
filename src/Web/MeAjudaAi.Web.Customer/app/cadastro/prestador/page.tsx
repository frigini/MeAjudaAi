"use client";

import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import * as z from "zod";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Form, FormControl, FormField, FormItem, FormLabel, FormMessage } from "@/components/ui/form";
import { useRegisterProvider } from "@/hooks/use-register-provider";
import { EProviderType } from "@/types/api/provider";
import { Checkbox } from "@/components/ui/checkbox";
import { useRouter } from "next/navigation";
import { toast } from "sonner";
import { useSession } from "next-auth/react";
import { useEffect } from "react";

import { registerProviderSchema } from "@/lib/schemas/auth";

export default function RegisterProviderPage() {
    const router = useRouter();
    const { data: session } = useSession();
    const { mutate: registerProvider, isPending } = useRegisterProvider();

    const form = useForm<z.infer<typeof registerProviderSchema>>({
        resolver: zodResolver(registerProviderSchema),
        defaultValues: {
            name: "",
            type: EProviderType.Individual,
            documentNumber: "",
            phoneNumber: "",
            email: "",
            acceptedTerms: false,
            acceptedPrivacyPolicy: false,
        },
    });

    // Preencher nome e email da sessão
    const { getValues, setValue } = form; // Stable references
    useEffect(() => {
        if (session?.user) {
            if (session.user.name && !getValues("name")) {
                setValue("name", session.user.name);
            }
            if (session.user.email && !getValues("email")) {
                setValue("email", session.user.email);
            }
        }
    }, [session, getValues, setValue]);

    function onSubmit(values: z.infer<typeof registerProviderSchema>) {
        registerProvider(values, {
            onSuccess: () => {
                toast.success("Cadastro iniciado com sucesso!");
                router.push("/cadastro/prestador/perfil");
            },
            onError: (error) => {
                toast.error(`Erro ao cadastrar: ${error.message}`);
            }
        });
    }

    return (
        <div className="container mx-auto py-10 max-w-lg">
            <h1 className="text-2xl font-bold mb-6 text-center">Torne-se um Prestador</h1>
            <p className="text-muted-foreground text-center mb-8">
                Preencha seus dados básicos para começar a oferecer serviços na plataforma.
            </p>

            <Form {...form}>
                <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-6 bg-white p-6 rounded-lg shadow-sm border">
                    <FormField
                        control={form.control}
                        name="name"
                        render={({ field }) => (
                            <FormItem>
                                <FormLabel>Nome Completo (ou Razão Social)</FormLabel>
                                <FormControl>
                                    <Input placeholder="Seu nome" {...field} />
                                </FormControl>
                                <FormMessage />
                            </FormItem>
                        )}
                    />

                    <FormField
                        control={form.control}
                        name="type"
                        render={({ field }) => (
                            <FormItem>
                                <FormLabel>Tipo de Pessoa</FormLabel>
                                <FormControl>
                                    <div className="flex flex-wrap gap-4">
                                        <Button
                                            type="button"
                                            className="flex-1 min-w-[140px]"
                                            variant={field.value === EProviderType.Individual ? "primary" : "outline"}
                                            onClick={() => field.onChange(EProviderType.Individual)}
                                        >
                                            Pessoa Física (CPF)
                                        </Button>
                                        <Button
                                            type="button"
                                            className="flex-1 min-w-[140px]"
                                            variant={field.value === EProviderType.Company ? "primary" : "outline"}
                                            onClick={() => field.onChange(EProviderType.Company)}
                                        >
                                            Pessoa Jurídica (CNPJ)
                                        </Button>
                                        <Button
                                            type="button"
                                            className="flex-1 min-w-[140px]"
                                            variant={field.value === EProviderType.Cooperative ? "primary" : "outline"}
                                            onClick={() => field.onChange(EProviderType.Cooperative)}
                                        >
                                            Cooperativa
                                        </Button>
                                        <Button
                                            type="button"
                                            className="flex-1 min-w-[140px]"
                                            variant={field.value === EProviderType.Freelancer ? "primary" : "outline"}
                                            onClick={() => field.onChange(EProviderType.Freelancer)}
                                        >
                                            Autônomo
                                        </Button>
                                    </div>
                                </FormControl>
                                <FormMessage />
                            </FormItem>
                        )}
                    />

                    <FormField
                        control={form.control}
                        name="documentNumber"
                        render={({ field }) => (
                            <FormItem>
                                <FormLabel>{(form.watch("type") === EProviderType.Individual || form.watch("type") === EProviderType.Freelancer) ? "CPF" : "CNPJ"}</FormLabel>
                                <FormControl>
                                    <Input placeholder="Apenas números" {...field} />
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
                                <FormLabel>Email (da sua conta)</FormLabel>
                                <FormControl>
                                    <Input {...field} disabled className="bg-muted text-muted-foreground" />
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
                                <FormLabel>Telefone / WhatsApp</FormLabel>
                                <FormControl>
                                    <Input placeholder="(00) 00000-0000" {...field} />
                                </FormControl>
                                <FormMessage />
                            </FormItem>
                        )}
                    />

                    <FormField
                        control={form.control}
                        name="acceptedTerms"
                        render={({ field }) => (
                            <FormItem className="flex flex-row items-start space-x-3 space-y-0 rounded-md border p-4 shadow-sm">
                                <FormControl>
                                    <Checkbox
                                        checked={field.value}
                                        onCheckedChange={field.onChange}
                                    />
                                </FormControl>
                                <div className="space-y-1 leading-none">
                                    <FormLabel>
                                        Aceito os termos de uso
                                    </FormLabel>
                                    <FormMessage />
                                </div>
                            </FormItem>
                        )}
                    />

                    <FormField
                        control={form.control}
                        name="acceptedPrivacyPolicy"
                        render={({ field }) => (
                            <FormItem className="flex flex-row items-start space-x-3 space-y-0 rounded-md border p-4 shadow-sm">
                                <FormControl>
                                    <Checkbox
                                        checked={field.value}
                                        onCheckedChange={field.onChange}
                                    />
                                </FormControl>
                                <div className="space-y-1 leading-none">
                                    <FormLabel>
                                        Aceito a política de privacidade
                                    </FormLabel>
                                    <FormMessage />
                                </div>
                            </FormItem>
                        )}
                    />

                    <Button type="submit" className="w-full" disabled={isPending}>
                        {isPending ? "Criando conta..." : "Continuar"}
                    </Button>
                </form>
            </Form>
        </div>
    );
}
