import { z } from "zod";
import { EVerificationStatus } from "@/types/api/provider";
import { normalizeVerificationStatus } from "@/lib/utils/normalization";

export const VerificationStatusSchema = z.preprocess(
    normalizeVerificationStatus,
    z.nativeEnum(EVerificationStatus).optional().nullable()
);
