import { type NextAuthOptions } from "next-auth"
import Keycloak from "next-auth/providers/keycloak"
import Credentials from "next-auth/providers/credentials"
import { JWT } from "next-auth/jwt"
import { decodeJwt } from "jose"
import { getServerSession } from "next-auth/next"
import { getAuthSession } from "../libs/auth/src/lib/auth-wrapper"
import {
    GetServerSidePropsContext,
    NextApiRequest,
    NextApiResponse,
} from "next"

async function refreshAccessToken(token: JWT): Promise<JWT> {
    try {
        const response = await fetch(`${process.env.KEYCLOAK_ISSUER}/protocol/openid-connect/token`, {
            headers: { "Content-Type": "application/x-www-form-urlencoded" },
            body: new URLSearchParams({
                client_id: process.env.KEYCLOAK_CLIENT_ID ?? "",
                client_secret: process.env.KEYCLOAK_CLIENT_SECRET ?? "",
                grant_type: "refresh_token",
                refresh_token: token.refreshToken ?? "",
            }),
            method: "POST",
        })

        const refreshedTokens = await response.json()

        if (!response.ok) {
            throw refreshedTokens
        }

        return {
            ...token,
            accessToken: refreshedTokens.access_token,
            expiresAt: Date.now() + (refreshedTokens.expires_in * 1000),
            refreshToken: refreshedTokens.refresh_token ?? token.refreshToken, // Fall back to old refresh token
        }
    } catch (error: any) {
        const keycloakError = error as { error?: string; message?: string; error_description?: string; status?: number; statusCode?: number };
        // Log Keycloak error fields (error, error_description) instead of generic message/status
        console.error("RefreshAccessTokenError", {
            error: keycloakError.error || keycloakError.message,
            error_description: keycloakError.error_description,
            status: keycloakError.status || keycloakError.statusCode
        });

        return {
            ...token,
            error: "RefreshAccessTokenError",
        }
    }
}

function requireEnv(name: string): string {
    const value = process.env[name];
    if (!value) {
        // Warning: during 'next build', some variables might be missing (e.g. in CI)
        // We log a warning instead of throwing to avoid breaking the build.
        if (process.env.NODE_ENV !== "development") {
            console.warn(`[auth] Warning: Environment variable ${name} is missing.`);
        }
        return "";
    }
    return value;
}

let criticalEnvValidated = false;

export function resetValidationForTest() {
    criticalEnvValidated = false;
}

const isCi = process.env.CI === "true" || process.env.NEXT_PUBLIC_CI === "true" || process.env.MOCK_AUTH === "true";

export function validateCriticalEnvOnStartup() {
    if (criticalEnvValidated) return;
    
    // Allows bypassing runtime verification locally or during stateless pipelines
    if (process.env.SKIP_AUTH_ENV_VALIDATION === "true" || isCi) {
        criticalEnvValidated = true;
        return;
    }

    const requiredKeys = ["KEYCLOAK_CLIENT_ID", "KEYCLOAK_CLIENT_SECRET", "KEYCLOAK_ISSUER"];
    const missing = requiredKeys.filter(key => requireEnv(key) === "");
    
    // Check secret separately to handle both AUTH_SECRET and NEXTAUTH_SECRET correctly
    const hasSecret = process.env.AUTH_SECRET || process.env.NEXTAUTH_SECRET;
    if (!hasSecret) {
        missing.push("AUTH_SECRET (or NEXTAUTH_SECRET)");
    }

    if (missing.length > 0) {
        throw new Error(`Critical environment variables missing at runtime: ${missing.join(", ")}`);
    }

    criticalEnvValidated = true;
}

export const authOptions: NextAuthOptions = {
    secret: process.env.AUTH_SECRET || process.env.NEXTAUTH_SECRET,
    providers: [
        Keycloak({
            clientId: isCi ? "ci-placeholder" : requireEnv("KEYCLOAK_CLIENT_ID"),
            clientSecret: isCi ? "ci-placeholder" : requireEnv("KEYCLOAK_CLIENT_SECRET"),
            issuer: isCi ? "http://localhost:8080/realms/meajudaai" : requireEnv("KEYCLOAK_ISSUER"),
        }),
        Credentials({
            id: "credentials",
            name: "Credentials",
            credentials: {
                email: { label: "Email", type: "email" },
                password: { label: "Senha", type: "password" },
            },
            async authorize(credentials) {
                if (!credentials?.email || !credentials?.password) return null;

                try {
                    const res = await fetch(
                        `${process.env.KEYCLOAK_ISSUER}/protocol/openid-connect/token`,
                        {
                            method: "POST",
                            headers: { "Content-Type": "application/x-www-form-urlencoded" },
                            body: new URLSearchParams({
                                client_id: process.env.KEYCLOAK_CLIENT_ID ?? "",
                                client_secret: process.env.KEYCLOAK_CLIENT_SECRET ?? "",
                                grant_type: "password",
                                username: credentials.email as string,
                                password: credentials.password as string,
                                scope: "openid profile email",
                            }),
                        }
                    );

                    if (!res.ok) return null;

                    const tokens = await res.json();

                    // Decode the access token to get user info
                    let payload: { sub?: string; name?: string; preferred_username?: string; email?: string };
                    try {
                        payload = decodeJwt(tokens.access_token);
                    } catch {
                        console.error("TokenDecodeError", { error: "Failed to decode JWT token" });
                        return null;
                    }

                    return {
                        id: payload.sub || "",
                        name: (payload.name as string) || (payload.preferred_username as string) || "",
                        email: (payload.email as string) || "",
                        // Custom fields to pass to jwt callback
                        accessToken: tokens.access_token as string,
                        refreshToken: tokens.refresh_token as string,
                        expiresAt: Date.now() + (tokens.expires_in * 1000),
                    };
                } catch (error: any) {
                    const authError = error as Error;
                    console.error("CredentialsAuthError", {
                        error: authError.name || "UnknownError",
                        message: authError.message || "An error occurred during authentication",
                    });
                    return null;
                }
            },
        }),
    ],
    callbacks: {
        async jwt({ token, account, profile, user }) {
            // Initial sign in via OAuth (Keycloak OIDC)
            if (account && account.provider === "keycloak" && profile) {
                return {
                    ...token,
                    accessToken: account.access_token ?? "",
                    refreshToken: account.refresh_token ?? "",
                    expiresAt: account.expires_at ? account.expires_at * 1000 : Date.now() + 3600000,
                    id: profile.sub ?? "",
                }
            }

            // Initial sign in via Credentials (ROPC against Keycloak)
            if (account && account.provider === "credentials" && user) {
                return {
                    ...token,
                    accessToken: user.accessToken ?? "",
                    refreshToken: user.refreshToken ?? "",
                    expiresAt: user.expiresAt ?? Date.now() + 3600000,
                    id: user.id ?? "",
                }
            }

            // Return previous token if the access token has not expired yet
            if (Date.now() < (token.expiresAt ?? 0)) {
                return token
            }

            // Access token has expired, try to update it
            return refreshAccessToken(token)
        },
        async session({ session, token }) {
            if (token?.accessToken) {
                session.accessToken = token.accessToken as string
            }
            if (token?.id && session.user) {
                session.user.id = token.id as string
            }
            if (token?.error) {
                session.error = token.error as string
            }
            return session
        },
        async redirect({ url, baseUrl }) {
            // Rejeitar URLs protocolares relativas (ex: //attacker.com) para evitar Open Redirect
            if (url.startsWith("//")) {
                return baseUrl
            }

            try {
                const resolvedUrl = new URL(url, baseUrl)
                const baseOrigin = new URL(baseUrl).origin

                // Permite se a origem for a mesma ou se for um caminho relativo válido (já resolvido pelo construtor URL)
                if (resolvedUrl.origin === baseOrigin) {
                    return resolvedUrl.toString()
                }
            } catch {
                // Se falhar o parsing, cai no fallback seguro
            }

            return baseUrl
        },
    },
    // Debug in development
    debug: process.env.NODE_ENV === "development",
};

export function auth(
    ...args:
        | [GetServerSidePropsContext["req"], GetServerSidePropsContext["res"]]
        | [NextApiRequest, NextApiResponse]
        | []
) {
    validateCriticalEnvOnStartup();
    if (args.length === 0) {
        return getAuthSession(authOptions);
    }
    return getServerSession(...args, authOptions);
}
