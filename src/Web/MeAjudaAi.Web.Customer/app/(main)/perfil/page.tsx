"use client";

import { useSession } from "next-auth/react";
import { useQuery } from "@tanstack/react-query";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { authenticatedFetch } from "@/lib/api/fetch-client";
import { MeAjudaAiModulesUsersApplicationDtosUserDto } from "@/lib/api/generated/types.gen";
import { unwrapResponse } from "@/lib/api/response-utils";
import Link from "next/link";
import { User, Mail, Phone, MapPin, Pencil, Loader2 } from "lucide-react";

export default function ProfilePage() {
    const { data: session, status } = useSession();

    const { data: user, isLoading, error } = useQuery({
        queryKey: ["user-profile", session?.user?.id],
        queryFn: async () => {
            if (!session?.user?.id || !session?.accessToken) return null;

            const data = await authenticatedFetch<MeAjudaAiModulesUsersApplicationDtosUserDto>(`/api/v1/users/${session.user.id}`, {
                token: session.accessToken
            });

            return unwrapResponse<MeAjudaAiModulesUsersApplicationDtosUserDto>(data);
        },
        enabled: !!session?.user?.id && !!session?.accessToken,
    });

    if (status === "loading" || (status === "authenticated" && isLoading)) {
        return (
            <div className="flex flex-col items-center justify-center min-h-[60vh]">
                <Loader2 className="h-12 w-12 animate-spin text-primary mb-4" />
                <p className="text-muted-foreground">Carregando seu perfil...</p>
            </div>
        );
    }

    if (error || (status === "authenticated" && !user && !isLoading)) {
        return (
            <div className="container mx-auto py-20 text-center">
                <h1 className="text-2xl font-bold text-destructive">Erro ao carregar perfil</h1>
                <p className="mt-2 text-muted-foreground">Não foi possível carregar os dados do seu perfil no momento.</p>
                <Button onClick={() => window.location.reload()} className="mt-4">Tentar Novamente</Button>
            </div>
        );
    }

    if (!user) return null;

    return (
        <div className="container mx-auto px-4 py-8 max-w-4xl">
            <div className="flex flex-col sm:flex-row items-start sm:items-center justify-between mb-8 gap-4">
                <h1 className="text-3xl font-bold">Meu Perfil</h1>
                <Button asChild>
                    <Link href="/perfil/editar">
                        <Pencil className="mr-2 size-4" />
                        Editar Perfil
                    </Link>
                </Button>
            </div>

            <Card>
                <CardHeader>
                    <CardTitle className="text-xl">Informações Pessoais</CardTitle>
                </CardHeader>
                <CardContent className="space-y-6">
                    <div className="grid gap-6 md:grid-cols-2">
                        <div className="space-y-1">
                            <h4 className="text-sm font-medium text-muted-foreground flex items-center gap-2">
                                <User className="size-4" /> Nome Completo
                            </h4>
                            <p className="font-medium text-lg">{user.fullName || [user.firstName, user.lastName].filter(Boolean).join(' ') || ''}</p>
                        </div>

                        <div className="space-y-1">
                            <h4 className="text-sm font-medium text-muted-foreground flex items-center gap-2">
                                <Mail className="size-4" /> Email
                            </h4>
                            <p className="font-medium text-lg">{user.email}</p>
                        </div>

                        <div className="space-y-1">
                            <h4 className="text-sm font-medium text-muted-foreground flex items-center gap-2">
                                <Phone className="size-4" /> Telefone
                            </h4>
                            <p className="font-medium text-lg">{"Não informado"}</p>
                        </div>

                        <div className="space-y-1">
                            <h4 className="text-sm font-medium text-muted-foreground flex items-center gap-2">
                                <MapPin className="size-4" /> Localização
                            </h4>
                            <p className="font-medium text-lg">
                                {"Não informado"}
                            </p>
                        </div>
                    </div>
                </CardContent>
            </Card>
        </div>
    );
}
