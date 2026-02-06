import { auth } from "@/auth";
import { redirect } from "next/navigation";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { apiUsersGet2 } from "@/lib/api/generated";
import { getAuthHeaders } from "@/lib/api/auth-headers";
import { EditProfileForm } from "@/components/profile/edit-profile-form";

export const dynamic = "force-dynamic";

export default async function EditProfilePage() {
    const session = await auth();

    if (!session || !session.user || !session.user.id) {
        redirect("/api/auth/signin");
    }

    // Fetch user details
    let user = null;

    try {
        const headers = await getAuthHeaders();
        const response = await apiUsersGet2({
            path: { id: session.user.id },
            headers: headers
        });

        user = (response.data as any)?.result;
    } catch (e) {
        console.error("Failed to fetch user profile", e);
        // Error handling could be improved, but for now allow rendering form optionally or redirect
    }

    if (!user) {
        return (
            <div className="container mx-auto px-4 py-8">
                <p className="text-destructive">Erro ao carregar dados do usuário.</p>
            </div>
        )
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
