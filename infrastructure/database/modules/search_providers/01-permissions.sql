-- Search Providers Module - Permissions
-- Grant permissions for search_providers module (geospatial search with PostGIS)

-- Enable PostGIS extension if not already enabled
CREATE EXTENSION IF NOT EXISTS postgis;

-- Create search_providers schema if it doesn't exist
-- CREATE SCHEMA IF NOT EXISTS search_providers; -- DISABLED: Schema created by EF Core

-- Set explicit schema ownership
-- ALTER SCHEMA search_providers OWNER TO search_owner; -- DISABLED: Set after EF creates schema

-- Grant permissions only if schema exists (will be created by EF Core)
DO $$
BEGIN
    IF EXISTS (SELECT 1 FROM information_schema.schemata WHERE schema_name = 'search_providers') THEN
        -- Lock down PUBLIC access on this schema
        REVOKE ALL ON SCHEMA search_providers FROM PUBLIC;
        REVOKE ALL ON ALL TABLES IN SCHEMA search_providers FROM PUBLIC;
        REVOKE ALL ON ALL SEQUENCES IN SCHEMA search_providers FROM PUBLIC;
        
        GRANT USAGE ON SCHEMA search_providers TO search_role;
        GRANT SELECT, INSERT, UPDATE, DELETE ON ALL TABLES IN SCHEMA search_providers TO search_role;
        GRANT USAGE, SELECT ON ALL SEQUENCES IN SCHEMA search_providers TO search_role;
        
        -- Grant cross-schema permissions to app role
        GRANT USAGE ON SCHEMA search_providers TO meajudaai_app_role;
        GRANT SELECT, INSERT, UPDATE, DELETE ON ALL TABLES IN SCHEMA search_providers TO meajudaai_app_role;
        GRANT USAGE, SELECT ON ALL SEQUENCES IN SCHEMA search_providers TO meajudaai_app_role;
        
        -- Set default privileges for future tables and sequences created by search_owner
        ALTER DEFAULT PRIVILEGES FOR ROLE search_owner IN SCHEMA search_providers GRANT SELECT, INSERT, UPDATE, DELETE ON TABLES TO search_role;
        ALTER DEFAULT PRIVILEGES FOR ROLE search_owner IN SCHEMA search_providers GRANT USAGE, SELECT ON SEQUENCES TO search_role;
        
        -- Set default privileges for app role on objects created by search_owner
        ALTER DEFAULT PRIVILEGES FOR ROLE search_owner IN SCHEMA search_providers GRANT SELECT, INSERT, UPDATE, DELETE ON TABLES TO meajudaai_app_role;
        ALTER DEFAULT PRIVILEGES FOR ROLE search_owner IN SCHEMA search_providers GRANT USAGE, SELECT ON SEQUENCES TO meajudaai_app_role;
        
        -- Set default search path for search_role
        ALTER ROLE search_role SET search_path = search_providers, public;
    END IF;
END;
$$ LANGUAGE plpgsql;

-- Grant cross-schema permissions with conditional check
DO $$
BEGIN
    -- Grant read-only access to providers schema (for denormalization sync)
    IF EXISTS (SELECT 1 FROM information_schema.schemata WHERE schema_name = 'providers') THEN
        GRANT USAGE ON SCHEMA providers TO search_role;
        GRANT SELECT ON ALL TABLES IN SCHEMA providers TO search_role;
    END IF;
END;
$$ LANGUAGE plpgsql;

-- PostGIS spatial reference system access (only if PostGIS is installed)
DO $$
BEGIN
    IF EXISTS (SELECT 1 FROM information_schema.tables WHERE table_schema = 'public' AND table_name = 'spatial_ref_sys') THEN
        GRANT SELECT ON TABLE public.spatial_ref_sys TO search_role;
        GRANT SELECT ON TABLE public.spatial_ref_sys TO meajudaai_app_role;
    END IF;
END;
$$ LANGUAGE plpgsql;

-- Search Providers schema purpose (only if schema exists)
DO $$
BEGIN
    IF EXISTS (SELECT 1 FROM information_schema.schemata WHERE schema_name = 'search_providers') THEN
        COMMENT ON SCHEMA search_providers IS 'Search & Discovery module - Geospatial provider search with PostGIS';
    END IF;
END;
$$ LANGUAGE plpgsql;
