-- PROVIDERS Module - Database Roles
-- Create dedicated role for providers module

-- SECURITY: Replace <secure_password> with a strong, environment-specific secret before applying
-- Generate with: openssl rand -base64 32
-- Never commit actual passwords to version control
CREATE ROLE providers_role LOGIN PASSWORD '<secure_password>';

-- Grant providers role to app role for cross-module access
GRANT providers_role TO meajudaai_app_role;