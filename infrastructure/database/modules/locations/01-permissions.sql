-- Locations Module - Permissions
-- Grant permissions for locations module (CEP lookup and geocoding)

-- Create locations schema if it doesn't exist
-- CREATE SCHEMA IF NOT EXISTS locations; -- DISABLED: Schema created by EF Core

-- Set explicit schema ownership
-- ALTER SCHEMA locations OWNER TO location_owner; -- DISABLED: Set after EF creates schema

-- Grant permissions only if schema exists (will be created by EF Core)
DO $$
BEGIN
    IF EXISTS (SELECT 1 FROM information_schema.schemata WHERE schema_name = 'locations') THEN
        -- Lock down PUBLIC access on this schema
        REVOKE ALL ON SCHEMA locations FROM PUBLIC;
        REVOKE ALL ON ALL TABLES IN SCHEMA locations FROM PUBLIC;
        REVOKE ALL ON ALL SEQUENCES IN SCHEMA locations FROM PUBLIC;
        
        GRANT USAGE ON SCHEMA locations TO location_role;
        GRANT SELECT, INSERT, UPDATE, DELETE ON ALL TABLES IN SCHEMA locations TO location_role;
        GRANT USAGE, SELECT ON ALL SEQUENCES IN SCHEMA locations TO location_role;
        
        -- Grant cross-schema permissions to app role
        GRANT USAGE ON SCHEMA locations TO meajudaai_app_role;
        GRANT SELECT, INSERT, UPDATE, DELETE ON ALL TABLES IN SCHEMA locations TO meajudaai_app_role;
        GRANT USAGE, SELECT ON ALL SEQUENCES IN SCHEMA locations TO meajudaai_app_role;
        
        -- Set default privileges for future tables and sequences created by location_owner
        ALTER DEFAULT PRIVILEGES FOR ROLE location_owner IN SCHEMA locations GRANT SELECT, INSERT, UPDATE, DELETE ON TABLES TO location_role;
        ALTER DEFAULT PRIVILEGES FOR ROLE location_owner IN SCHEMA locations GRANT USAGE, SELECT ON SEQUENCES TO location_role;
        
        -- Set default privileges for app role on objects created by location_owner
        ALTER DEFAULT PRIVILEGES FOR ROLE location_owner IN SCHEMA locations GRANT SELECT, INSERT, UPDATE, DELETE ON TABLES TO meajudaai_app_role;
        ALTER DEFAULT PRIVILEGES FOR ROLE location_owner IN SCHEMA locations GRANT USAGE, SELECT ON SEQUENCES TO meajudaai_app_role;
        
        -- Set default search path for location_role
        ALTER ROLE location_role SET search_path = locations, public;
    END IF;
END;
$$ LANGUAGE plpgsql;

-- Document schema purpose
COMMENT ON SCHEMA locations IS 'Locations module - CEP lookup, address validation, and geocoding services';
