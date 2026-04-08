import { expect, test, describe } from "vitest";
import { getVerificationBadgeVariant } from "../../lib/utils";
import { EVerificationStatus } from "../../lib/types";

const statusVariantMap: [EVerificationStatus | undefined, string][] = [
  [EVerificationStatus.Verified, "success"],
  [EVerificationStatus.InProgress, "warning"],
  [EVerificationStatus.Suspended, "warning"],
  [EVerificationStatus.Rejected, "destructive"],
  [EVerificationStatus.None, "secondary"],
  [EVerificationStatus.Pending, "secondary"],
  [undefined, "secondary"],
  [null, "secondary"],
  [-1, "secondary"],
  [999, "secondary"],
  ["invalid" as any, "secondary"],
];

describe("getVerificationBadgeVariant", () => {
  test.each(statusVariantMap)("returns %s for %p status", (status, expected) => {
    expect(getVerificationBadgeVariant(status as any)).toBe(expected);
  });
});
