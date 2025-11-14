-- PROVIDERS Module - Database Roles (EXAMPLE - Module not implemented yet)
-- Create dedicated role for providers module (NOLOGIN role for permission grouping)
--
-- NOTE: Creating meajudaai_app_role in every module is safe but creates duplication.
-- Consider centralizing app role creation in a single bootstrap script (e.g., 00-bootstrap.sql)
-- to reduce redundancy across module initialization files.

-- Create providers module role if it doesn't exist (NOLOGIN, no password in DDL)
DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_catalog.pg_roles WHERE rolname = 'providers_role') THEN
        CREATE ROLE providers_role NOLOGIN INHERIT;
    END IF;
END;
$$ LANGUAGE plpgsql;

-- Create providers module owner role if it doesn't exist
DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_catalog.pg_roles WHERE rolname = 'providers_owner') THEN
        CREATE ROLE providers_owner NOLOGIN INHERIT;
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

-- Grant providers role to app role for cross-module access (idempotent)
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_auth_members m 
        JOIN pg_roles r1 ON m.roleid = r1.oid 
        JOIN pg_roles r2 ON m.member = r2.oid 
        WHERE r1.rolname = 'providers_role' AND r2.rolname = 'meajudaai_app_role'
    ) THEN
        GRANT providers_role TO meajudaai_app_role;
    END IF;
END;
$$ LANGUAGE plpgsql;

-- Grant providers_owner to app_owner (for schema management)
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_auth_members m 
        JOIN pg_roles r1 ON m.roleid = r1.oid 
        JOIN pg_roles r2 ON m.member = r2.oid 
        WHERE r1.rolname = 'providers_owner' AND r2.rolname = 'meajudaai_app_owner'
    ) THEN
        GRANT providers_owner TO meajudaai_app_owner;
    END IF;
END;
$$ LANGUAGE plpgsql;

-- NOTE: Actual LOGIN users with passwords should be created in environment-specific
-- migrations that read passwords from secure session GUCs or configuration, not in versioned DDL.
-- Example: CREATE USER providers_login_user WITH PASSWORD current_setting('app.providers_password') IN ROLE providers_role;

-- Document roles
COMMENT ON ROLE providers_role IS 'Permission grouping role for providers schema';
COMMENT ON ROLE providers_owner IS 'Owner role for providers schema objects';
COMMENT ON ROLE meajudaai_app_role IS 'App-wide role for cross-module access';
COMMENT ON ROLE meajudaai_app_owner IS 'Owner role for application-owned objects';