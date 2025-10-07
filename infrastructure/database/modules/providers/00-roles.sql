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
        CREATE ROLE providers_role NOLOGIN;
    END IF;
END
$$ LANGUAGE plpgsql;

-- Create general application role for cross-cutting operations if it doesn't exist
DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_catalog.pg_roles WHERE rolname = 'meajudaai_app_role') THEN
        CREATE ROLE meajudaai_app_role NOLOGIN;
    END IF;
END
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
END
$$ LANGUAGE plpgsql;