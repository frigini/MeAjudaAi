-- Documents Module - Database Roles
-- Create dedicated role for documents module (NOLOGIN role for permission grouping)

-- Create documents module role if it doesn't exist (NOLOGIN, no password in DDL)
DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_catalog.pg_roles WHERE rolname = 'documents_role') THEN
        CREATE ROLE documents_role NOLOGIN INHERIT;
    END IF;
END;
$$ LANGUAGE plpgsql;

-- Create documents module owner role if it doesn't exist
DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_catalog.pg_roles WHERE rolname = 'documents_owner') THEN
        CREATE ROLE documents_owner NOLOGIN INHERIT;
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

-- Create hangfire role if it doesn't exist (for background jobs)
DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_catalog.pg_roles WHERE rolname = 'hangfire_role') THEN
        CREATE ROLE hangfire_role NOLOGIN INHERIT;
    END IF;
END;
$$ LANGUAGE plpgsql;

-- Grant documents role to app role for cross-module access (idempotent)
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_auth_members m 
        JOIN pg_roles r1 ON m.roleid = r1.oid 
        JOIN pg_roles r2 ON m.member = r2.oid 
        WHERE r1.rolname = 'documents_role' AND r2.rolname = 'meajudaai_app_role'
    ) THEN
        GRANT documents_role TO meajudaai_app_role;
    END IF;
END;
$$ LANGUAGE plpgsql;

-- Grant hangfire role to app role (hangfire needs to access documents for background jobs)
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_auth_members m 
        JOIN pg_roles r1 ON m.roleid = r1.oid 
        JOIN pg_roles r2 ON m.member = r2.oid 
        WHERE r1.rolname = 'hangfire_role' AND r2.rolname = 'meajudaai_app_role'
    ) THEN
        GRANT hangfire_role TO meajudaai_app_role;
    END IF;
END;
$$ LANGUAGE plpgsql;

-- Grant documents_owner to app_owner (for schema management)
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_auth_members m 
        JOIN pg_roles r1 ON m.roleid = r1.oid 
        JOIN pg_roles r2 ON m.member = r2.oid 
        WHERE r1.rolname = 'documents_owner' AND r2.rolname = 'meajudaai_app_owner'
    ) THEN
        GRANT documents_owner TO meajudaai_app_owner;
    END IF;
END;
$$ LANGUAGE plpgsql;

-- NOTE: Actual LOGIN users with passwords should be created in environment-specific
-- migrations that read passwords from secure session GUCs or configuration, not in versioned DDL.
-- Example: CREATE USER documents_login_user WITH PASSWORD current_setting('app.documents_password') IN ROLE documents_role;

-- Document roles
COMMENT ON ROLE documents_role IS 'Permission grouping role for documents schema';
COMMENT ON ROLE documents_owner IS 'Owner role for meajudaai_documents schema objects';
COMMENT ON ROLE hangfire_role IS 'Permission role for Hangfire background jobs (hangfire schema)';
COMMENT ON ROLE meajudaai_app_role IS 'App-wide role for cross-module access';
COMMENT ON ROLE meajudaai_app_owner IS 'Owner role for application-owned objects';
