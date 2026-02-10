import NextAuth from "next-auth"
import Keycloak from "next-auth/providers/keycloak"
import { JWT } from "next-auth/jwt"

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
        // Log only safe error details to prevent token leakage
        console.error("RefreshAccessTokenError", {
            message: error.message,
            status: error.status || error.statusCode
        });

        return {
            ...token,
            error: "RefreshAccessTokenError",
        }
    }
}

function requireEnv(name: string): string {
    const value = process.env[name];
    if (!value) throw new Error(`Missing ${name}`);
    return value;
}

export const { handlers, signIn, signOut, auth } = NextAuth({
    providers: [
        Keycloak({
            clientId: requireEnv("KEYCLOAK_CLIENT_ID"),
            clientSecret: requireEnv("KEYCLOAK_CLIENT_SECRET"),
            issuer: requireEnv("KEYCLOAK_ISSUER"),
        }),
    ],
    pages: {
        signIn: "/auth/signin",
    },
    callbacks: {
        async jwt({ token, account, profile }) {
            // Initial sign in
            if (account && profile) {
                return {
                    ...token,
                    accessToken: account.access_token ?? "",
                    refreshToken: account.refresh_token ?? "",
                    expiresAt: account.expires_at ? account.expires_at * 1000 : Date.now() + 3600000,
                    id: profile.sub ?? "",
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
});
