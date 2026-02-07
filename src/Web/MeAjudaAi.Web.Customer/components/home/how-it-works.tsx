"use client";

import { useState } from "react";
import Image from "next/image";
import { ChevronDown, ChevronUp } from "lucide-react";
import { Button } from "@/components/ui/button";

export function HowItWorks() {
    const [isOpen, setIsOpen] = useState(false);

    return (
        <div className="mb-4">
            <button
                onClick={() => setIsOpen(!isOpen)}
                className="flex items-center gap-2 group focus:outline-none focus-visible:ring-2 focus-visible:ring-ring rounded-lg"
                aria-expanded={isOpen}
            >
                <h2 className="text-xl font-bold text-orange-500">Como funciona?</h2>
                {isOpen ? (
                    <ChevronUp className="h-6 w-6 text-orange-500 transition-transform group-hover:scale-110" />
                ) : (
                    <ChevronDown className="h-6 w-6 text-orange-500 transition-transform group-hover:scale-110" />
                )}
            </button>
            <p className="text-foreground-subtle text-sm mt-1 mb-4 max-w-2xl">
                Expanda esse menu para conhecer o processo de solicitação do serviço, e tenha dicas de segurança
            </p>

            <div
                className={`transition-all duration-300 ease-in-out overflow-hidden ${isOpen ? "max-h-[2000px] opacity-100" : "max-h-0 opacity-0"
                    }`}
            >
                <div className="grid grid-cols-1 md:grid-cols-2 gap-8 md:gap-16 pb-4">
                    {/* Step 1 */}
                    <div className="flex flex-col gap-4">
                        <div className="relative h-64 w-full flex items-center justify-center">
                            <Image
                                src="/assets/illustrations/how-it-works-1.png"
                                alt="Ilustração: Cadastro e Busca"
                                fill
                                className="object-contain"
                                sizes="(max-width: 768px) 100vw, (max-width: 1200px) 50vw, 33vw"
                            />
                        </div>
                        <h3 className="text-foreground font-semibold">
                            1 - Faça seu cadastro como Cliente e depois busque pelo serviço desejado
                        </h3>
                    </div>

                    {/* Step 2 */}
                    <div className="flex flex-col gap-4 md:mt-16">
                        <div className="relative h-64 w-full flex items-center justify-center">
                            <Image
                                src="/assets/illustrations/how-it-works-2.png"
                                alt="Ilustração: Encontrar Prestador"
                                fill
                                className="object-contain"
                                sizes="(max-width: 768px) 100vw, (max-width: 1200px) 50vw, 33vw"
                            />
                        </div>
                        <h3 className="text-foreground font-semibold">
                            2 - Encontre um prestador que tenha recomendações, como o site ainda está no começo, se ligue na próxima dica
                        </h3>
                    </div>

                    {/* Step 3 */}
                    <div className="flex flex-col gap-4">
                        <div className="relative h-64 w-full flex items-center justify-center">
                            <Image
                                src="/assets/illustrations/how-it-works-3.png"
                                alt="Ilustração: Contato e Pagamento"
                                fill
                                className="object-contain"
                                sizes="(max-width: 768px) 100vw, (max-width: 1200px) 50vw, 33vw"
                            />
                        </div>
                        <h3 className="text-foreground font-semibold">
                            3 - Com o contato do prestador em mãos, envie uma mensagem ou faça uma ligação, peça indícios dos serviços já prestados e combine o valor, prazo, dia e horário para a realização do serviço. Ah! Nunca pague antes do serviço prestado, combine sempre uma forma justa para vocês dois.
                        </h3>
                    </div>

                    {/* Step 4 */}
                    <div className="flex flex-col gap-4 md:mt-16">
                        <div className="relative h-64 w-full flex items-center justify-center">
                            <Image
                                src="/assets/illustrations/how-it-works-4.png"
                                alt="Ilustração: Avaliação"
                                fill
                                className="object-contain"
                                sizes="(max-width: 768px) 100vw, (max-width: 1200px) 50vw, 33vw"
                            />
                        </div>
                        <h3 className="text-foreground font-semibold">
                            4 - E o mais importante, depois do serviço prestado, faça uma avaliação do prestador de serviço, diga como foi sua experiência e ajude os usuários futuros a saberem quem procurar.
                        </h3>
                    </div>

                    <div className="col-span-1 md:col-span-2 flex justify-center mt-4 mb-4">
                        <Button
                            variant="ghost"
                            onClick={() => setIsOpen(false)}
                            className="text-orange-500 hover:text-orange-600 hover:bg-orange-50 gap-2 font-medium"
                        >
                            Esconder
                            <span aria-hidden="true">👁️</span>
                        </Button>
                    </div>
                </div>
            </div>

            <div className="w-full h-px bg-orange-200 mt-4 mb-0"></div>
        </div>
    );
}
