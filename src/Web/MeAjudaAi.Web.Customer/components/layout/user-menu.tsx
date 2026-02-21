'use client';

import { useSession, signIn, signOut } from "next-auth/react";
import { Button } from "@/components/ui/button";
import { Avatar } from "@/components/ui/avatar";
import {
    DropdownMenu,
    DropdownMenuContent,
    DropdownMenuItem,
    DropdownMenuLabel,
    DropdownMenuSeparator,
    DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import { User, LogOut, Briefcase } from "lucide-react";
import Link from "next/link";
import { useProviderStatus } from "@/hooks/use-provider-status";
import { EProviderStatus, PROVIDER_STATUS_LABELS, PROVIDER_TIER_LABELS } from "@/types/provider";
import { AuthSelectionDropdown } from "@/components/auth/auth-selection-dropdown";

export function UserMenu() {
    const { data: session, status } = useSession();
    const { data: providerStatus, isLoading: isLoadingProvider } = useProviderStatus();
    // Fail-safe: Show buttons by default (avoids infinite loading if JS fails)
    // If authenticated, we show the avatar.
    // Loading state - prevent flash of unauthenticated UI
    if (status === "loading") {
        return (
            <div className="h-10 w-10 rounded-full bg-secondary/20 animate-pulse" />
        );
    }

    // Check for session errors (e.g. RefreshAccessTokenError)
    if (session?.user) {
        if ((session as { error?: string }).error) {
            // Force sign out if token is invalid
            void signOut({ callbackUrl: "/" });
            return null; // Don't render anything while signing out
        }

        return (

            <DropdownMenu>
                <DropdownMenuTrigger asChild>
                    <Button variant="ghost" className="relative h-10 w-10 rounded-full">
                        <Avatar
                            src={session?.user?.image}
                            alt={session?.user?.name ?? "Usuário"}
                        />
                    </Button>
                </DropdownMenuTrigger>
                <DropdownMenuContent className="w-56" align="end" forceMount>
                    <DropdownMenuLabel className="font-normal">
                        <div className="flex flex-col space-y-1">
                            <p className="text-sm font-medium leading-none">{session?.user?.name ?? "Usuário"}</p>
                            <p className="text-xs leading-none text-muted-foreground">
                                {session?.user?.email ?? "—"}
                            </p>
                        </div>
                    </DropdownMenuLabel>
                    <DropdownMenuSeparator />
                    <DropdownMenuItem asChild>
                        <Link href="/perfil">
                            <User className="mr-2 h-4 w-4" />
                            <span>Meu Perfil</span>
                        </Link>
                    </DropdownMenuItem>

                    {/* Show provider details when loaded and exists; otherwise render 'Quero trabalhar' fallback */}
                    {!isLoadingProvider && <DropdownMenuSeparator />}

                    {isLoadingProvider ? (
                        // Loading state handled or just suppress
                        null
                    ) : providerStatus ? (
                        <>
                            <DropdownMenuLabel className="font-normal">
                                <div className="flex justify-between items-center">
                                    <span className="text-sm font-medium">Conta Prestador</span>
                                    <span className="text-xs bg-primary/10 text-primary px-2 py-0.5 rounded-full capitalize border border-primary/20">
                                        {PROVIDER_TIER_LABELS[providerStatus.tier] ?? "Desconhecido"}
                                    </span>
                                </div>
                                <p className="text-xs text-muted-foreground mt-1">
                                    Status: <span className={
                                        providerStatus.status === EProviderStatus.Active ? "text-green-600 font-medium" :
                                            (providerStatus.status === EProviderStatus.Rejected || providerStatus.status === EProviderStatus.Suspended) ? "text-red-600 font-medium" :
                                                "text-amber-600"
                                    }>
                                        {PROVIDER_STATUS_LABELS[providerStatus.status] ?? "Desconhecido"}
                                    </span>
                                </p>
                            </DropdownMenuLabel>
                            <DropdownMenuItem asChild>
                                <Link href={providerStatus.status === EProviderStatus.Active ? "/prestador/dashboard" : "/cadastro/prestador/perfil"}>
                                    <Briefcase className="mr-2 h-4 w-4" />
                                    <span>{providerStatus.status === EProviderStatus.Active ? "Painel do Prestador" : "Continuar Cadastro"}</span>
                                </Link>
                            </DropdownMenuItem>
                        </>
                    ) : (
                        <DropdownMenuItem asChild>
                            <Link href="/cadastro/prestador">
                                <Briefcase className="mr-2 h-4 w-4" />
                                <span>Quero trabalhar</span>
                            </Link>
                        </DropdownMenuItem>
                    )}
                    <DropdownMenuSeparator />
                    <DropdownMenuItem onClick={() => signOut({ callbackUrl: "/" })}>
                        <LogOut className="mr-2 h-4 w-4" />
                        <span>Sair</span>
                    </DropdownMenuItem>
                </DropdownMenuContent>
            </DropdownMenu>
        );
    }

    // Default view: Unauthenticated / Loading / Error
    return (
        <div className="flex items-center gap-3">
            <AuthSelectionDropdown />
            <Button variant="secondary" size="sm" onClick={() => signIn("keycloak")}>
                Login
            </Button>
        </div>
    );
}
