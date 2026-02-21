"use client";

import Link from "next/link";
import { User, Briefcase } from "lucide-react";

import { Button } from "@/components/ui/button";
import {
    DropdownMenu,
    DropdownMenuContent,
    DropdownMenuItem,
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
            <DropdownMenuContent align="end" className="w-56">
                <DropdownMenuItem asChild className="cursor-pointer">
                    <Link href="/cadastro/cliente" className="flex items-center gap-2 py-2">
                        <User className="h-4 w-4 text-primary" />
                        <div className="flex flex-col">
                            <span className="font-medium">Quero ser cliente</span>
                            <span className="text-xs text-muted-foreground">
                                Contratar serviços
                            </span>
                        </div>
                    </Link>
                </DropdownMenuItem>
                <DropdownMenuItem asChild className="cursor-pointer">
                    <Link href="/cadastro/prestador" className="flex items-center gap-2 py-2">
                        <Briefcase className="h-4 w-4 text-primary" />
                        <div className="flex flex-col">
                            <span className="font-medium">Sou prestador</span>
                            <span className="text-xs text-muted-foreground">
                                Oferecer serviços
                            </span>
                        </div>
                    </Link>
                </DropdownMenuItem>
            </DropdownMenuContent>
        </DropdownMenu>
    );
}
