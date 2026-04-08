import type { VerificationStatus } from "../types";

export type BadgeVariant = "success" | "warning" | "destructive" | "secondary";

export function getVerificationBadgeVariant(status?: VerificationStatus): BadgeVariant {
  switch (status) {
    case 3: // Verified
      return "success";
    case 2: // InProgress
    case 5: // Suspended
      return "warning";
    case 4: // Rejected
      return "destructive";
    default: // None (0), Pending (1), unknown
      return "secondary";
  }
}
