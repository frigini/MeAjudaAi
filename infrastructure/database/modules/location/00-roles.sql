-- Location Module - Database Roles
-- Create dedicated role for location module (NOLOGIN role for permission grouping)

-- Create location module role if it doesn't exist (NOLOGIN, no password in DDL)
DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_catalog.pg_roles WHERE rolname = 'location_role') THEN
        CREATE ROLE location_role NOLOGIN INHERIT;
    END IF;
END;
$$ LANGUAGE plpgsql;

-- Create location module owner role if it doesn't exist
DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_catalog.pg_roles WHERE rolname = 'location_owner') THEN
        CREATE ROLE location_owner NOLOGIN INHERIT;
    END IF;
END;
$$ LANGUAGE plpgsql;

-- Grant location role to app role for cross-module access (idempotent)
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_auth_members m 
        JOIN pg_roles r1 ON m.roleid = r1.oid 
        JOIN pg_roles r2 ON m.member = r2.oid 
        WHERE r1.rolname = 'location_role' AND r2.rolname = 'meajudaai_app_role'
    ) THEN
        GRANT location_role TO meajudaai_app_role;
    END IF;
END;
$$ LANGUAGE plpgsql;

-- Grant location_owner to app_owner (for schema management)
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_auth_members m 
        JOIN pg_roles r1 ON m.roleid = r1.oid 
        JOIN pg_roles r2 ON m.member = r2.oid 
        WHERE r1.rolname = 'location_owner' AND r2.rolname = 'meajudaai_app_owner'
    ) THEN
        GRANT location_owner TO meajudaai_app_owner;
    END IF;
END;
$$ LANGUAGE plpgsql;

-- NOTE: Actual LOGIN users with passwords should be created in environment-specific
-- migrations that read passwords from secure session GUCs or configuration, not in versioned DDL.

-- Document roles
COMMENT ON ROLE location_role IS 'Permission grouping role for location schema';
COMMENT ON ROLE location_owner IS 'Owner role for location schema objects';
