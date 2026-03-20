"use client";

import { useForm, useFieldArray } from "react-hook-form";
import { z } from "zod";
import { zodResolver } from "@hookform/resolvers/zod";
import { Label } from "../../components/ui/label";
import { Input } from "../../components/ui/input";
import { Button } from "../../components/ui/button";
import { Plus, Trash2 } from "lucide-react";
import Link from "next/link";
import { useRouter } from "next/navigation";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { apiMeGet, apiMePut, MeAjudaAiModulesProvidersApplicationDtosRequestsUpdateProviderProfileRequest } from "../../lib/api/generated";
import { toast } from "sonner";
import { useEffect } from "react";

const profileSchema = z.object({
  fullName: z.string().min(3, "Nome muito curto"),
  fantasyName: z.string().optional(),
  email: z.string().email("E-mail inválido"),
  cpf: z.string().min(11, "CPF inválido"),
  phones: z.array(
    z.object({
      number: z.string().min(8, "Telefone inválido"),
      isWhatsapp: z.boolean().default(false),
    })
  ).min(1, "Adicione pelo menos um telefone"),
  cep: z.string().min(8, "CEP inválido"),
  address: z.string().min(3, "Endereço inválido"),
  number: z.string().min(1, "Campo obrigatório"),
  neighborhood: z.string().min(2, "Bairro inválido"),
  city: z.string().min(2, "Cidade inválida"),
  state: z.string().length(2, "Sigla de 2 letras"),
  showAddressToClient: z.boolean().default(false),
});

type ProfileFormValues = z.infer<typeof profileSchema>;

export default function AlterarDadosPage() {
  const router = useRouter();
  const queryClient = useQueryClient();
  
  const form = useForm<ProfileFormValues>({
    resolver: zodResolver(profileSchema),
    defaultValues: {
      fullName: "",
      fantasyName: "",
      email: "",
      cpf: "",
      phones: [{ number: "", isWhatsapp: false }],
      cep: "",
      address: "",
      number: "",
      neighborhood: "",
      city: "",
      state: "",
      showAddressToClient: false,
    },
  });

  const { fields: phoneFields, append: appendPhone, remove: removePhone } = useFieldArray({
    control: form.control,
    name: "phones",
  });

  const { data: response, isLoading } = useQuery({
    queryKey: ["providerMe"],
    queryFn: () => apiMeGet()
  });

  useEffect(() => {
    if (response?.data?.data) {
      const provider = response.data.data;
      const bp = provider.businessProfile;
      const contact = bp?.contactInfo;
      const addr = bp?.primaryAddress;

      let mappedPhones = [];
      if (contact?.phoneNumber) {
         mappedPhones.push({ number: contact.phoneNumber, isWhatsapp: false });
      }
      if (contact?.additionalPhones && contact.additionalPhones.length > 0) {
         contact.additionalPhones.forEach((p: string) => mappedPhones.push({ number: p, isWhatsapp: false }));
      }
      if (mappedPhones.length === 0) {
         mappedPhones.push({ number: "", isWhatsapp: false });
      }

      form.reset({
        fullName: provider.name || "",
        fantasyName: bp?.fantasyName || "",
        email: contact?.email || "",
        cpf: "Não disponível na consulta pública", 
        phones: mappedPhones,
        cep: addr?.zipCode || "",
        address: addr?.street || "",
        number: addr?.number || "",
        neighborhood: addr?.neighborhood || "",
        city: addr?.city || "",
        state: addr?.state || "",
        showAddressToClient: bp?.showAddressToClient ?? false,
      });
    }
  }, [response, form]);

  const updateMutation = useMutation({
    mutationFn: (req: MeAjudaAiModulesProvidersApplicationDtosRequestsUpdateProviderProfileRequest) =>
      apiMePut({ body: req }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["providerMe"] });
      toast.success("Perfil atualizado com sucesso!");
      router.push("/");
    },
    onError: () => {
      toast.error("Erro ao atualizar o perfil. Tente novamente mais tarde.");
    }
  });

  const onSubmit = (data: ProfileFormValues) => {
    const primaryPhone = data.phones[0]?.number || "";
    const additionalPhones = data.phones.slice(1).map(p => p.number).filter(p => !!p);

    updateMutation.mutate({
      name: data.fullName,
      businessProfile: {
        fantasyName: data.fantasyName,
        showAddressToClient: data.showAddressToClient,
        contactInfo: {
          email: data.email,
          phoneNumber: primaryPhone,
          additionalPhones: additionalPhones,
          website: response?.data?.data?.businessProfile?.contactInfo?.website
        },
        primaryAddress: {
          zipCode: data.cep,
          street: data.address,
          number: data.number,
          neighborhood: data.neighborhood,
          city: data.city,
          state: data.state,
          complement: response?.data?.data?.businessProfile?.primaryAddress?.complement,
          country: response?.data?.data?.businessProfile?.primaryAddress?.country || "Brasil",
        }
      }
    });
  };

  return (
    <div className="container mx-auto max-w-2xl py-8 px-4 sm:px-6 lg:px-8">
      <div className="mb-6 flex items-center gap-4">
        <Button variant="ghost" size="sm" asChild className="h-8 w-8 px-0">
          <Link href="/">
            <span  className="text-xl">&lsaquo;</span>
            <span className="sr-only">Voltar</span>
          </Link>
        </Button>
        <span className="font-bold">AjudaAí</span>
        <div className="ml-auto text-sm font-medium hover:underline cursor-pointer">Sair</div>
      </div>

      <main className="rounded-xl border border-border bg-surface p-6 shadow-sm sm:p-10">
        <h1 className="mb-8 text-2xl font-bold tracking-tight text-foreground">
          Alterar dados
        </h1>

        <form onSubmit={form.handleSubmit(onSubmit)} className="flex flex-col gap-6">
          <div className="flex flex-col gap-1.5">
            <Label htmlFor="fullName">Nome completo</Label>
            <Input id="fullName" {...form.register("fullName")} />
            {form.formState.errors.fullName && (
              <span className="text-xs text-destructive">{form.formState.errors.fullName.message}</span>
            )}
          </div>

          <div className="flex flex-col gap-1.5">
            <Label htmlFor="fantasyName">Nome fantasia</Label>
            <Input id="fantasyName" {...form.register("fantasyName")} />
          </div>

          <div className="flex flex-col gap-1.5">
            <Label htmlFor="email">E-mail</Label>
            <Input id="email" type="email" {...form.register("email")} />
            {form.formState.errors.email && (
              <span className="text-xs text-destructive">{form.formState.errors.email.message}</span>
            )}
          </div>

          <div className="flex flex-col gap-1.5">
            <Label htmlFor="cpf">CPF</Label>
            <Input id="cpf" {...form.register("cpf")} />
            {form.formState.errors.cpf && (
              <span className="text-xs text-destructive">{form.formState.errors.cpf.message}</span>
            )}
          </div>

          {/* Telefones */}
          <div className="flex flex-col gap-3">
            <Label>Telefones principais</Label>
            <div className="flex items-center gap-2">
              <Input 
                placeholder="Adicione um novo telefone aqui" 
                className="w-full text-sm h-9" 
                id="new-phone"
              />
              <Button type="button" size="sm" onClick={() => {
                const input = document.getElementById("new-phone") as HTMLInputElement;
                if (input.value) {
                  appendPhone({ number: input.value, isWhatsapp: false });
                  input.value = "";
                }
              }} className="h-9 w-9 px-0 shrink-0">
                <Plus className="h-4 w-4" />
              </Button>
            </div>
            
            {phoneFields.length > 0 && (
               <div className="mt-2 text-sm">
                 <div className="mb-2 grid grid-cols-[1fr_80px_60px] gap-4 font-semibold text-xs text-foreground-subtle uppercase tracking-wide">
                   <div>Número</div>
                   <div className="text-center">É Whatsapp?</div>
                   <div className="text-center">Excluir</div>
                 </div>
                 {phoneFields.map((field, index) => (
                   <div key={field.id} className="grid grid-cols-[1fr_80px_60px] items-center gap-4 py-2 border-b border-border/50 last:border-0 hover:bg-muted/50 transition-colors">
                     <span className="tabular-nums opacity-90">{field.number}</span>
                     <div className="flex justify-center">
                       <input 
                         type="checkbox" 
                         {...form.register(`phones.${index}.isWhatsapp`)} 
                         className="h-4 w-4 accent-emerald-500 rounded border-border"
                       />
                     </div>
                     <div className="flex justify-center">
                       <button 
                         type="button" 
                         onClick={() => removePhone(index)}
                         className="text-destructive hover:bg-destructive/10 p-1.5 rounded-md transition-colors"
                       >
                         <Trash2 className="h-4 w-4" />
                       </button>
                     </div>
                   </div>
                 ))}
               </div>
            )}
            {form.formState.errors.phones?.message && (
              <span className="text-xs text-destructive">{form.formState.errors.phones.message}</span>
            )}
          </div>

          <div className="flex flex-col gap-1.5">
            <Label htmlFor="cep">CEP</Label>
            <Input id="cep" {...form.register("cep")} />
            {form.formState.errors.cep && (
              <span className="text-xs text-destructive">{form.formState.errors.cep.message}</span>
            )}
          </div>

          <div className="flex flex-col gap-1.5">
            <Label htmlFor="address">Logradouro</Label>
            <Input id="address" {...form.register("address")} />
            {form.formState.errors.address && (
              <span className="text-xs text-destructive">{form.formState.errors.address.message}</span>
            )}
          </div>

          <div className="flex flex-col gap-1.5">
            <Label htmlFor="number">Número</Label>
            <Input id="number" {...form.register("number")} />
            {form.formState.errors.number && (
              <span className="text-xs text-destructive">{form.formState.errors.number.message}</span>
            )}
          </div>

          <div className="flex flex-col gap-1.5">
            <Label htmlFor="neighborhood">Bairro</Label>
            <Input id="neighborhood" {...form.register("neighborhood")} />
            {form.formState.errors.neighborhood && (
              <span className="text-xs text-destructive">{form.formState.errors.neighborhood.message}</span>
            )}
          </div>

          <div className="flex flex-col gap-1.5">
            <Label htmlFor="city">Cidade</Label>
            <Input id="city" {...form.register("city")} />
            {form.formState.errors.city && (
              <span className="text-xs text-destructive">{form.formState.errors.city.message}</span>
            )}
          </div>

          <div className="flex flex-col gap-1.5">
            <Label htmlFor="state">Estado</Label>
            <Input id="state" {...form.register("state")} />
            {form.formState.errors.state && (
              <span className="text-xs text-destructive">{form.formState.errors.state.message}</span>
            )}
          </div>

          <div className="flex items-center justify-between border-t border-border pt-6">
            <Label htmlFor="showAddress" className="font-semibold text-foreground">Mostrar endereço para meu cliente?</Label>
            <input 
              id="showAddress" 
              type="checkbox" 
              {...form.register("showAddressToClient")}
              className="relative inline-flex h-6 w-11 shrink-0 cursor-pointer items-center justify-center rounded-full bg-border transition-colors duration-200 ease-in-out focus:outline-none focus:ring-2 focus:ring-primary focus:ring-offset-2 data-[checked=true]:bg-emerald-500 appearance-none after:absolute after:left-0.5 after:top-0.5 after:h-5 after:w-5 after:rounded-full after:bg-white after:transition-all after:content-[''] checked:bg-emerald-500 checked:after:translate-x-5"
            />
          </div>

          <div className="mt-8 flex justify-center gap-4 border-t border-border pt-8">
            <Button 
              variant="ghost" 
              type="button" 
              onClick={() => {
                if (form.formState.isDirty) {
                  const confirm = window.confirm("Deseja mesmo cancelar? Você tem dados alterados não salvos.");
                  if (confirm) router.push("/");
                } else {
                  router.push("/");
                }
              }}
              className="w-32 bg-muted hover:bg-muted/80"
            >
              Cancelar
            </Button>
            <Button 
              variant="primary" 
              type="submit" 
              disabled={updateMutation.isPending}
              className="w-32 bg-emerald-500 hover:bg-emerald-600 border-emerald-500"
            >
              {updateMutation.isPending ? "Salvando..." : "Salvar"}
            </Button>
          </div>
        </form>
      </main>
    </div>
  );
}
