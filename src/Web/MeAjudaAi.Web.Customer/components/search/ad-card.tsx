import { Button } from "@/components/ui/button";

export function AdCard() {
    return (
        <div className="h-full min-h-[300px] rounded-xl border border-border bg-[#FFF8F3] p-6 flex flex-col justify-between relative overflow-hidden group">
            <div className="flex flex-col gap-2 z-10">
                <span className="text-xs font-medium text-foreground-subtle uppercase tracking-wider">Publicidade</span>
                <h3 className="text-lg font-bold text-orange-600 leading-tight">
                    A frase é batida mas...<br />
                    O SEU CLIENTE TAMBÉM VIU ESTE ANÚNCIO!
                </h3>
            </div>

            <div className="absolute right-[-20px] top-[40%] h-24 w-24 rounded-full bg-orange-600/90 blur-sm transform rotate-12 group-hover:scale-110 transition-transform duration-500"></div>

            <div className="mt-auto z-10">
                <Button variant="link" className="text-foreground-subtle hover:text-orange-600 p-0 h-auto font-medium">
                    Anuncie aqui!
                </Button>
            </div>
        </div>
    );
}
