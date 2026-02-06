import { auth } from "@/auth";
import { redirect } from "next/navigation";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { apiUsersGet2 } from "@/lib/api/generated";
import Link from "next/link";
import { User, Mail, Phone, MapPin, Pencil } from "lucide-react";

export const dynamic = "force-dynamic";

export default async function ProfilePage() {
    const session = await auth();

    if (!session?.user?.id) {
        redirect("/auth/signin");
    }

    // Fetch user details
    let user = null;

    try {
        const response = await apiUsersGet2({
            path: { id: session.user.id },
            headers: {
                'Authorization': `Bearer ${(session as any).accessToken}`
            }
        });

        user = (response.data as any)?.result;
    } catch (e) {
        console.error("Failed to fetch user profile", e);
    }

    if (!user) {
        return (
            <div className="container mx-auto py-10 text-center">
                <h1 className="text-2xl font-bold text-destructive">Erro ao carregar perfil</h1>
                <p className="mt-2 text-muted-foreground">Não foi possível carregar os dados do seu perfil no momento.</p>
            </div>
        );
    }

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
                            <p className="font-medium text-lg">{user.fullName || (user.firstName + ' ' + user.lastName)}</p>
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
                                {/* eslint-disable-next-line @typescript-eslint/no-explicit-any */}
                                {(user as any)?.address || (user as any)?.location || "Não informado"}
                            </p>
                        </div>
                    </div>
                </CardContent>
            </Card>
        </div>
    );
}
