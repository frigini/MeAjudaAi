import { getServerSession, type Session } from "next-auth";
import { headers } from "next/headers";

const isCI = process.env.CI === "true" || process.env.MOCK_AUTH === "true";

export async function getAuthSession(authOptions: any): Promise<Session | null> {
  if (isCI) {
    // In Next.js 15, headers() is a Promise
    const headerList = await headers();
    const mockAuth = headerList.get("x-mock-auth");
    
    if (mockAuth === "true") {
      // Return a stable mock session for E2E tests
      return {
        user: {
          id: "test-admin-id",
          name: "Test Admin",
          email: "admin@test.com",
          roles: ["admin"],
        },
        expires: new Date(Date.now() + 3600 * 1000).toISOString(),
      } as Session;
    }
  }

  return await getServerSession(authOptions);
}
