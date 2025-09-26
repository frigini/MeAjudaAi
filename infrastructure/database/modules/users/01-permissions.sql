-- Users Module - Permissions
-- Grant permissions for users module
GRANT USAGE ON SCHEMA users TO users_role;
GRANT SELECT, INSERT, UPDATE, DELETE ON ALL TABLES IN SCHEMA users TO users_role;
GRANT USAGE, SELECT ON ALL SEQUENCES IN SCHEMA users TO users_role;

-- Set default privileges for future tables and sequences created by users_owner
ALTER DEFAULT PRIVILEGES FOR ROLE users_owner IN SCHEMA users GRANT SELECT, INSERT, UPDATE, DELETE ON TABLES TO users_role;
ALTER DEFAULT PRIVILEGES FOR ROLE users_owner IN SCHEMA users GRANT USAGE, SELECT ON SEQUENCES TO users_role;

-- Set default search path for users_role
ALTER ROLE users_role SET search_path = users, public;

-- Grant cross-schema permissions to app role
GRANT USAGE ON SCHEMA users TO meajudaai_app_role;
GRANT SELECT, INSERT, UPDATE, DELETE ON ALL TABLES IN SCHEMA users TO meajudaai_app_role;
GRANT USAGE, SELECT ON ALL SEQUENCES IN SCHEMA users TO meajudaai_app_role;

-- Set default privileges for app role on objects created by users_owner
ALTER DEFAULT PRIVILEGES FOR ROLE users_owner IN SCHEMA users GRANT SELECT, INSERT, UPDATE, DELETE ON TABLES TO meajudaai_app_role;
ALTER DEFAULT PRIVILEGES FOR ROLE users_owner IN SCHEMA users GRANT USAGE, SELECT ON SEQUENCES TO meajudaai_app_role;

-- Create dedicated application schema for cross-cutting objects
CREATE SCHEMA IF NOT EXISTS meajudaai_app;

-- Grant permissions on dedicated application schema
GRANT USAGE, CREATE ON SCHEMA meajudaai_app TO meajudaai_app_role;

-- Set search path for app role (dedicated schema first, then module schemas, then public)
ALTER ROLE meajudaai_app_role SET search_path = meajudaai_app, users, public;

-- Grant limited permissions on public schema (read-only)
GRANT USAGE ON SCHEMA public TO users_role;
GRANT USAGE ON SCHEMA public TO meajudaai_app_role;