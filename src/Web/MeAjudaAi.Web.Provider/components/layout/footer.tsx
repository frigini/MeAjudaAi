"use client";

import { Mail, Phone, Instagram } from "lucide-react";
import { useTranslation } from "react-i18next";

export interface FooterProps {
    className?: string;
}

export function Footer({ className }: FooterProps) {
    const { t } = useTranslation("common");

    return (
        <footer className={`bg-secondary text-white ${className || ''}`}>
            <div className="container mx-auto px-4 py-12">
                <div className="grid grid-cols-1 md:grid-cols-4 gap-8">
                    {/* Missão */}
                    <div>
                        <h3 className="font-bold text-lg mb-4">{t("mission")}</h3>
                        <p className="text-sm text-white/90">
                            {t("mission_text")}
                        </p>
                    </div>

                    {/* Visão */}
                    <div>
                        <h3 className="font-bold text-lg mb-4">{t("vision")}</h3>
                        <p className="text-sm text-white/90">
                            {t("vision_text")}
                        </p>
                    </div>

                    {/* Valores */}
                    <div>
                        <h3 className="font-bold text-lg mb-4">{t("values")}</h3>
                        <p className="text-sm text-white/90">
                            {t("values_text")}
                        </p>
                    </div>

                    {/* Contatos */}
                    <div>
                        <h3 className="font-bold text-lg mb-4">{t("contacts")}</h3>
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
                                {t("privacy_policy")}
                            </p>
                        </div>
                    </div>
                </div>

                <div className="mt-8 pt-8 border-t border-white/20 text-center text-sm">
                    © {new Date().getFullYear()} AjudaAi. {t("copyright")}
                </div>
            </div>
        </footer>
    );
}
