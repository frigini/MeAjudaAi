-- Users Module - Permissions
-- Grant permissions for users module

-- Create users schema if it doesn't exist
-- CREATE SCHEMA IF NOT EXISTS users; -- DISABLED: Schema created by EF Core

-- Set explicit schema ownership
-- ALTER SCHEMA users OWNER TO users_owner; -- DISABLED: Set after EF creates schema

-- Grant permissions only if schema exists (will be created by EF Core)
DO $$
BEGIN
    IF EXISTS (SELECT 1 FROM information_schema.schemata WHERE schema_name = 'users') THEN
        -- Grant immediate permissions on existing objects
        GRANT USAGE ON SCHEMA users TO users_role;
        GRANT SELECT, INSERT, UPDATE, DELETE ON ALL TABLES IN SCHEMA users TO users_role;
        GRANT USAGE, SELECT ON ALL SEQUENCES IN SCHEMA users TO users_role;
        
        -- Grant cross-schema permissions to app role
        GRANT USAGE ON SCHEMA users TO meajudaai_app_role;
        GRANT SELECT, INSERT, UPDATE, DELETE ON ALL TABLES IN SCHEMA users TO meajudaai_app_role;
        GRANT USAGE, SELECT ON ALL SEQUENCES IN SCHEMA users TO meajudaai_app_role;
        
        -- Set default privileges for future tables and sequences created by users_owner
        ALTER DEFAULT PRIVILEGES FOR ROLE users_owner IN SCHEMA users GRANT SELECT, INSERT, UPDATE, DELETE ON TABLES TO users_role;
        ALTER DEFAULT PRIVILEGES FOR ROLE users_owner IN SCHEMA users GRANT USAGE, SELECT ON SEQUENCES TO users_role;
        
        -- Set default privileges for app role on objects created by users_owner
        ALTER DEFAULT PRIVILEGES FOR ROLE users_owner IN SCHEMA users GRANT SELECT, INSERT, UPDATE, DELETE ON TABLES TO meajudaai_app_role;
        ALTER DEFAULT PRIVILEGES FOR ROLE users_owner IN SCHEMA users GRANT USAGE, SELECT ON SEQUENCES TO meajudaai_app_role;
        
        -- Set default search path for users_role
        ALTER ROLE users_role SET search_path = users, public;
    END IF;
END;
$$ LANGUAGE plpgsql;

-- NOTE: The meajudaai_app schema, its ownership, grants, search_path, and public
-- schema hardening are all managed by documents/01-permissions.sql, which
-- precedes users/01-permissions.sql alphabetically (d < u) and is the
-- authoritative location for these cross-cutting settings.