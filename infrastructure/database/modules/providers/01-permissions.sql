-- Users Module - Permissions
-- Grant permissions for users module
GRANT USAGE ON SCHEMA users TO users_role;
GRANT SELECT, INSERT, UPDATE, DELETE ON ALL TABLES IN SCHEMA users TO users_role;
GRANT USAGE, SELECT ON ALL SEQUENCES IN SCHEMA users TO users_role;

-- Set default privileges for future tables and sequences
ALTER DEFAULT PRIVILEGES IN SCHEMA users GRANT SELECT, INSERT, UPDATE, DELETE ON TABLES TO users_role;
ALTER DEFAULT PRIVILEGES IN SCHEMA users GRANT USAGE, SELECT ON SEQUENCES TO users_role;

-- Set default search path for users_role
ALTER ROLE users_role SET search_path = users, public;

-- Grant cross-schema permissions to app role
GRANT USAGE ON SCHEMA users TO meajudaai_app_role;
GRANT SELECT, INSERT, UPDATE, DELETE ON ALL TABLES IN SCHEMA users TO meajudaai_app_role;
GRANT USAGE, SELECT ON ALL SEQUENCES IN SCHEMA users TO meajudaai_app_role;

-- Set default privileges for app role
ALTER DEFAULT PRIVILEGES IN SCHEMA users GRANT SELECT, INSERT, UPDATE, DELETE ON TABLES TO meajudaai_app_role;
ALTER DEFAULT PRIVILEGES IN SCHEMA users GRANT USAGE, SELECT ON SEQUENCES TO meajudaai_app_role;

-- Set search path for app role
ALTER ROLE meajudaai_app_role SET search_path = users, public;

-- Grant permissions on public schema
GRANT USAGE ON SCHEMA public TO users_role;
GRANT USAGE ON SCHEMA public TO meajudaai_app_role;
GRANT CREATE ON SCHEMA public TO meajudaai_app_role;