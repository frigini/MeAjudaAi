"use client";

import { useState } from "react";
import { useRouter } from "next/navigation";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Textarea } from "@/components/ui/textarea";
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Plus, Loader2, Trash2 } from "lucide-react";
import { toast } from "sonner";
import { VerifiedBadge } from "@/components/ui/verified-badge";
import { ProviderDto, ServiceDto } from "@/types/api/provider";



const UUID_REGEX = /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i;

interface ProviderDashboardClientProps {
    provider: ProviderDto;
}

export default function ProviderDashboardClient({ provider }: ProviderDashboardClientProps) {
    const router = useRouter();
    const [isEditingDescription, setIsEditingDescription] = useState(false);
    // Description is nested in businessProfile
    const [description, setDescription] = useState(provider.businessProfile?.description || "");
    const [isSavingDescription, setIsSavingDescription] = useState(false);

    const [newServiceId, setNewServiceId] = useState("");
    const [isAddingService, setIsAddingService] = useState(false);
    const [isRemovingService, setIsRemovingService] = useState<string | null>(null);

    const services = provider.services ?? [];

    const handleSaveDescription = async () => {
        setIsSavingDescription(true);
        try {
            const res = await fetch(`/api/providers/me`, {
                method: "PUT",
                headers: {
                    "Content-Type": "application/json",
                },
                body: JSON.stringify({
                    name: provider.name,
                    businessProfile: {
                        ...(provider.businessProfile || {}),
                        description: description
                    }
                })
            });

            if (!res.ok) throw new Error("Failed to update description");

            toast.success("Descrição atualizada com sucesso!");
            setIsEditingDescription(false);
            router.refresh();
        } catch (error) {
            console.error(error);
            toast.error("Erro ao atualizar descrição.");
        } finally {
            setIsSavingDescription(false);
        }
    };


    const handleCancelDescription = () => {
        setDescription(provider.businessProfile?.description || "");
        setIsEditingDescription(false);
    };

    const handleAddService = async () => {
        if (!newServiceId) return;

        if (!UUID_REGEX.test(newServiceId)) {
            toast.error("ID do serviço inválido. Deve ser um UUID.");
            return;
        }

        setIsAddingService(true);
        try {
            // Using local proxy route: /api/providers/[id]/services/[serviceId]
            // We use POST to this route
            const res = await fetch(`/api/providers/${provider.id}/services/${newServiceId}`, {
                method: "POST"
            });

            if (!res.ok) throw new Error("Failed to add service");

            toast.success("Serviço adicionado!");
            setNewServiceId("");
            router.refresh();
        } catch (error) {
            console.error(error);
            toast.error("Erro ao adicionar serviço. Verifique o ID.");
        } finally {
            setIsAddingService(false);
        }
    };

    const handleRemoveService = async (serviceId: string) => {
        if (!confirm("Tem certeza que deseja remover este serviço?")) return;
        setIsRemovingService(serviceId);
        try {
            const res = await fetch(`/api/providers/${provider.id}/services/${serviceId}`, {
                method: "DELETE"
            });

            if (!res.ok) throw new Error("Failed to remove service");

            toast.success("Serviço removido!");
            router.refresh();
        } catch (error) {
            console.error(error);
            toast.error("Erro ao remover serviço.");
        } finally {
            setIsRemovingService(null);
        }
    };

    return (
        <div className="container mx-auto py-8 px-4 max-w-5xl space-y-8">
            {/* Header */}
            <div className="flex flex-col md:flex-row md:items-center justify-between gap-4">
                <div>
                    <h1 className="text-3xl font-bold text-slate-900">Meu Painel</h1>
                    <p className="text-slate-500">Gerencie seu perfil e serviços</p>
                </div>
                <div className="flex items-center gap-3 bg-white p-2 px-4 rounded-full shadow-sm border">
                    <span className="font-semibold text-slate-700">{provider.name}</span>
                    <VerifiedBadge status={provider.verificationStatus ?? "Pending"} />
                </div>
            </div>

            <div className="grid grid-cols-1 lg:grid-cols-3 gap-8">
                {/* Left Column: Profile Info */}
                <div className="lg:col-span-2 space-y-6">

                    {/* Description Card */}
                    <Card>
                        <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
                            <CardTitle className="text-xl font-bold text-slate-800">Sobre Mim</CardTitle>
                            {!isEditingDescription && (
                                <Button variant="ghost" size="sm" onClick={() => setIsEditingDescription(true)}>
                                    Editar
                                </Button>
                            )}
                        </CardHeader>
                        <CardContent className="pt-4">
                            {isEditingDescription ? (
                                <div className="space-y-4">
                                    <Textarea
                                        value={description}
                                        onChange={(e) => setDescription(e.target.value)}
                                        className="min-h-[150px]"
                                        placeholder="Descreva seus serviços e experiência..."
                                    />
                                    <div className="flex justify-end gap-2">
                                        <Button variant="outline" size="sm" onClick={handleCancelDescription} disabled={isSavingDescription}>
                                            Cancelar
                                        </Button>
                                        <Button size="sm" onClick={handleSaveDescription} disabled={isSavingDescription} className="bg-[#E0702B] hover:bg-[#c56225]">
                                            {isSavingDescription && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
                                            Salvar
                                        </Button>
                                    </div>
                                </div>
                            ) : (
                                <p className="text-slate-600 whitespace-pre-wrap leading-relaxed">
                                    {provider.businessProfile?.description || "Nenhuma descrição adicionada."}
                                </p>
                            )}
                        </CardContent>
                    </Card>

                    {/* Services Card */}
                    <Card>
                        <CardHeader>
                            <CardTitle className="text-xl font-bold text-slate-800">Meus Serviços</CardTitle>
                            <CardDescription>Gerencie os serviços que você oferece</CardDescription>
                        </CardHeader>
                        <CardContent className="space-y-6">
                            {/* Add Service */}
                            <div className="flex gap-2">
                                <Input
                                    placeholder="ID do Serviço (UUID)"
                                    value={newServiceId}
                                    onChange={(e) => setNewServiceId(e.target.value)}
                                />
                                <Button onClick={handleAddService} disabled={isAddingService || !newServiceId} className="bg-[#E0702B] hover:bg-[#c56225]">
                                    {isAddingService ? <Loader2 className="h-4 w-4 animate-spin" /> : <Plus className="h-4 w-4 mr-2" />}
                                    Adicionar
                                </Button>
                            </div>

                            {/* Service List */}
                            <div className="space-y-2">
                                {services.length === 0 ? (
                                    <p className="text-slate-500 text-center py-4">Nenhum serviço cadastrado.</p>
                                ) : (
                                    services.map((service) => (
                                        <div key={service.serviceId} className="flex items-center justify-between p-3 bg-slate-50 rounded-lg border">
                                            <div className="flex items-center gap-2">
                                                <Badge variant="secondary" className="bg-white">{service.serviceName}</Badge>
                                            </div>
                                            <Button
                                                variant="ghost"
                                                size="sm"
                                                className="text-red-500 hover:text-red-700 hover:bg-red-50"
                                                onClick={() => service.serviceId && handleRemoveService(service.serviceId)}
                                                disabled={isRemovingService === service.serviceId}
                                            >
                                                {isRemovingService === service.serviceId ? <Loader2 className="h-4 w-4 animate-spin" /> : <Trash2 className="h-4 w-4" />}
                                            </Button>
                                        </div>
                                    ))
                                )}
                            </div>
                        </CardContent>
                    </Card>

                </div>

                {/* Right Column: Status & Stats */}
                <div className="space-y-6">
                    <Card>
                        <CardHeader>
                            <CardTitle className="text-sm font-medium text-slate-500 uppercase tracking-wider">Status da Conta</CardTitle>
                        </CardHeader>
                        <CardContent>
                            <div className="flex items-center gap-2">
                                <VerifiedBadge status={provider.verificationStatus ?? "Pending"} showLabel size="lg" />
                                {provider.verificationStatus === "Pending" && (
                                    <span className="text-yellow-600 font-medium">Pendente de Verificação</span>
                                )}
                            </div>
                            <p className="text-xs text-slate-400 mt-4">
                                ID: {provider.id}
                            </p>
                            <p className="text-xs text-slate-400">
                                Email: {provider.businessProfile?.contactInfo?.email}
                            </p>
                        </CardContent>
                    </Card>
                </div>
            </div>
        </div>
    );
}
