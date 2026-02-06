import { auth } from "@/auth";
import { redirect } from "next/navigation";
import { AppProviders } from "@/components/providers/app-providers";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { apiUsersGet2 } from "@/lib/api/generated";
import { getAuthHeaders } from "@/lib/api/auth-headers";
import Link from "next/link";
import { User, Mail, Phone, MapPin, Edit } from "lucide-react";

export const dynamic = "force-dynamic";

export default async function ProfilePage() {
    const session = await auth();

    if (!session || !session.user || !session.user.id) {
        redirect("/api/auth/signin");
    }

    // Fetch user details from API
    let user = null;
    let error = null;

    try {
        const headers = await getAuthHeaders();
        const response = await apiUsersGet2({
            path: {
                id: session.user.id
            },
            headers: headers
        });

        user = (response.data as any)?.result;
    } catch (e) {
        console.error("Failed to fetch user profile", e);
        error = "Não foi possível carregar os dados do perfil.";
    }

    if (error) {
        return (
            <div className="container mx-auto px-4 py-8">
                <Card className="border-destructive">
                    <CardContent className="pt-6">
                        <p className="text-destructive font-medium">{error}</p>
                    </CardContent>
                </Card>
            </div>
        )
    }

    return (
        <div className="container mx-auto px-4 py-8 max-w-4xl">
            <div className="flex flex-col sm:flex-row items-start sm:items-center justify-between mb-8 gap-4">
                <h1 className="text-3xl font-bold">Meu Perfil</h1>
                <Button asChild>
                    <Link href="/perfil/editar">
                        <Edit className="mr-2 size-4" />
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
                            <p className="font-medium text-lg">{user?.fullName || session.user.name}</p>
                        </div>

                        <div className="space-y-1">
                            <h4 className="text-sm font-medium text-muted-foreground flex items-center gap-2">
                                <Mail className="size-4" /> Email
                            </h4>
                            <p className="font-medium text-lg">{user?.email || session.user.email}</p>
                        </div>

                        {/* Phone isn't in default session but is in API */}
                        <div className="space-y-1">
                            <h4 className="text-sm font-medium text-muted-foreground flex items-center gap-2">
                                <Phone className="size-4" /> Telefone
                            </h4>
                            <p className="font-medium text-lg">{/*user?.phoneNumber ||*/ "Não informado"}</p>
                            {/* Note: I need to check if user DTO has phoneNumber. It likely does based on types. */}
                        </div>

                        <div className="space-y-1">
                            <h4 className="text-sm font-medium text-muted-foreground flex items-center gap-2">
                                <MapPin className="size-4" /> Localização
                            </h4>
                            <p className="font-medium text-lg">{"- -"}</p>
                        </div>
                    </div>
                </CardContent>
            </Card>
        </div>
    );
}
