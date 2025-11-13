-- PROVIDERS Module - Permissions
-- Grant permissions for providers module

-- Create providers schema if it doesn't exist
CREATE SCHEMA IF NOT EXISTS providers;

-- Set explicit schema ownership
ALTER SCHEMA providers OWNER TO providers_owner;

GRANT USAGE ON SCHEMA providers TO providers_role;
GRANT SELECT, INSERT, UPDATE, DELETE ON ALL TABLES IN SCHEMA providers TO providers_role;
GRANT USAGE, SELECT ON ALL SEQUENCES IN SCHEMA providers TO providers_role;

-- Set default privileges for future tables and sequences created by providers_owner
ALTER DEFAULT PRIVILEGES FOR ROLE providers_owner IN SCHEMA providers GRANT SELECT, INSERT, UPDATE, DELETE ON TABLES TO providers_role;
ALTER DEFAULT PRIVILEGES FOR ROLE providers_owner IN SCHEMA providers GRANT USAGE, SELECT ON SEQUENCES TO providers_role;

-- Set default search path
ALTER ROLE providers_role SET search_path = providers, public;

-- Grant cross-schema permissions to app role
GRANT USAGE ON SCHEMA providers TO meajudaai_app_role;
GRANT SELECT, INSERT, UPDATE, DELETE ON ALL TABLES IN SCHEMA providers TO meajudaai_app_role;
GRANT USAGE, SELECT ON ALL SEQUENCES IN SCHEMA providers TO meajudaai_app_role;

-- Set default privileges for app role on objects created by providers_owner
ALTER DEFAULT PRIVILEGES FOR ROLE providers_owner IN SCHEMA providers GRANT SELECT, INSERT, UPDATE, DELETE ON TABLES TO meajudaai_app_role;
ALTER DEFAULT PRIVILEGES FOR ROLE providers_owner IN SCHEMA providers GRANT USAGE, SELECT ON SEQUENCES TO meajudaai_app_role;

-- Grant limited permissions on public schema (read-only)
GRANT USAGE ON SCHEMA public TO providers_role;
GRANT USAGE ON SCHEMA public TO meajudaai_app_role;

-- Harden public schema by revoking CREATE from PUBLIC (security best practice)
REVOKE CREATE ON SCHEMA public FROM PUBLIC;
