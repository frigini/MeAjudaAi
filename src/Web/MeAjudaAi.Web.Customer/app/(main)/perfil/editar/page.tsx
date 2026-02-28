import { auth } from "@/auth";
import { redirect } from "next/navigation";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { authenticatedFetch } from "@/lib/api/fetch-client";
import { ApiUsersGet2Responses } from "@/lib/api/generated/types.gen";
import { EditProfileForm } from "@/components/profile/edit-profile-form";

export const dynamic = "force-dynamic";

export default async function EditProfilePage() {
    const session = await auth();

    if (!session?.user?.id) {
        redirect("/auth/signin");
    }

    // Fetch user details
    let user = null;

    try {
        // eslint-disable-next-line @typescript-eslint/no-explicit-any
        const token = (session as any).accessToken;

        const data = await authenticatedFetch<ApiUsersGet2Responses>(`/api/v1/users/${session.user.id}`, {
            token: token
        });

        // eslint-disable-next-line @typescript-eslint/no-explicit-any
        user = (data as any)?.value || (data as any)?.result || data;
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
        <div className="container mx-auto px-4 py-8 max-w-2xl">
            <h1 className="text-3xl font-bold mb-8">Editar Perfil</h1>

            <Card>
                <CardHeader>
                    <CardTitle>Dados Pessoais</CardTitle>
                </CardHeader>
                <CardContent>
                    <EditProfileForm
                        userId={session.user.id}
                        initialData={{
                            firstName: user.firstName,
                            lastName: user.lastName,
                            email: user.email,
                            // phoneNumber: user.phoneNumber // Missing in DTO
                        }}
                    />
                </CardContent>
            </Card>
        </div>
    );
}
