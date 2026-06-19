-- Bookings Module - Database Roles

DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_catalog.pg_roles WHERE rolname = '{{ROLE_NAME}}') THEN
        CREATE ROLE {{ROLE_NAME}} NOLOGIN INHERIT;
    END IF;
END;
$$ LANGUAGE plpgsql;

DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_catalog.pg_roles WHERE rolname = '{{ROLE_OWNER_NAME}}') THEN
        CREATE ROLE {{ROLE_OWNER_NAME}} NOLOGIN INHERIT;
    END IF;
END;
$$ LANGUAGE plpgsql;

DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_catalog.pg_roles WHERE rolname = '{{APP_ROLE_NAME}}') THEN
        CREATE ROLE {{APP_ROLE_NAME}} NOLOGIN INHERIT;
    END IF;
END;
$$ LANGUAGE plpgsql;

DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_auth_members m 
        JOIN pg_roles r1 ON m.roleid = r1.oid 
        JOIN pg_roles r2 ON m.member = r2.oid 
        WHERE r1.rolname = '{{ROLE_NAME}}' AND r2.rolname = '{{APP_ROLE_NAME}}'
    ) THEN
        GRANT {{ROLE_NAME}} TO {{APP_ROLE_NAME}};
    END IF;
END;
$$ LANGUAGE plpgsql;

-- Commenting roles
COMMENT ON ROLE {{ROLE_NAME}} IS 'Permission grouping role for {{SCHEMA_NAME}} schema';
COMMENT ON ROLE {{ROLE_OWNER_NAME}} IS 'Owner role for {{SCHEMA_NAME}} schema objects';
COMMENT ON ROLE {{APP_ROLE_NAME}} IS 'App-wide role for cross-module access';
