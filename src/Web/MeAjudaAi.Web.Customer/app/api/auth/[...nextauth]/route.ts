import NextAuth from "next-auth"
import { authOptions, validateCriticalEnvOnStartup } from "@/auth"

// Run validation during initialization (skips Next.js build phase internally)
validateCriticalEnvOnStartup();

const handler = NextAuth(authOptions)

export { handler as GET, handler as POST }
