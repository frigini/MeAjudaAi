import NextAuth, { type NextAuthOptions, type DefaultSession } from "next-auth";
import Keycloak from "next-auth/providers/keycloak";

declare module "next-auth" {
  interface Session extends DefaultSession {
    user: {
      id?: string;
      roles?: string[];
    } & DefaultSession["user"];
  }
}

function getRequiredEnv(name: string): string {
  const value = process.env[name];
  if (!value) {
    throw new Error(`Missing required environment variable: ${name}`);
  }
  return value;
}

const isCi = process.env.CI === "true" || process.env.NEXT_PUBLIC_CI === "true";

const hasAdminVars = process.env.KEYCLOAK_ADMIN_CLIENT_ID && process.env.KEYCLOAK_ADMIN_CLIENT_SECRET;
const hasClientVars = process.env.KEYCLOAK_CLIENT_ID && process.env.KEYCLOAK_CLIENT_SECRET;

const hasPartialAdminVars = (!!process.env.KEYCLOAK_ADMIN_CLIENT_ID) !== (!!process.env.KEYCLOAK_ADMIN_CLIENT_SECRET);
const hasPartialClientVars = (!!process.env.KEYCLOAK_CLIENT_ID) !== (!!process.env.KEYCLOAK_CLIENT_SECRET);

if (hasPartialAdminVars) {
  throw new Error("Both KEYCLOAK_ADMIN_CLIENT_ID and KEYCLOAK_ADMIN_CLIENT_SECRET must be set or neither");
}

if (hasPartialClientVars) {
  throw new Error("Both KEYCLOAK_CLIENT_ID and KEYCLOAK_CLIENT_SECRET must be set or neither");
}

let keycloakClientId: string;
let keycloakClientSecret: string;
let keycloakIssuer: string;
let authMode: string;

if (hasAdminVars) {
  authMode = "ADMIN";
  keycloakClientId = process.env.KEYCLOAK_ADMIN_CLIENT_ID!;
  keycloakClientSecret = process.env.KEYCLOAK_ADMIN_CLIENT_SECRET!;
  keycloakIssuer = process.env.KEYCLOAK_ISSUER || (isCi ? "http://localhost:8080/realms/meajudaai" : getRequiredEnv("KEYCLOAK_ISSUER"));
} else if (hasClientVars) {
  authMode = "CLIENT";
  keycloakClientId = process.env.KEYCLOAK_CLIENT_ID!;
  keycloakClientSecret = process.env.KEYCLOAK_CLIENT_SECRET!;
  keycloakIssuer = process.env.KEYCLOAK_ISSUER || (isCi ? "http://localhost:8080/realms/meajudaai" : getRequiredEnv("KEYCLOAK_ISSUER"));
} else if (isCi) {
  authMode = "CI_PLACEHOLDER";
  keycloakClientId = "placeholder";
  keycloakClientSecret = "placeholder";
  keycloakIssuer = "http://localhost:8080/realms/meajudaai";
  console.warn("[auth] Warning: Missing Keycloak environment variables - using placeholder values for CI build.");
} else {
  authMode = "REQUIRED";
  keycloakClientId = getRequiredEnv("KEYCLOAK_CLIENT_ID");
  keycloakClientSecret = getRequiredEnv("KEYCLOAK_CLIENT_SECRET");
  keycloakIssuer = getRequiredEnv("KEYCLOAK_ISSUER");
}

let hostname = 'unknown';
if (keycloakIssuer) {
  try {
    hostname = new URL(keycloakIssuer).hostname;
  } catch (e) {
    console.warn(`[auth] Invalid KEYCLOAK_ISSUER URL: ${keycloakIssuer}`, e);
  }
}
console.log(`[auth] Using KEYCLOAK credentials: ${authMode} (issuer: ${hostname})`);

export const authOptions: NextAuthOptions = {
  providers: [
    Keycloak({
      clientId: keycloakClientId,
      clientSecret: keycloakClientSecret,
      issuer: keycloakIssuer,
    }),
  ],
  pages: {
    signIn: "/login",
    error: "/login",
  },
  session: {
    strategy: "jwt",
    maxAge: 30 * 60,
  },
  callbacks: {
    async jwt({ token, account, profile }) {
      if (account && profile) {
        const keycloakProfile = profile as {
          sub?: string;
          realm_access?: { roles?: string[] };
        };
        token.id = keycloakProfile.sub ?? profile.sub;
        token.roles = keycloakProfile.realm_access?.roles ?? [];
        token.accessToken = account.access_token;
        token.refreshToken = account.refresh_token;
        token.exp = account.expires_at;
      }
      return token;
    },
    async session({ session, token }) {
      if (session.user) {
        session.user.id = token.id as string | undefined;
        session.user.roles = token.roles as string[] | undefined;
      }
      return session;
    },
  },
};

const handler = NextAuth(authOptions);
export { handler as GET, handler as POST };
