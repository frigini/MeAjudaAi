"use client";

import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import * as z from "zod";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import {
    Form,
    FormControl,
    FormField,
    FormItem,
    FormLabel,
    FormMessage,
} from "@/components/ui/form";
import { apiProfilePut } from "@/lib/api/generated/sdk.gen";
import { toast } from "sonner";
import { useRouter } from "next/navigation";
import { useState } from "react";
import { Loader2 } from "lucide-react";
import { useSession } from "next-auth/react";

// Schema validation
const profileSchema = z.object({
    firstName: z.string().min(2, "Nome deve ter pelo menos 2 caracteres"),
    lastName: z.string().min(2, "Sobrenome deve ter pelo menos 2 caracteres"),
    email: z.string().email("Email inválido"),
    phoneNumber: z.string().regex(/^\(\d{2}\)\s?\d{4,5}-\d{4}$/, "Formato inválido (ex: (11) 99999-9999)").optional().or(z.literal("")),
});

type ProfileFormValues = z.infer<typeof profileSchema>;

interface EditProfileFormProps {
    userId: string;
    initialData: Partial<ProfileFormValues>;
}

export function EditProfileForm({ userId, initialData }: EditProfileFormProps) {
    const router = useRouter();
    const { data: session, status } = useSession();
    const [isLoading, setIsLoading] = useState(false);

    const form = useForm<ProfileFormValues>({
        resolver: zodResolver(profileSchema),
        defaultValues: {
            firstName: initialData.firstName || "",
            lastName: initialData.lastName || "",
            email: initialData.email || "",
            phoneNumber: initialData.phoneNumber || "",
        },
    });

    async function onSubmit(data: ProfileFormValues) {
        if (status !== 'authenticated' || !session?.accessToken) {
            toast.error("Erro de autenticação", {
                description: "Sua sessão expirou. Faça login novamente.",
            });
            return;
        }

        setIsLoading(true);
        try {
            // Using properly typed session access
            const token = session.accessToken;
            const headers: HeadersInit = {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${token}`
            };

            const { error } = await apiProfilePut({
                path: { id: userId },
                body: {
                    firstName: data.firstName,
                    lastName: data.lastName,
                    email: data.email,
                    phoneNumber: data.phoneNumber || null,
                },
                headers: headers
            });

            if (error) {
                console.error("Profile update error:", error);
                toast.error("Erro ao atualizar perfil");
                return;
            }

            toast.success("Perfil atualizado com sucesso!");

            // Refresh to ensure data is consistent, then redirect
            // We await a small delay to allow the refresh (which is fire-and-forget) to likely process
            // before ensuring navigation to the profile page.
            router.refresh();
            await new Promise((resolve) => setTimeout(resolve, 100));
            router.replace("/perfil");
        } catch (error) {
            console.error("Profile update exception:", error);
            toast.error("Erro inesperado", {
                description: "Ocorreu um erro ao atualizar o perfil.",
            });
        } finally {
            setIsLoading(false);
        }
    }

    return (
        <Form {...form}>
            <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-6">
                <div className="grid gap-4 md:grid-cols-2">
                    <FormField
                        control={form.control}
                        name="firstName"
                        render={({ field }) => (
                            <FormItem>
                                <FormLabel>Nome</FormLabel>
                                <FormControl>
                                    <Input placeholder="Seu nome" {...field} />
                                </FormControl>
                                <FormMessage />
                            </FormItem>
                        )}
                    />
                    <FormField
                        control={form.control}
                        name="lastName"
                        render={({ field }) => (
                            <FormItem>
                                <FormLabel>Sobrenome</FormLabel>
                                <FormControl>
                                    <Input placeholder="Seu sobrenome" {...field} />
                                </FormControl>
                                <FormMessage />
                            </FormItem>
                        )}
                    />
                </div>

                <FormField
                    control={form.control}
                    name="email"
                    render={({ field }) => (
                        <FormItem>
                            <FormLabel>Email</FormLabel>
                            <FormControl>
                                <Input
                                    placeholder="seu.email@exemplo.com"
                                    {...field}
                                    disabled={true}
                                    title="Alteração de email não permitida neste momento."
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
                            <FormLabel>Telefone (Opcional)</FormLabel>
                            <FormControl>
                                <Input placeholder="(11) 99999-9999" {...field} />
                            </FormControl>
                            <FormMessage />
                        </FormItem>
                    )}
                />

                <div className="flex justify-end gap-4">
                    <Button
                        type="button"
                        variant="outline"
                        onClick={() => router.back()}
                        disabled={isLoading}
                    >
                        Cancelar
                    </Button>
                    <Button type="submit" disabled={isLoading} className="min-w-[120px]">
                        {isLoading ? (
                            <>
                                <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                                Salvando...
                            </>
                        ) : (
                            "Salvar Alterações"
                        )}
                    </Button>
                </div>
            </form>
        </Form>
    );
}
