import { Megaphone } from "lucide-react";

export function AdBanner() {
    return (
        <div className="w-full bg-[#F2E8DF] border-b border-border py-4 text-center overflow-hidden">
            <div className="container mx-auto px-4 flex items-center justify-center gap-2 text-sm text-secondary">
                <Megaphone className="h-4 w-4 text-secondary" />
                <span className="font-medium">Anuncie aqui! Ganhe a visibilidade que precisa na sua empresa</span>
            </div>
        </div>
    );
}
