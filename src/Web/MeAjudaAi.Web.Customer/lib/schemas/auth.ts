import * as z from "zod";
import { EProviderType } from "@/types/provider";

// CPF validation helper
function isValidCpf(cpf: string): boolean {
    if (!cpf || cpf.length !== 11 || /^(\d)\1{10}$/.test(cpf)) {
        return false;
    }

    let sum = 0;
    let remainder: number;

    // First check digit
    for (let i = 1; i <= 9; i++) {
        sum += parseInt(cpf.substring(i - 1, i)) * (11 - i);
    }

    remainder = (sum * 10) % 11;
    if (remainder === 10 || remainder === 11) remainder = 0;
    if (remainder !== parseInt(cpf.substring(9, 10))) return false;

    sum = 0;

    // Second check digit
    for (let i = 1; i <= 10; i++) {
        sum += parseInt(cpf.substring(i - 1, i)) * (12 - i);
    }

    remainder = (sum * 10) % 11;
    if (remainder === 10 || remainder === 11) remainder = 0;
    if (remainder !== parseInt(cpf.substring(10, 11))) return false;

    return true;
}

// CNPJ validation helper
function isValidCnpj(cnpj: string): boolean {
    if (!cnpj || cnpj.length !== 14 || /^(\d)\1{13}$/.test(cnpj)) {
        return false;
    }

    let sum = 0;
    let remainder: number;

    // First check digit
    let multiplier = 5;
    for (let i = 0; i < 12; i++) {
        sum += parseInt(cnpj[i]) * multiplier;
        multiplier = multiplier === 2 ? 9 : multiplier - 1;
    }

    remainder = sum % 11;
    if (remainder < 2) remainder = 0;
    else remainder = 11 - remainder;

    if (remainder !== parseInt(cnpj[12])) return false;

    sum = 0;
    multiplier = 6;

    // Second check digit
    for (let i = 0; i < 13; i++) {
        sum += parseInt(cnpj[i]) * multiplier;
        multiplier = multiplier === 2 ? 9 : multiplier - 1;
    }

    remainder = sum % 11;
    if (remainder < 2) remainder = 0;
    else remainder = 11 - remainder;

    if (remainder !== parseInt(cnpj[13])) return false;

    return true;
}

export const registerProviderSchema = z.object({
    name: z.string().min(2, "Nome deve ter pelo menos 2 caracteres"),
    documentNumber: z.string()
        .min(11, "Documento inválido")
        .max(14, "Documento inválido")
        .regex(/^\d+$/, "Apenas números são permitidos"), // CPF 11, CNPJ 14 (sem formatação)
    phoneNumber: z.string().min(10, "Telefone inválido (mínimo 10 dígitos)").max(11, "Telefone inválido (máximo 11 dígitos)").regex(/^\d+$/, "Apenas dígitos são permitidos"),
    type: z.nativeEnum(EProviderType),
    email: z.string().email("Email inválido"), // Now strictly required
    acceptedTerms: z.boolean().refine(v => v === true, "Você deve aceitar os termos de uso"),
    acceptedPrivacyPolicy: z.boolean().refine(v => v === true, "Você deve aceitar a política de privacidade"),
}).superRefine((data, ctx) => {
    if (data.type === EProviderType.None) {
        ctx.addIssue({
            code: z.ZodIssueCode.custom,
            message: "Tipo de provedor inválido",
            path: ["type"],
        });
    } else if (data.type === EProviderType.Individual || data.type === EProviderType.Freelancer) {
        // CPF must be 11 digits
        if (data.documentNumber.length !== 11) {
            ctx.addIssue({
                code: z.ZodIssueCode.custom,
                message: "CPF deve ter 11 dígitos",
                path: ["documentNumber"],
            });
        } else if (!isValidCpf(data.documentNumber)) {
            ctx.addIssue({
                code: z.ZodIssueCode.custom,
                message: "CPF inválido",
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
        } else if (!isValidCnpj(data.documentNumber)) {
            ctx.addIssue({
                code: z.ZodIssueCode.custom,
                message: "CNPJ inválido",
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
    phoneNumber: z.string().min(10, "Telefone inválido (mínimo 10 dígitos)").max(11, "Telefone inválido (máximo 11 dígitos)").regex(/^\d+$/, "Apenas dígitos são permitidos"),
    password: z.string()
        .min(8, "Senha deve ter pelo menos 8 caracteres")
        .regex(/[a-zA-Z]/, "A senha deve conter pelo menos uma letra")
        .regex(/[0-9]/, "A senha deve conter pelo menos um número")
        .regex(/[^a-zA-Z0-9]/, "A senha deve conter pelo menos um caractere especial"),
    confirmPassword: z.string(),
    acceptedTerms: z.boolean().refine((val) => val === true, "Você deve aceitar os termos de uso"),
    acceptedPrivacyPolicy: z.boolean().refine((val) => val === true, "Você deve aceitar a política de privacidade"),
}).refine((data) => data.password === data.confirmPassword, {
    message: "As senhas não conferem",
    path: ["confirmPassword"],
});

export type RegisterCustomerSchema = z.infer<typeof registerCustomerSchema>;
export type RegisterProviderSchema = z.infer<typeof registerProviderSchema>;
export type AddressSchema = z.infer<typeof addressSchema>;
