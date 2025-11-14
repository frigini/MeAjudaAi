-- Users Module - Database Roles
-- Create dedicated role for users module (NOLOGIN role for permission grouping)

-- Create users module role if it doesn't exist (NOLOGIN, no password in DDL)
DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_catalog.pg_roles WHERE rolname = 'users_role') THEN
        CREATE ROLE users_role NOLOGIN INHERIT;
    END IF;
END;
$$ LANGUAGE plpgsql;

-- Create users module owner role if it doesn't exist
DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_catalog.pg_roles WHERE rolname = 'users_owner') THEN
        CREATE ROLE users_owner NOLOGIN INHERIT;
    END IF;
END;
$$ LANGUAGE plpgsql;

-- Create general application role for cross-cutting operations if it doesn't exist
DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_catalog.pg_roles WHERE rolname = 'meajudaai_app_role') THEN
        CREATE ROLE meajudaai_app_role NOLOGIN INHERIT;
    END IF;
END;
$$ LANGUAGE plpgsql;

-- Create application schema owner role if it doesn't exist
DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_catalog.pg_roles WHERE rolname = 'meajudaai_app_owner') THEN
        CREATE ROLE meajudaai_app_owner NOLOGIN INHERIT;
    END IF;
END;
$$ LANGUAGE plpgsql;

-- Grant users role to app role for cross-module access (idempotent)
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_auth_members m 
        JOIN pg_roles r1 ON m.roleid = r1.oid 
        JOIN pg_roles r2 ON m.member = r2.oid 
        WHERE r1.rolname = 'users_role' AND r2.rolname = 'meajudaai_app_role'
    ) THEN
        GRANT users_role TO meajudaai_app_role;
    END IF;
END;
$$ LANGUAGE plpgsql;

-- Grant users_owner to app_owner (for schema management)
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_auth_members m 
        JOIN pg_roles r1 ON m.roleid = r1.oid 
        JOIN pg_roles r2 ON m.member = r2.oid 
        WHERE r1.rolname = 'users_owner' AND r2.rolname = 'meajudaai_app_owner'
    ) THEN
        GRANT users_owner TO meajudaai_app_owner;
    END IF;
END;
$$ LANGUAGE plpgsql;

-- NOTE: Actual LOGIN users with passwords should be created in environment-specific
-- migrations that read passwords from secure session GUCs or configuration, not in versioned DDL.
-- Example: CREATE USER users_login_user WITH PASSWORD current_setting('app.users_password') IN ROLE users_role;

-- Document roles
COMMENT ON ROLE users_role IS 'Permission grouping role for users schema';
COMMENT ON ROLE users_owner IS 'Owner role for users schema objects';
COMMENT ON ROLE meajudaai_app_role IS 'App-wide role for cross-module access';
COMMENT ON ROLE meajudaai_app_owner IS 'Owner role for application-owned objects';