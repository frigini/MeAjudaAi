"use client";

import { useEffect } from "react";
import { useRouter } from "next/navigation";
import { useForm } from "react-hook-form";
import { z } from "zod";
import { zodResolver } from "@hookform/resolvers/zod";
import { Label } from "../../../components/ui/label";
import { Input } from "../../../components/ui/input";
import { Button } from "../../../components/ui/button";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { apiMeGet, apiMePut, MeAjudaAiModulesProvidersApplicationDtosRequestsUpdateProviderProfileRequest } from "@/lib/api/generated";
import { toast } from "sonner";

const basicInfoSchema = z.object({
  description: z.string().max(1000, "Descrição muito longa").optional(),
  zipCode: z.string().min(8, "CEP inválido").max(9, "CEP inválido"),
  street: z.string().min(3, "Endereço inválido").max(200, "Endereço muito longo"),
  number: z.string().min(1, "Número obrigatório").max(20, "Número muito longo"),
  complement: z.string().max(100).optional(),
  neighborhood: z.string().min(2, "Bairro inválido").max(100, "Bairro muito longo"),
  city: z.string().min(2, "Cidade inválida").max(100, "Cidade muito longa"),
  state: z.string().length(2, "Use a sigla com 2 letras").max(50),
});

type BasicInfoFormValues = z.infer<typeof basicInfoSchema>;

export default function BasicInfoPage() {
  const router = useRouter();
  const queryClient = useQueryClient();

  const form = useForm<BasicInfoFormValues>({
    resolver: zodResolver(basicInfoSchema),
    defaultValues: {
      description: "",
      zipCode: "",
      street: "",
      number: "",
      complement: "",
      neighborhood: "",
      city: "",
      state: "",
    },
  });

  const { data: providerData, isLoading } = useQuery({
    queryKey: ["providerMe"],
    queryFn: () => apiMeGet(),
  });

  useEffect(() => {
    if (providerData?.data?.data) {
      const bp = providerData.data.data.businessProfile;
      const addr = bp?.primaryAddress;
      
      form.reset({
        description: bp?.description || "",
        zipCode: addr?.zipCode || "",
        street: addr?.street || "",
        number: addr?.number || "",
        complement: addr?.complement || "",
        neighborhood: addr?.neighborhood || "",
        city: addr?.city || "",
        state: addr?.state || "",
      });
    }
  }, [providerData, form]);

  const updateMutation = useMutation({
    mutationFn: (req: MeAjudaAiModulesProvidersApplicationDtosRequestsUpdateProviderProfileRequest) =>
      apiMePut({ body: req }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["providerMe"] });
      toast.success("Informações básicas salvas com sucesso!");
      router.push("/onboarding/services");
    },
    onError: () => {
      toast.error("Erro ao salvar. Tente novamente.");
    },
  });

  const onSubmit = (data: BasicInfoFormValues) => {
    const currentBp = providerData?.data?.data?.businessProfile;
    
    updateMutation.mutate({
      name: providerData?.data?.data?.name || "",
      businessProfile: {
        legalName: currentBp?.legalName,
        fantasyName: currentBp?.fantasyName,
        description: data.description,
        contactInfo: currentBp?.contactInfo,
        showAddressToClient: currentBp?.showAddressToClient,
        primaryAddress: {
          street: data.street,
          number: data.number,
          complement: data.complement,
          neighborhood: data.neighborhood,
          city: data.city,
          state: data.state,
          zipCode: data.zipCode,
          country: "Brasil",
        },
      },
    });
  };

  if (isLoading) {
    return (
      <div className="flex items-center justify-center min-h-[400px]">
        <div className="animate-pulse text-muted-foreground">Carregando...</div>
      </div>
    );
  }

  return (
    <form onSubmit={form.handleSubmit(onSubmit)} className="flex flex-col gap-6">
      <div>
        <h2 className="text-xl font-semibold text-foreground">Informações Básicas</h2>
        <p className="mt-1 text-sm text-foreground-subtle">
          Complete os dados do seu perfil de negócio (BusinessProfile) para que os clientes te conheçam melhor.
        </p>
      </div>

      <div className="flex flex-col gap-4 border-t border-border pt-6">
        <div className="flex flex-col gap-1.5">
          <Label htmlFor="description">Descrição do Negócio / Biografia</Label>
          <textarea
            id="description"
            rows={4}
            className="flex w-full rounded-md border border-input bg-transparent px-3 py-2 text-sm shadow-sm placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring"
            placeholder="Conte um pouco sobre sua experiência e diferenciais..."
            {...form.register("description")}
          />
          {form.formState.errors.description && (
            <span className="text-xs text-destructive">{form.formState.errors.description.message}</span>
          )}
        </div>

        <h3 className="text-sm font-semibold text-foreground pt-4">Endereço Principal</h3>
        
        <div className="grid grid-cols-6 gap-4">
          <div className="col-span-2 flex flex-col gap-1.5">
            <Label htmlFor="zipCode">CEP</Label>
            <Input id="zipCode" placeholder="00000-000" {...form.register("zipCode")} />
            {form.formState.errors.zipCode && (
              <span className="text-xs text-destructive">{form.formState.errors.zipCode.message}</span>
            )}
          </div>
          <div className="col-span-4 flex flex-col gap-1.5">
            <Label htmlFor="street">Rua / Avenida</Label>
            <Input id="street" placeholder="Av. Principal" {...form.register("street")} />
            {form.formState.errors.street && (
              <span className="text-xs text-destructive">{form.formState.errors.street.message}</span>
            )}
          </div>
        </div>

        <div className="grid grid-cols-2 gap-4">
          <div className="flex flex-col gap-1.5">
            <Label htmlFor="number">Número</Label>
            <Input id="number" placeholder="123" {...form.register("number")} />
            {form.formState.errors.number && (
              <span className="text-xs text-destructive">{form.formState.errors.number.message}</span>
            )}
          </div>
          <div className="flex flex-col gap-1.5">
            <Label htmlFor="complement">Complemento</Label>
            <Input id="complement" placeholder="Sala 101" {...form.register("complement")} />
          </div>
        </div>

        <div className="grid grid-cols-2 gap-4">
          <div className="flex flex-col gap-1.5">
            <Label htmlFor="neighborhood">Bairro</Label>
            <Input id="neighborhood" placeholder="Centro" {...form.register("neighborhood")} />
            {form.formState.errors.neighborhood && (
              <span className="text-xs text-destructive">{form.formState.errors.neighborhood.message}</span>
            )}
          </div>
        </div>

        <div className="grid grid-cols-2 gap-4">
          <div className="flex flex-col gap-1.5">
            <Label htmlFor="city">Cidade</Label>
            <Input id="city" placeholder="São Paulo" {...form.register("city")} />
            {form.formState.errors.city && (
              <span className="text-xs text-destructive">{form.formState.errors.city.message}</span>
            )}
          </div>
          <div className="flex flex-col gap-1.5">
            <Label htmlFor="state">Estado</Label>
            <Input id="state" placeholder="SP" maxLength={2} {...form.register("state")} />
            {form.formState.errors.state && (
              <span className="text-xs text-destructive">{form.formState.errors.state.message}</span>
            )}
          </div>
        </div>
      </div>

      <div className="mt-4 flex items-center justify-between border-t border-border pt-6">
        <Button variant="ghost" type="button" onClick={() => router.back()}>
          Voltar
        </Button>
        <Button type="submit" disabled={updateMutation.isPending}>
          {updateMutation.isPending ? "Salvando..." : "Salvar e Continuar"}
        </Button>
      </div>
    </form>
  );
}
