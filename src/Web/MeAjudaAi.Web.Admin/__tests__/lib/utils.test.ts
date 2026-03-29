import { expect, test, describe } from "vitest";
import { getVerificationBadgeVariant } from "../../lib/utils";
import { EVerificationStatus } from "../../lib/types";

describe("getVerificationBadgeVariant", () => {
  test("returns success for Verified status", () => {
    expect(getVerificationBadgeVariant(EVerificationStatus.Verified)).toBe("success");
  });

  test("returns warning for InProgress status", () => {
    expect(getVerificationBadgeVariant(EVerificationStatus.InProgress)).toBe("warning");
  });

  test("returns warning for Suspended status", () => {
    expect(getVerificationBadgeVariant(EVerificationStatus.Suspended)).toBe("warning");
  });

  test("returns destructive for Rejected status", () => {
    expect(getVerificationBadgeVariant(EVerificationStatus.Rejected)).toBe("destructive");
  });

  test("returns secondary for other statuses", () => {
    expect(getVerificationBadgeVariant(EVerificationStatus.None)).toBe("secondary");
    expect(getVerificationBadgeVariant(EVerificationStatus.Pending)).toBe("secondary");
    expect(getVerificationBadgeVariant(undefined as any)).toBe("secondary");
    expect(getVerificationBadgeVariant(null as any)).toBe("secondary");
    expect(getVerificationBadgeVariant(-1 as any)).toBe("secondary");
    expect(getVerificationBadgeVariant(999 as any)).toBe("secondary");
    expect(getVerificationBadgeVariant("invalid" as any)).toBe("secondary");
  });
});
