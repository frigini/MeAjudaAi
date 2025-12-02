-- Search Module - Database Roles
-- Create dedicated role for search module (NOLOGIN role for permission grouping)

-- Create search module role if it doesn't exist (NOLOGIN, no password in DDL)
DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_catalog.pg_roles WHERE rolname = 'search_role') THEN
        CREATE ROLE search_role NOLOGIN INHERIT;
    END IF;
END;
$$ LANGUAGE plpgsql;

-- Create search module owner role if it doesn't exist
DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_catalog.pg_roles WHERE rolname = 'search_owner') THEN
        CREATE ROLE search_owner NOLOGIN INHERIT;
    END IF;
END;
$$ LANGUAGE plpgsql;

-- Grant search role to app role for cross-module access (idempotent)
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_auth_members m 
        JOIN pg_roles r1 ON m.roleid = r1.oid 
        JOIN pg_roles r2 ON m.member = r2.oid 
        WHERE r1.rolname = 'search_role' AND r2.rolname = 'meajudaai_app_role'
    ) THEN
        GRANT search_role TO meajudaai_app_role;
    END IF;
END;
$$ LANGUAGE plpgsql;

-- Grant search_owner to app_owner (for schema management)
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_auth_members m 
        JOIN pg_roles r1 ON m.roleid = r1.oid 
        JOIN pg_roles r2 ON m.member = r2.oid 
        WHERE r1.rolname = 'search_owner' AND r2.rolname = 'meajudaai_app_owner'
    ) THEN
        GRANT search_owner TO meajudaai_app_owner;
    END IF;
END;
$$ LANGUAGE plpgsql;

-- NOTE: Actual LOGIN users with passwords should be created in environment-specific
-- migrations that read passwords from secure session GUCs or configuration, not in versioned DDL.

-- Document roles
COMMENT ON ROLE search_role IS 'Permission grouping role for search schema';
COMMENT ON ROLE search_owner IS 'Owner role for search schema objects';
