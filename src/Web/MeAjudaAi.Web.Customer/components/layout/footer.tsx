import { Mail, Phone, Instagram } from "lucide-react";
import { cn } from "@/lib/utils/cn";

export interface FooterProps {
    className?: string;
}

export function Footer({ className }: FooterProps) {
    return (
        <footer className={cn("bg-secondary text-white", className)}>
            <div className="container mx-auto px-4 py-12">
                <div className="grid grid-cols-1 md:grid-cols-4 gap-8">
                    {/* Missão */}
                    <div>
                        <h3 className="font-bold text-lg mb-4">Missão</h3>
                        <p className="text-sm text-white/90">
                            Nossa missão é tornar a busca por profissionais de confiança
                            mais simples e segura. Conectamos quem precisa com quem sabe
                            fazer, facilitando o acesso a serviços de qualidade com base em
                            avaliações reais e transparentes.
                        </p>
                    </div>

                    {/* Visão */}
                    <div>
                        <h3 className="font-bold text-lg mb-4">Visão</h3>
                        <p className="text-sm text-white/90">
                            Tornar-se a maior plataforma de conexão de prestadores de
                            serviços do Brasil. Facilitamos a contratação de profissionais
                            autônomos, promovendo a economia local e tornando a vida das
                            pessoas mais prática.
                        </p>
                    </div>

                    {/* Valores */}
                    <div>
                        <h3 className="font-bold text-lg mb-4">Valores</h3>
                        <p className="text-sm text-white/90">
                            Responsabilidade, ética, seriedade, excelência, segurança e
                            respeito ao próximo. Acreditamos em um mercado justo e
                            transparente para todos.
                        </p>
                    </div>

                    {/* Contatos */}
                    <div>
                        <h3 className="font-bold text-lg mb-4">Contatos</h3>
                        <div className="flex flex-col gap-3">
                            <a
                                href="mailto:contato@ajudaai.com"
                                className="flex items-center gap-2 text-sm hover:underline"
                            >
                                <Mail className="size-4" />
                                contato@ajudaai.com
                            </a>
                            <a
                                href="tel:+5511999999999"
                                className="flex items-center gap-2 text-sm hover:underline"
                            >
                                <Phone className="size-4" />
                                (11) 99999-9999
                            </a>
                            <a
                                href="https://instagram.com/ajudaai"
                                target="_blank"
                                rel="noopener noreferrer"
                                className="flex items-center gap-2 text-sm hover:underline"
                            >
                                <Instagram className="size-4" />
                                @ajudaai
                            </a>
                        </div>

                        <div className="mt-6">
                            <p className="text-xs font-semibold mb-1">
                                Política de Privacidade
                            </p>
                            {/* TODO: Add link to privacy policy page when it exists */}
                        </div>
                    </div>
                </div>

                <div className="mt-8 pt-8 border-t border-white/20 text-center text-sm">
                    © {new Date().getFullYear()} AjudaAi. Todos os direitos reservados.
                </div>
            </div>
        </footer>
    );
}
