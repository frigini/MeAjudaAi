import { VerificationStatusSchema } from "@/app/(main)/prestador/[id]/page";
import { EVerificationStatus } from "@/types/api/provider";

// Test-only execution logic
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

    console.log("Starting verificationStatus conversion tests...");

    cases.forEach(({ input, expected }, index) => {
        const result = VerificationStatusSchema.safeParse(input);
        if (!result.success || result.data !== expected) {
            const actual = result.success ? result.data : "PARSE_ERROR";
            throw new Error(`Test case ${index} FAILED: Input "${input}" -> Expected ${expected}, but got ${actual}`);
        }
        console.log(`Test case ${index} passed: Input "${input}" -> Expected ${expected}`);
    });
};

// Run tests immediately on load to break CI if they fail
testVerificationStatusConversion();
