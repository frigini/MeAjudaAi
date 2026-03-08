import { VerificationStatusSchema } from "@/lib/schemas/verification-status";
import { EVerificationStatus } from "@/types/api/provider";

/**
 * Suite de testes para a conversão de status de verificação.
 * Lançará um erro se qualquer caso falhar, interrompendo o build/CI.
 */
export const testVerificationStatusConversion = () => {
    const cases = [
        { input: 0, expected: EVerificationStatus.None },
        { input: 1, expected: EVerificationStatus.Pending },
        { input: "0", expected: EVerificationStatus.None },
        { input: "1", expected: EVerificationStatus.Pending },
        { input: "verified", expected: EVerificationStatus.Verified },
        { input: "REJECTED", expected: EVerificationStatus.Rejected },
        { input: "inprogress", expected: EVerificationStatus.InProgress },
        { input: "in_progress", expected: EVerificationStatus.InProgress },
        { input: "suspended", expected: EVerificationStatus.Suspended },
        { input: "none", expected: EVerificationStatus.None },
        { input: "unknown", expected: EVerificationStatus.Pending },
        { input: null, expected: null },
        { input: undefined, expected: undefined },
        { input: 3, expected: EVerificationStatus.Verified }
    ];

    console.log("Iniciando testes de conversão de verificationStatus...");

    cases.forEach(({ input, expected }, index) => {
        const result = VerificationStatusSchema.safeParse(input);
        if (!result.success || result.data !== expected) {
            const actual = result.success ? result.data : "PARSING_FAILED";
            const errorMsg = `Caso de teste ${index} FALHOU: Entrada "${input}" -> Esperado ${expected}, mas obteve ${actual}`;
            console.error(errorMsg);
            throw new Error(errorMsg);
        }
        console.log(`Caso de teste ${index} passou: Entrada "${input}" -> Esperado ${expected}`);
    });

    console.log("Todos os testes de conversão passaram.");
};

// Executa os testes imediatamente ao carregar o módulo
testVerificationStatusConversion();
