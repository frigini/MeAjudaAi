"use client";

import Link from "next/link";
import { UserCircle, Wrench, ChevronRight } from "lucide-react";

import { Button } from "@/components/ui/button";
import {
    DropdownMenu,
    DropdownMenuContent,
    DropdownMenuItem,
    DropdownMenuLabel,
    DropdownMenuSeparator,
    DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";

export function AuthSelectionDropdown() {
    return (
        <DropdownMenu>
            <DropdownMenuTrigger asChild>
                <Button variant="outline" size="sm">
                    Cadastre-se grátis
                </Button>
            </DropdownMenuTrigger>
            <DropdownMenuContent align="end" className="w-80 p-0">
                <DropdownMenuLabel className="px-5 pt-5 pb-1 text-center">
                    <span className="block text-lg font-semibold text-secondary">Cadastre-se Grátis</span>
                    <span className="block text-sm font-normal text-muted-foreground mt-0.5">
                        Escolha a melhor opção pra você
                    </span>
                </DropdownMenuLabel>

                <div className="p-3 space-y-1">
                    <DropdownMenuItem asChild className="cursor-pointer rounded-lg p-0">
                        <Link href="/cadastro/cliente" className="flex items-center gap-4 px-4 py-4">
                            <UserCircle className="h-7 w-7 text-primary shrink-0" />
                            <div className="flex flex-col flex-1 min-w-0">
                                <span className="text-base font-semibold">Quero ser cliente</span>
                                <span className="text-sm text-muted-foreground">
                                    Encontre os melhores profissionais disponíveis para você.
                                </span>
                            </div>
                            <ChevronRight className="h-5 w-5 text-muted-foreground shrink-0" />
                        </Link>
                    </DropdownMenuItem>

                    <DropdownMenuSeparator />

                    <DropdownMenuItem asChild className="cursor-pointer rounded-lg p-0">
                        <Link href="/cadastro/prestador" className="flex items-center gap-4 px-4 py-4">
                            <Wrench className="h-7 w-7 text-secondary shrink-0" />
                            <div className="flex flex-col flex-1 min-w-0">
                                <span className="text-base font-semibold">Sou prestador</span>
                                <span className="text-sm text-muted-foreground">
                                    Divulgue seus serviços para milhares de clientes.
                                </span>
                            </div>
                            <ChevronRight className="h-5 w-5 text-muted-foreground shrink-0" />
                        </Link>
                    </DropdownMenuItem>
                </div>
            </DropdownMenuContent>
        </DropdownMenu>
    );
}
