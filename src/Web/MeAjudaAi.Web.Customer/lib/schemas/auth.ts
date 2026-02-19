import * as z from "zod";
import { EProviderType } from "@/types/provider";

export const registerProviderSchema = z.object({
    name: z.string().min(2, "Nome deve ter pelo menos 2 caracteres"),
    documentNumber: z.string()
        .min(11, "Documento inválido")
        .max(14, "Documento inválido")
        .regex(/^\d+$/, "Apenas números são permitidos"), // CPF 11, CNPJ 14 (sem formatação)
    phoneNumber: z.string().min(10, "Telefone inválido"),
    type: z.nativeEnum(EProviderType),
    email: z.string().email("Email inválido").optional().or(z.literal("")),
}).superRefine((data, ctx) => {
    if (data.type === EProviderType.Individual || data.type === EProviderType.Freelancer) {
        // CPF must be 11 digits
        if (data.documentNumber.length !== 11) {
            ctx.addIssue({
                code: z.ZodIssueCode.custom,
                message: "CPF deve ter 11 dígitos",
                path: ["documentNumber"],
            });
        }
    } else if (data.type === EProviderType.Company || data.type === EProviderType.Cooperative) {
        // CNPJ must be 14 digits
        if (data.documentNumber.length !== 14) {
            ctx.addIssue({
                code: z.ZodIssueCode.custom,
                message: "CNPJ deve ter 14 dígitos",
                path: ["documentNumber"],
            });
        }
    }
});

export const addressSchema = z.object({
    zipCode: z.string().min(8, "CEP inválido").max(9, "CEP inválido").regex(/^\d{5}-?\d{3}$/, "Formato inválido (00000-000)"),
    street: z.string().min(3, "Rua obrigatória"),
    number: z.string().min(1, "Número obrigatório"),
    complement: z.string().optional(),
    neighborhood: z.string().min(2, "Bairro obrigatório"),
    city: z.string().min(2, "Cidade obrigatória"),
    state: z.string().length(2, "Estado deve ter 2 letras"),
});

export const registerCustomerSchema = z.object({
    name: z.string().min(2, "Nome deve ter pelo menos 2 caracteres"),
    email: z.string().email("Email inválido"),
    phoneNumber: z.string().min(10, "Telefone inválido"),
    password: z.string().min(6, "Senha deve ter pelo menos 6 caracteres"),
    confirmPassword: z.string(),
    termsAccepted: z.boolean().refine((val) => val === true, "Você deve aceitar os termos de uso"),
}).refine((data) => data.password === data.confirmPassword, {
    message: "As senhas não conferem",
    path: ["confirmPassword"],
});

export type RegisterCustomerSchema = z.infer<typeof registerCustomerSchema>;
export type RegisterProviderSchema = z.infer<typeof registerProviderSchema>;
export type AddressSchema = z.infer<typeof addressSchema>;
