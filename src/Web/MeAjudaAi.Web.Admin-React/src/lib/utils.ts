import type { VerificationStatus } from "./types";

export type BadgeVariant = "success" | "warning" | "destructive" | "secondary";

export function getVerificationBadgeVariant(status?: VerificationStatus): BadgeVariant {
  switch (status) {
    case 2:
      return "success";
    case 0:
      return "warning";
    case 5:
      return "warning";
    case 3:
    case 4:
      return "destructive";
    default:
      return "secondary";
  }
}
