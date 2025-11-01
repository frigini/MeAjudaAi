-- PROVIDERS Module - Permissions
-- Grant permissions for providers module
GRANT USAGE ON SCHEMA providers TO providers_role;
GRANT SELECT, INSERT, UPDATE, DELETE ON ALL TABLES IN SCHEMA providers TO providers_role;
GRANT USAGE, SELECT ON ALL SEQUENCES IN SCHEMA providers TO providers_role;

-- Set default privileges for future tables and sequences
ALTER DEFAULT PRIVILEGES IN SCHEMA providers GRANT SELECT, INSERT, UPDATE, DELETE ON TABLES TO providers_role;
ALTER DEFAULT PRIVILEGES IN SCHEMA providers GRANT USAGE, SELECT ON SEQUENCES TO providers_role;

-- Set default search path
ALTER ROLE providers_role SET search_path = providers, public;

-- Grant cross-schema permissions to app role
GRANT USAGE ON SCHEMA providers TO meajudaai_app_role;
GRANT SELECT, INSERT, UPDATE, DELETE ON ALL TABLES IN SCHEMA providers TO meajudaai_app_role;
GRANT USAGE, SELECT ON ALL SEQUENCES IN SCHEMA providers TO meajudaai_app_role;

-- Set default privileges for future tables and sequences for app role
ALTER DEFAULT PRIVILEGES IN SCHEMA providers GRANT SELECT, INSERT, UPDATE, DELETE ON TABLES TO meajudaai_app_role;
ALTER DEFAULT PRIVILEGES IN SCHEMA providers GRANT USAGE, SELECT ON SEQUENCES TO meajudaai_app_role;
