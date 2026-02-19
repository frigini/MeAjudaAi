"use client";

import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Form, FormControl, FormField, FormItem, FormLabel, FormMessage } from "@/components/ui/form";
import { addressSchema, AddressSchema } from "@/lib/schemas/auth";
import { useMyProviderProfile } from "@/hooks/use-my-provider-profile";
import { useUpdateProviderProfile } from "@/hooks/use-update-provider-profile";
import { useViaCep } from "@/hooks/use-via-cep";
import { useRouter } from "next/navigation";
import { toast } from "sonner";
import { useEffect } from "react";
import { Loader2 } from "lucide-react";

export default function ProviderAddressPage() {
    const router = useRouter();
    const { data: profile, isLoading: isLoadingProfile } = useMyProviderProfile();
    const { mutate: updateProfile, isPending: isSaving } = useUpdateProviderProfile();
    const { fetchAddress, isLoading: isLoadingCep } = useViaCep();

    const form = useForm<AddressSchema>({
        resolver: zodResolver(addressSchema),
        defaultValues: {
            zipCode: "",
            street: "",
            number: "",
            complement: "",
            neighborhood: "", // Renamed from district
            city: "",
            state: "",
        },
    });

    // Load existing address if available
    useEffect(() => {
        if (profile?.businessProfile?.primaryAddress) {
            const addr = profile.businessProfile.primaryAddress;
            form.reset({
                zipCode: addr.zipCode || "",
                street: addr.street || "",
                number: addr.number || "",
                complement: addr.complement || "",
                neighborhood: addr.neighborhood || "",
                city: addr.city || "",
                state: addr.state || "",
            });
        }
    }, [profile, form]);

    const handleCepBlur = async (e: React.FocusEvent<HTMLInputElement>) => {
        const cep = e.target.value;
        if (cep.length >= 8) {
            const data = await fetchAddress(cep);
            if (data) {
                form.setValue("street", data.logradouro);
                form.setValue("neighborhood", data.bairro); // ViaCEP uses 'bairro', mapped to 'neighborhood'
                form.setValue("city", data.localidade);
                form.setValue("state", data.uf);
                form.setFocus("number");
            }
        }
    };

    function onSubmit(values: AddressSchema) {
        if (!profile) return;

        // Construct full update payload properly
        const payload = {
            name: profile.name,
            businessProfile: {
                legalName: profile.businessProfile?.legalName || profile.name,
                fantasyName: profile.businessProfile?.fantasyName || null,
                description: profile.businessProfile?.description || null,
                contactInfo: {
                    email: profile.businessProfile?.contactInfo?.email || "",
                    phoneNumber: profile.businessProfile?.contactInfo?.phoneNumber || null,
                    website: profile.businessProfile?.contactInfo?.website || null,
                },
                primaryAddress: { // Changed to primaryAddress
                    street: values.street,
                    number: values.number,
                    complement: values.complement || null,
                    neighborhood: values.neighborhood,
                    city: values.city,
                    state: values.state,
                    zipCode: values.zipCode,
                    country: "Brasil",
                },
            },
        };

        // @ts-ignore - Payload structure might slightly differ from explicit types but matches useUpdateProviderProfile expectations
        updateProfile(payload, {
            onSuccess: () => {
                toast.success("Endereço salvo com sucesso!");
                router.push("/cadastro/prestador/perfil");
            },
            onError: (error) => {
                toast.error(`Erro ao salvar endereço: ${error.message}`);
            },
        });
    }

    if (isLoadingProfile) {
        return (
            <div className="container mx-auto py-20 flex justify-center">
                <Loader2 className="h-8 w-8 animate-spin text-primary" />
            </div>
        );
    }

    return (
        <div className="container mx-auto py-10 max-w-2xl">
            <h1 className="text-2xl font-bold mb-6">Endereço Comercial</h1>
            <p className="text-muted-foreground mb-8">
                Informe o endereço de onde você presta serviços ou sua base operacional.
            </p>

            <Form {...form}>
                <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-6 bg-white p-6 rounded-lg shadow-sm border">
                    <div className="grid grid-cols-12 gap-4">
                        <div className="col-span-4">
                            <FormField
                                control={form.control}
                                name="zipCode"
                                render={({ field }) => (
                                    <FormItem>
                                        <FormLabel>CEP</FormLabel>
                                        <FormControl>
                                            <div className="relative">
                                                <Input
                                                    placeholder="00000-000"
                                                    {...field}
                                                    onBlur={(e) => {
                                                        field.onBlur();
                                                        handleCepBlur(e);
                                                    }}
                                                    maxLength={9}
                                                />
                                                {isLoadingCep && (
                                                    <div className="absolute right-3 top-2.5">
                                                        <Loader2 className="h-4 w-4 animate-spin text-muted-foreground" />
                                                    </div>
                                                )}
                                            </div>
                                        </FormControl>
                                        <FormMessage />
                                    </FormItem>
                                )}
                            />
                        </div>
                        <div className="col-span-8 flex items-end pb-2">
                            <a
                                href="https://buscacepinter.correios.com.br/app/endereco/index.php"
                                target="_blank"
                                rel="noopener noreferrer"
                                className="text-xs text-primary hover:underline"
                            >
                                Não sei meu CEP
                            </a>
                        </div>
                    </div>

                    <div className="grid grid-cols-12 gap-4">
                        <div className="col-span-9">
                            <FormField
                                control={form.control}
                                name="street"
                                render={({ field }) => (
                                    <FormItem>
                                        <FormLabel>Rua / Logradouro</FormLabel>
                                        <FormControl>
                                            <Input {...field} />
                                        </FormControl>
                                        <FormMessage />
                                    </FormItem>
                                )}
                            />
                        </div>
                        <div className="col-span-3">
                            <FormField
                                control={form.control}
                                name="number"
                                render={({ field }) => (
                                    <FormItem>
                                        <FormLabel>Número</FormLabel>
                                        <FormControl>
                                            <Input {...field} />
                                        </FormControl>
                                        <FormMessage />
                                    </FormItem>
                                )}
                            />
                        </div>
                    </div>

                    <FormField
                        control={form.control}
                        name="complement"
                        render={({ field }) => (
                            <FormItem>
                                <FormLabel>Complemento (Opcional)</FormLabel>
                                <FormControl>
                                    <Input placeholder="Apto, Sala, Bloco..." {...field} />
                                </FormControl>
                                <FormMessage />
                            </FormItem>
                        )}
                    />

                    <div className="grid grid-cols-12 gap-4">
                        <div className="col-span-5">
                            <FormField
                                control={form.control}
                                name="neighborhood"
                                render={({ field }) => (
                                    <FormItem>
                                        <FormLabel>Bairro</FormLabel>
                                        <FormControl>
                                            <Input {...field} />
                                        </FormControl>
                                        <FormMessage />
                                    </FormItem>
                                )}
                            />
                        </div>
                        <div className="col-span-5">
                            <FormField
                                control={form.control}
                                name="city"
                                render={({ field }) => (
                                    <FormItem>
                                        <FormLabel>Cidade</FormLabel>
                                        <FormControl>
                                            <Input {...field} readOnly className="bg-slate-50" />
                                        </FormControl>
                                        <FormMessage />
                                    </FormItem>
                                )}
                            />
                        </div>
                        <div className="col-span-2">
                            <FormField
                                control={form.control}
                                name="state"
                                render={({ field }) => (
                                    <FormItem>
                                        <FormLabel>UF</FormLabel>
                                        <FormControl>
                                            <Input {...field} readOnly className="bg-slate-50" maxLength={2} />
                                        </FormControl>
                                        <FormMessage />
                                    </FormItem>
                                )}
                            />
                        </div>
                    </div>

                    <div className="flex justify-end gap-4 pt-4">
                        <Button type="button" variant="outline" onClick={() => router.back()}>
                            Voltar
                        </Button>
                        <Button type="submit" disabled={isSaving}>
                            {isSaving ? (
                                <>
                                    <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                                    Salvando...
                                </>
                            ) : (
                                "Salvar e Continuar"
                            )}
                        </Button>
                    </div>
                </form>
            </Form>
        </div>
    );
}
