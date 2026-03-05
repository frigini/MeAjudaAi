"use client";

import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Form, FormControl, FormField, FormItem, FormLabel, FormMessage } from "@/components/ui/form";
import { useRegisterProvider } from "@/hooks/use-register-provider";
import { EProviderType } from "@/types/api/provider";
import { Checkbox } from "@/components/ui/checkbox";
import { useRouter } from "next/navigation";
import { toast } from "sonner";
import { useSession } from "next-auth/react";
import { useEffect, useState } from "react";
import Link from "next/link";
import { ShieldCheck, Info } from "lucide-react";

import { RegisterProviderSchema, registerProviderSchema } from "@/lib/schemas/auth";

export default function RegisterProviderPage() {
    const [showPrivacyInfo, setShowPrivacyInfo] = useState(false);
    const router = useRouter();
    const { data: session } = useSession();
    const { mutate: registerProvider, isPending } = useRegisterProvider();

    const form = useForm<RegisterProviderSchema>({
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

    function onSubmit(values: RegisterProviderSchema) {
        registerProvider(values, {
            onSuccess: () => {
                toast.success("Cadastro iniciado com sucesso!");
                router.push("/cadastro/prestador/perfil");
            },
            onError: (error) => {
                console.error("Erro ao cadastrar prestador:", error);
                toast.error("Erro ao cadastrar. Tente novamente mais tarde.");
            }
        });
    }

    const providerType = form.watch("type");

    return (
        <div className="w-full max-w-md mx-auto space-y-8 px-4 py-8 mt-12 mb-12">

            {/* Stepper */}
            <div className="flex items-center justify-between w-full max-w-xs mx-auto mb-8 relative">
                <div className="absolute left-0 top-1/2 -translate-y-1/2 w-full h-0.5 bg-border z-0"></div>

                <div className="relative z-10 flex flex-col items-center gap-2 bg-background px-2">
                    <div className="flex h-8 w-8 items-center justify-center rounded-full bg-primary text-primary-foreground font-semibold text-sm ring-4 ring-background">
                        1
                    </div>
                    <span className="text-xs font-medium text-primary absolute -bottom-6 whitespace-nowrap">Dados Iniciais</span>
                </div>

                <div className="relative z-10 flex flex-col items-center gap-2 bg-background px-2">
                    <div className="flex h-8 w-8 items-center justify-center rounded-full bg-muted text-muted-foreground font-semibold text-sm ring-4 ring-background">
                        2
                    </div>
                    <span className="text-xs font-medium text-muted-foreground absolute -bottom-6 whitespace-nowrap">Endereço</span>
                </div>

                <div className="relative z-10 flex flex-col items-center gap-2 bg-background px-2">
                    <div className="flex h-8 w-8 items-center justify-center rounded-full bg-muted text-muted-foreground font-semibold text-sm ring-4 ring-background">
                        3
                    </div>
                    <span className="text-xs font-medium text-muted-foreground absolute -bottom-6 whitespace-nowrap">Documentos</span>
                </div>
            </div>

            {/* Header */}
            <div className="text-center pt-4">
                <h1 className="text-2xl font-bold tracking-tight">
                    Passo 1: Crie sua conta
                </h1>
                <p className="text-muted-foreground mt-2 text-sm">
                    Inicie seu credenciamento. Nas próximas etapas, pediremos seu endereço e documentos para garantir a segurança da plataforma.
                </p>
            </div>

            {/* Registration Form */}
            <Form {...form}>
                <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-7">
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
                                <FormLabel>{(providerType === EProviderType.Individual || providerType === EProviderType.Freelancer) ? "CPF" : "CNPJ"}</FormLabel>
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
                                    <Input
                                        {...field}
                                        disabled={Boolean(session?.user?.email)}
                                        className={Boolean(session?.user?.email) ? "bg-muted text-muted-foreground" : ""}
                                    />
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
                                        Aceito os <Link href="/termos-de-uso" target="_blank" rel="noopener noreferrer" className="underline hover:text-primary">Termos de Uso</Link>
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
                                        Aceito a <Link href="/politica-de-privacidade" target="_blank" rel="noopener noreferrer" className="underline hover:text-primary">Política de Privacidade</Link>
                                    </FormLabel>
                                    <FormMessage />
                                </div>
                            </FormItem>
                        )}
                    />

                    <Button type="submit" size="lg" className="w-full mt-8 bg-secondary hover:bg-secondary-hover text-white text-lg font-semibold shadow-md transition-all hover:scale-[1.02]" disabled={isPending}>
                        {isPending ? "Criando conta..." : "Continuar"}
                    </Button>
                </form>
            </Form>

            {/* Privacy & Security Badge */}
            <div className="relative">
                <button
                    type="button"
                    onClick={() => setShowPrivacyInfo(!showPrivacyInfo)}
                    className="w-full flex items-center gap-3 rounded-lg border border-green-200 bg-green-50 px-4 py-3 text-left transition-colors hover:bg-green-100 mt-6"
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
                                    Seus dados são verificados, mas permanecem privados.{" "}
                                    <strong>Nenhum outro usuário</strong> poderá ver seu telefone ou documento inteiro.
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

            {/* Passive consent footer */}
            <p className="text-center text-xs text-muted-foreground mt-6">
                Ao criar sua conta, você concorda com nossos{" "}
                <Link href="/termos-de-uso" className="underline hover:text-primary">Termos de Uso</Link>{" "}
                e{" "}
                <Link href="/politica-de-privacidade" className="underline hover:text-primary">Política de Privacidade</Link>.
            </p>
        </div>
    );
}
