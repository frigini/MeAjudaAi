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

const hasKeycloakConfig = process.env.KEYCLOAK_ADMIN_CLIENT_ID || process.env.KEYCLOAK_CLIENT_ID;
const hasKeycloakSecret = process.env.KEYCLOAK_ADMIN_CLIENT_SECRET || process.env.KEYCLOAK_CLIENT_SECRET;
const hasKeycloakIssuer = !!process.env.KEYCLOAK_ISSUER;

const isFullyConfigured = hasKeycloakConfig && hasKeycloakSecret && hasKeycloakIssuer;

let keycloakClientId: string;
let keycloakClientSecret: string;
let keycloakIssuer: string;

if (isFullyConfigured) {
  keycloakClientId = process.env.KEYCLOAK_ADMIN_CLIENT_ID || process.env.KEYCLOAK_CLIENT_ID!;
  keycloakClientSecret = process.env.KEYCLOAK_ADMIN_CLIENT_SECRET || process.env.KEYCLOAK_CLIENT_SECRET!;
  keycloakIssuer = process.env.KEYCLOAK_ISSUER!;
} else if (process.env.CI === "true" || process.env.NEXT_PUBLIC_CI === "true") {
  keycloakClientId = "placeholder";
  keycloakClientSecret = "placeholder";
  keycloakIssuer = "http://localhost:8080/realms/meajudaai";
  console.warn("[auth] Warning: Missing Keycloak environment variables - using placeholder values for CI build.");
} else {
  keycloakClientId = process.env.KEYCLOAK_ADMIN_CLIENT_ID || process.env.KEYCLOAK_CLIENT_ID || "placeholder";
  keycloakClientSecret = process.env.KEYCLOAK_ADMIN_CLIENT_SECRET || process.env.KEYCLOAK_CLIENT_SECRET || "placeholder";
  keycloakIssuer = process.env.KEYCLOAK_ISSUER || "http://localhost:8080/realms/meajudaai";
}

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
    async jwt({ token, user, account, profile }) {
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
