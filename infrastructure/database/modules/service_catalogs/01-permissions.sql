-- Service Catalogs Module - Permissions
-- Grant permissions for service_catalogs module (service catalog management)

-- Create service_catalogs schema if it doesn't exist
-- CREATE SCHEMA IF NOT EXISTS service_catalogs; -- DISABLED: Schema created by EF Core

-- Set explicit schema ownership
-- ALTER SCHEMA service_catalogs OWNER TO catalogs_owner; -- DISABLED: Set after EF creates schema

-- Grant permissions only if schema exists (will be created by EF Core)
DO $$
BEGIN
    IF EXISTS (SELECT 1 FROM information_schema.schemata WHERE schema_name = 'service_catalogs') THEN
        GRANT USAGE ON SCHEMA service_catalogs TO catalogs_role;
        GRANT SELECT, INSERT, UPDATE, DELETE ON ALL TABLES IN SCHEMA service_catalogs TO catalogs_role;
        GRANT USAGE, SELECT ON ALL SEQUENCES IN SCHEMA service_catalogs TO catalogs_role;
        
        -- Grant cross-schema permissions to app role
        GRANT USAGE ON SCHEMA service_catalogs TO meajudaai_app_role;
        GRANT SELECT, INSERT, UPDATE, DELETE ON ALL TABLES IN SCHEMA service_catalogs TO meajudaai_app_role;
        GRANT USAGE, SELECT ON ALL SEQUENCES IN SCHEMA service_catalogs TO meajudaai_app_role;
        
        -- Set default privileges for future tables and sequences created by catalogs_owner
        ALTER DEFAULT PRIVILEGES FOR ROLE catalogs_owner IN SCHEMA service_catalogs GRANT SELECT, INSERT, UPDATE, DELETE ON TABLES TO catalogs_role;
        ALTER DEFAULT PRIVILEGES FOR ROLE catalogs_owner IN SCHEMA service_catalogs GRANT USAGE, SELECT ON SEQUENCES TO catalogs_role;
        
        -- Set default privileges for app role on objects created by catalogs_owner
        ALTER DEFAULT PRIVILEGES FOR ROLE catalogs_owner IN SCHEMA service_catalogs GRANT SELECT, INSERT, UPDATE, DELETE ON TABLES TO meajudaai_app_role;
        ALTER DEFAULT PRIVILEGES FOR ROLE catalogs_owner IN SCHEMA service_catalogs GRANT USAGE, SELECT ON SEQUENCES TO meajudaai_app_role;
        
        -- Set default search path for catalogs_role
        ALTER ROLE catalogs_role SET search_path = service_catalogs, public;
    END IF;
END;
$$ LANGUAGE plpgsql;

-- Grant cross-schema permissions with conditional checks
DO $$
BEGIN
    -- Grant read-only access to providers schema (for future ProviderServices integration)
    IF EXISTS (SELECT 1 FROM information_schema.schemata WHERE schema_name = 'providers') THEN
        GRANT USAGE ON SCHEMA providers TO catalogs_role;
        GRANT SELECT ON ALL TABLES IN SCHEMA providers TO catalogs_role;
    END IF;
    
    -- Grant read-only access to search_providers schema (for denormalization of services)
    IF EXISTS (SELECT 1 FROM information_schema.schemata WHERE schema_name = 'search_providers') THEN
        GRANT USAGE ON SCHEMA search_providers TO catalogs_role;
        GRANT SELECT ON ALL TABLES IN SCHEMA search_providers TO catalogs_role;
    END IF;
END;
$$ LANGUAGE plpgsql;

-- Document schema purpose
COMMENT ON SCHEMA service_catalogs IS 'Service Catalog module - Admin-managed service categories and services';
