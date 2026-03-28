import { expect, test, describe } from "vitest";
import { getVerificationBadgeVariant } from "../../lib/utils";

describe("getVerificationBadgeVariant", () => {
  test("returns success for Verified status (3)", () => {
    expect(getVerificationBadgeVariant(3)).toBe("success");
  });

  test("returns warning for InProgress status (2)", () => {
    expect(getVerificationBadgeVariant(2)).toBe("warning");
  });

  test("returns warning for Suspended status (5)", () => {
    expect(getVerificationBadgeVariant(5)).toBe("warning");
  });

  test("returns destructive for Rejected status (4)", () => {
    expect(getVerificationBadgeVariant(4)).toBe("destructive");
  });

  test("returns secondary for other statuses", () => {
    expect(getVerificationBadgeVariant(0)).toBe("secondary");
    expect(getVerificationBadgeVariant(1)).toBe("secondary");
    expect(getVerificationBadgeVariant(undefined as any)).toBe("secondary");
  });
});
