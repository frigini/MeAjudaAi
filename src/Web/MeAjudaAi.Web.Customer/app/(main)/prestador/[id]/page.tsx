"use client";

import { useSession } from "next-auth/react";
import { useParams } from "next/navigation";
import { useQuery } from "@tanstack/react-query";
import React, { useState } from "react";
import Link from "next/link";
import { Button } from "@/components/ui/button";
import { Avatar } from "@/components/ui/avatar";
import { Rating } from "@/components/ui/rating";
import { ReviewList } from "@/components/reviews/review-list";
import { ReviewForm } from "@/components/reviews/review-form";
import { Badge } from "@/components/ui/badge";
import { MessageCircle, Loader2, MapPin } from "lucide-react";
import { VerifiedBadge } from "@/components/ui/verified-badge";
import { BookingModal } from "@/components/bookings/booking-modal";
import { z } from "zod";
import { getWhatsappLink } from "@/lib/utils/phone";
import { EProviderType } from "@/types/api/provider";
import { normalizeProviderType } from "@/lib/utils/normalization";
import { VerificationStatusSchema } from "@/lib/schemas/verification-status";

const PublicProviderSchema = z.object({
    id: z.string().uuid(),
    name: z.string(),
    type: z.preprocess(normalizeProviderType, z.nativeEnum(EProviderType).optional().default(EProviderType.None)),
    fantasyName: z.string().optional().nullable(),
    description: z.string().optional().nullable(),
    city: z.string().optional().nullable(),
    state: z.string().optional().nullable(),
    rating: z.number().optional().nullable(),
    reviewCount: z.number().optional().nullable(),
    phoneNumbers: z.array(z.string()).optional().nullable(),
    services: z.array(z.object({ id: z.string().uuid(), name: z.string() })).optional().nullable(),
    email: z.string().email().optional().nullable(),
    verificationStatus: VerificationStatusSchema
});

export default function ProviderProfilePage() {
    const { id } = useParams() as { id: string };
    const { data: session, status } = useSession();
    
    const isAuthenticated = status === "authenticated";
    const isLoadingAuth = status === "loading";
    const [selectedServiceId, setSelectedServiceId] = useState<string>("");

    const { data: providerData, isLoading, error } = useQuery({
        queryKey: ["public-provider", id],
        queryFn: async () => {
            const apiUrl = process.env.NEXT_PUBLIC_API_URL;
            const headers: Record<string, string> = {};
            
            if (session?.accessToken) {
                headers["Authorization"] = `Bearer ${session.accessToken}`;
            }

            const res = await fetch(`${apiUrl}/api/v1/providers/${id}/public`, {
                headers
            });

            if (res.status === 404) return null;
            if (!res.ok) throw new Error(`Failed to fetch provider: ${res.statusText}`);

            const json = await res.json();
            const dataToValidate = (json && typeof json === "object") 
                ? ("data" in json ? json.data : ("value" in json ? json.value : json))
                : json;
            
            const result = PublicProviderSchema.safeParse(dataToValidate);
            if (!result.success) throw new Error(`Invalid provider data: ${result.error.message}`);
            
            return result.data;
        },
        enabled: !!id,
    });

    if (isLoading) {
        return (
            <div className="flex flex-col items-center justify-center min-h-[60vh]">
                <Loader2 className="h-12 w-12 animate-spin text-primary mb-4" />
                <p className="text-muted-foreground">Carregando perfil do profissional...</p>
            </div>
        );
    }

    if (error || !providerData) {
        return (
            <div className="container mx-auto py-20 text-center">
                <h1 className="text-2xl font-bold text-destructive">Profissional não encontrado</h1>
                <p className="mt-2 text-muted-foreground">Não foi possível carregar os dados deste profissional no momento.</p>
                <Button asChild className="mt-4">
                    <Link href="/buscar">Voltar para busca</Link>
                </Button>
            </div>
        );
    }

    const displayName = providerData.fantasyName || providerData.name || "Prestador";
    const description = providerData.description || "Este prestador ainda não adicionou uma descrição.";
    const rating = providerData.rating ?? 0;
    const reviewCount = providerData.reviewCount ?? 0;
    const phones = providerData.phoneNumbers || [];
    const services = providerData.services ?? [];

    return (
        <div className="container mx-auto px-4 py-8">
            <div className="max-w-4xl mx-auto">
                <div className="grid grid-cols-1 md:grid-cols-12 gap-8 mb-12">
                    <div className="md:col-span-3 flex flex-col items-center space-y-4">
                        <Avatar
                            src={undefined}
                            alt={displayName}
                            fallback={displayName.substring(0, 2).toUpperCase()}
                            containerClassName="h-32 w-32 border-4 border-white shadow-md text-3xl font-bold"
                        />
                        
                        {/* Botão de Agendamento */}
                        <div className="w-full pt-2">
                            {isLoadingAuth ? (
                                <Button disabled className="w-full bg-slate-200 text-slate-500 py-6 text-lg">
                                    <Loader2 className="h-4 w-4 animate-spin mr-2" /> Carregando...
                                </Button>
                            ) : isAuthenticated ? (
                                services.length > 0 ? (
                                    <BookingModal 
                                        providerId={id} 
                                        providerName={displayName} 
                                        serviceId={selectedServiceId}
                                        trigger={
                                            <Button 
                                                disabled={!selectedServiceId}
                                                className="w-full bg-[#E0702B] hover:bg-[#C55A1F] text-white font-bold py-6 text-lg shadow-lg transition-all hover:scale-[1.02] active:scale-[0.98]"
                                            >
                                                {selectedServiceId ? "Agendar Horário" : "Selecione um Serviço"}
                                            </Button>
                                        }
                                    />
                                ) : (
                                    <Button disabled className="w-full bg-slate-200 text-slate-500 py-6 text-lg">
                                        Nenhum serviço disponível
                                    </Button>
                                )
                            ) : (
                                <Button asChild className="w-full bg-[#E0702B] hover:bg-[#C55A1F] text-white font-bold py-6 text-lg">
                                    <Link href={`/api/auth/signin?callbackUrl=${encodeURIComponent(`/prestador/${id}`)}`}>Entrar para Agendar</Link>
                                </Button>
                            )}
                        </div>

                        <div className="flex items-center gap-2 pt-2">
                            <Rating value={rating} className="text-[#E0702B]" />
                            {reviewCount > 0 && (
                                <span className="text-sm text-gray-600">({reviewCount} avaliações)</span>
                            )}
                        </div>

                        {phones.length > 0 ? (
                            <div className="w-full space-y-2">
                                {phones.map((phone: string, i: number) => {
                                    const whatsappLink = getWhatsappLink(phone);
                                    return (
                                        <div key={i} className="flex items-center gap-2 text-gray-600 text-sm">
                                            <span className="font-medium">{phone}</span>
                                            {whatsappLink && (
                                                <a
                                                    href={whatsappLink}
                                                    target="_blank"
                                                    rel="noopener noreferrer"
                                                    className="text-green-500 hover:text-green-600 transition-colors"
                                                    title="Chamar no WhatsApp"
                                                >
                                                    <MessageCircle className="w-4 h-4" />
                                                </a>
                                            )}
                                        </div>
                                    );
                                })}
                            </div>
                        ) : isAuthenticated ? (
                            <div className="w-full p-4 bg-blue-50 border border-blue-100 rounded-lg text-center">
                                <p className="text-sm text-gray-700">Este prestador não informou contatos.</p>
                            </div>
                        ) : !isLoadingAuth && (
                            <div className="w-full p-4 bg-orange-50 border border-orange-100 rounded-lg text-center">
                                <p className="text-sm text-gray-700 mb-2">Faça login para visualizar os contatos deste prestador.</p>
                                <Link
                                    href={`/api/auth/signin?callbackUrl=${encodeURIComponent(`/prestador/${id}`)}`}
                                    className="text-sm font-bold text-[#E0702B] hover:underline"
                                >
                                    Fazer Login
                                </Link>
                            </div>
                        )}
                    </div>

                    <div className="md:col-span-9 space-y-4">
                        <div className="flex items-center gap-3">
                            <h1 className="text-3xl md:text-4xl font-bold text-[#E0702B]">{displayName}</h1>
                            <VerifiedBadge status={providerData.verificationStatus ?? undefined} size="lg" />
                        </div>

                        {providerData.email && (
                            <p className="text-gray-500 font-medium text-sm lowercase">{providerData.email}</p>
                        )}
                        
                        <div className="flex items-center gap-2 text-muted-foreground">
                            <MapPin className="h-4 w-4" />
                            <span>{providerData.city} - {providerData.state}</span>
                        </div>

                        <div className="text-gray-600 leading-relaxed text-justify">
                            <p>{description}</p>
                        </div>

                        {services.length > 0 && (
                            <div className="pt-4">
                                <h2 className="text-lg font-bold text-gray-900 mb-3">Serviços</h2>
                                <div className="flex flex-wrap gap-2">
                                    {services.map((service) => (
                                        <Badge
                                            key={service.id}
                                            role="button"
                                            tabIndex={0}
                                            onClick={() => setSelectedServiceId(service.id)}
                                            onKeyDown={(e) => {
                                                if (e.key === "Enter" || e.key === " ") {
                                                    e.preventDefault();
                                                    setSelectedServiceId(service.id);
                                                }
                                            }}
                                            className={`px-3 py-1 cursor-pointer transition-colors text-sm rounded-full ${
                                                selectedServiceId === service.id 
                                                    ? "bg-[#002D62] text-white ring-2 ring-offset-1 ring-[#002D62]" 
                                                    : "hover:border-[#E0702B] hover:bg-[#E0702B]/5 text-gray-700"
                                            }`}
                                        >
                                            {service.name}
                                        </Badge>
                                    ))}
                                </div>
                                {!selectedServiceId && isAuthenticated && (
                                    <p className="text-xs text-orange-600 mt-2 font-medium animate-pulse">
                                        * Clique em um serviço acima para agendar
                                    </p>
                                )}
                            </div>
                        )}
                    </div>
                </div>
            </div>

            <div className="pt-8 border-t border-gray-200">
                <div className="flex items-center justify-between mb-6">
                    <h2 className="text-xl font-bold text-gray-900">Comentários</h2>
                </div>

                <div className="space-y-6">
                    <div className="bg-gray-50 p-3 rounded-lg">
                        <ReviewForm providerId={id} />
                    </div>
                    <ReviewList providerId={id} />
                </div>
            </div>
        </div>
    );
}
