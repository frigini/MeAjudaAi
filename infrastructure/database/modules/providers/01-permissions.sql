-- PROVIDERS Module - Permissions
-- Grant permissions for providers module
GRANT USAGE ON SCHEMA providers TO providers_role;
GRANT SELECT, INSERT, UPDATE, DELETE ON ALL TABLES IN SCHEMA providers TO providers_role;
GRANT USAGE, SELECT ON ALL SEQUENCES IN SCHEMA providers TO providers_role;

-- Set default privileges for future tables and sequences created by providers_role
ALTER DEFAULT PRIVILEGES FOR ROLE providers_role IN SCHEMA providers GRANT SELECT, INSERT, UPDATE, DELETE ON TABLES TO providers_role;
ALTER DEFAULT PRIVILEGES FOR ROLE providers_role IN SCHEMA providers GRANT USAGE, SELECT ON SEQUENCES TO providers_role;

-- Set default search path for providers_role
ALTER ROLE providers_role SET search_path = providers, public;

-- Grant cross-schema permissions to app role
GRANT USAGE ON SCHEMA providers TO meajudaai_app_role;
GRANT SELECT, INSERT, UPDATE, DELETE ON ALL TABLES IN SCHEMA providers TO meajudaai_app_role;
GRANT USAGE, SELECT ON ALL SEQUENCES IN SCHEMA providers TO meajudaai_app_role;

-- Set default privileges for app role on objects created by providers_role
ALTER DEFAULT PRIVILEGES FOR ROLE providers_role IN SCHEMA providers GRANT SELECT, INSERT, UPDATE, DELETE ON TABLES TO meajudaai_app_role;
ALTER DEFAULT PRIVILEGES FOR ROLE providers_role IN SCHEMA providers GRANT USAGE, SELECT ON SEQUENCES TO meajudaai_app_role;

-- Grant permissions on public schema
GRANT USAGE ON SCHEMA public TO providers_role;