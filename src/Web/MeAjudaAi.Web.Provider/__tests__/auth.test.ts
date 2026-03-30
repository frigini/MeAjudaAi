import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest';
import { validateCriticalEnvOnStartup, authOptions, resetValidationForTest } from '../auth';

// Mock jose for JWT decoding
vi.mock('jose', () => ({
    decodeJwt: vi.fn(() => ({ sub: 'user-123', email: 'test@test.com' }))
}));

// Mock next-auth functions
vi.mock('next-auth/next', () => ({
    getServerSession: vi.fn(),
}));

describe('auth.ts unit tests', () => {
    const originalEnv = { ...process.env };

    beforeEach(() => {
        vi.resetModules();
        resetValidationForTest();
        process.env = { ...originalEnv };
        vi.stubGlobal('fetch', vi.fn());
        vi.spyOn(console, 'error').mockImplementation(() => {});
        vi.spyOn(console, 'warn').mockImplementation(() => {});
    });

    afterEach(() => {
        process.env = originalEnv;
        vi.restoreAllMocks();
    });

    describe('validateCriticalEnvOnStartup', () => {
        it('should not throw in CI environment', () => {
            vi.stubEnv('CI', 'true');
            // Should not throw even if variables are missing
            expect(() => validateCriticalEnvOnStartup()).not.toThrow();
        });

        it('should throw if critical variables are missing in non-CI', () => {
            vi.stubEnv('CI', 'false');
            vi.stubEnv('SKIP_AUTH_ENV_VALIDATION', 'false');
            vi.stubEnv('KEYCLOAK_CLIENT_ID', '');
            
            expect(() => validateCriticalEnvOnStartup()).toThrow(/Critical environment variables missing/);
        });

        it('should pass if all variables are present', () => {
            vi.stubEnv('CI', 'false');
            vi.stubEnv('SKIP_AUTH_ENV_VALIDATION', 'false');
            vi.stubEnv('KEYCLOAK_CLIENT_ID', 'id');
            vi.stubEnv('KEYCLOAK_CLIENT_SECRET', 'secret');
            vi.stubEnv('KEYCLOAK_ISSUER', 'issuer');
            vi.stubEnv('NEXTAUTH_SECRET', 'auth-secret');
            
            expect(() => validateCriticalEnvOnStartup()).not.toThrow();
        });
    });

    describe('callbacks', () => {
        describe('jwt', () => {
            it('should handle initial Keycloak sign in', async () => {
                const token = {};
                const account = { 
                    provider: 'keycloak', 
                    access_token: 'at', 
                    refresh_token: 'rt', 
                    expires_at: 1000 
                };
                const profile = { sub: 'sub-1' };

                const result = await (authOptions.callbacks?.jwt as any)({ token, account, profile });

                expect(result.accessToken).toBe('at');
                expect(result.refreshToken).toBe('rt');
                expect(result.id).toBe('sub-1');
            });

            it('should return existing token if not expired', async () => {
                const token = { expiresAt: Date.now() + 10000, accessToken: 'existing' };
                const result = await (authOptions.callbacks?.jwt as any)({ token });
                expect(result.accessToken).toBe('existing');
            });
        });

        describe('session', () => {
            it('should extend session with token data', async () => {
                const session = { user: {} };
                const token = { accessToken: 'at', id: 'id-1', error: 'err' };

                const result = await (authOptions.callbacks?.session as any)({ session, token });

                expect(result.accessToken).toBe('at');
                expect(result.user.id).toBe('id-1');
                expect(result.error).toBe('err');
            });
        });

        describe('redirect', () => {
            it('should allow relative URLs', async () => {
                const url = '/dashboard';
                const baseUrl = 'http://localhost:3000';
                const result = await (authOptions.callbacks?.redirect as any)({ url, baseUrl });
                expect(result).toBe('http://localhost:3000/dashboard');
            });

            it('should reject different origin URLs', async () => {
                const url = 'https://malicious.com';
                const baseUrl = 'http://localhost:3000';
                const result = await (authOptions.callbacks?.redirect as any)({ url, baseUrl });
                expect(result).toBe(baseUrl);
            });
            
            it('should reject protocol-relative URLs', async () => {
                const url = '//malicious.com';
                const baseUrl = 'http://localhost:3000';
                const result = await (authOptions.callbacks?.redirect as any)({ url, baseUrl });
                expect(result).toBe(baseUrl);
            });
        });
    });
});
