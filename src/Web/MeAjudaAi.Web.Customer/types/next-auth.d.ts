import { DefaultSession } from "next-auth";

declare module "next-auth" {
    /**
     * Returned by `useSession`, `getSession` and received as a prop on the `SessionProvider` React Context
     */
    interface Session {
        /**
         * The user's access token from Keycloak.
         */
        accessToken?: string;

        /**
         * Any refresh error that might have occurred.
         */
        error?: string;

        user: {
            /** The user's internal database UUID. */
            id: string;
            // Add other properties if needed
        } & DefaultSession["user"];
    }
}

declare module "next-auth/jwt" {
    /** Returned by the `jwt` callback and `auth`, when using JWT sessions */
    interface JWT {
        accessToken?: string
        refreshToken?: string
        expiresAt?: number
        id?: string
        error?: string
    }
}
