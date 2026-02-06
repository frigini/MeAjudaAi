import NextAuth from "next-auth"
import Keycloak from "next-auth/providers/keycloak"

export const { handlers, signIn, signOut, auth } = NextAuth({
    providers: [
        Keycloak({
            clientId: process.env.KEYCLOAK_CLIENT_ID,
            clientSecret: process.env.KEYCLOAK_CLIENT_SECRET,
            issuer: process.env.KEYCLOAK_ISSUER,
        }),
    ],
    callbacks: {
        async jwt({ token, account, profile }) {
            if (account && profile) {
                token.accessToken = account.access_token
                token.id = profile.sub
            }
            return token
        },
        async session({ session, token }) {
            if (token) {
                session.accessToken = token.accessToken as string
                session.user.id = token.id as string
            }
            return session
        },
    },
    // Debug in development
    debug: process.env.NODE_ENV === "development",
})
