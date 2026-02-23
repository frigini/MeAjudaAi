import { z } from "zod";
import { EVerificationStatus } from "@/types/api/provider";

// Test-only schema extraction from prestador/[id]/page.tsx
const VerificationStatusSchema = z.preprocess((val) => {
    if (typeof val === 'string') {
        if (/^\d+$/.test(val)) {
            return parseInt(val, 10);
        }
        const lower = val.toLowerCase();
        if (lower === 'verified') return EVerificationStatus.Verified;
        if (lower === 'rejected') return EVerificationStatus.Rejected;
        if (lower === 'inprogress' || lower === 'in_progress') return EVerificationStatus.InProgress;
        if (lower === 'suspended') return EVerificationStatus.Suspended;
        if (lower === 'none') return EVerificationStatus.None;
        return EVerificationStatus.Pending;
    }
    return val;
}, z.nativeEnum(EVerificationStatus).optional().nullable());

// Mock testing logic (since no runner is configured)
export const testVerificationStatusConversion = () => {
    const cases = [
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

    console.log("Starting verificationStatus conversion tests...");

    cases.forEach(({ input, expected }, index) => {
        const result = VerificationStatusSchema.safeParse(input);
        if (result.success && result.data === expected) {
            console.log(`Test case ${index} passed: Input "${input}" -> Expected ${expected}`);
        } else {
            console.error(`Test case ${index} FAILED: Input "${input}" -> Expected ${expected}, got ${result.success ? result.data : result.error}`);
        }
    });
};
