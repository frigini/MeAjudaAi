-- Locations Module - Permissions
-- Grant permissions for locations module (CEP lookup and geocoding)

-- Create locations schema if it doesn't exist
CREATE SCHEMA IF NOT EXISTS locations;

-- Set explicit schema ownership
ALTER SCHEMA locations OWNER TO location_owner;

-- Lock down PUBLIC access on this schema
REVOKE ALL ON SCHEMA locations FROM PUBLIC;
REVOKE ALL ON ALL TABLES IN SCHEMA locations FROM PUBLIC;
REVOKE ALL ON ALL SEQUENCES IN SCHEMA locations FROM PUBLIC;

GRANT USAGE ON SCHEMA locations TO location_role;
GRANT SELECT, INSERT, UPDATE, DELETE ON ALL TABLES IN SCHEMA locations TO location_role;
GRANT USAGE, SELECT ON ALL SEQUENCES IN SCHEMA locations TO location_role;

-- Set default privileges for future tables and sequences created by location_owner
ALTER DEFAULT PRIVILEGES FOR ROLE location_owner IN SCHEMA locations GRANT SELECT, INSERT, UPDATE, DELETE ON TABLES TO location_role;
ALTER DEFAULT PRIVILEGES FOR ROLE location_owner IN SCHEMA locations GRANT USAGE, SELECT ON SEQUENCES TO location_role;

-- Set default search path for location_role
ALTER ROLE location_role SET search_path = locations, public;

-- Grant cross-schema permissions to app role
GRANT USAGE ON SCHEMA locations TO meajudaai_app_role;
GRANT SELECT, INSERT, UPDATE, DELETE ON ALL TABLES IN SCHEMA locations TO meajudaai_app_role;
GRANT USAGE, SELECT ON ALL SEQUENCES IN SCHEMA locations TO meajudaai_app_role;

-- Set default privileges for app role on objects created by location_owner
ALTER DEFAULT PRIVILEGES FOR ROLE location_owner IN SCHEMA locations GRANT SELECT, INSERT, UPDATE, DELETE ON TABLES TO meajudaai_app_role;
ALTER DEFAULT PRIVILEGES FOR ROLE location_owner IN SCHEMA locations GRANT USAGE, SELECT ON SEQUENCES TO meajudaai_app_role;

-- Document schema purpose
COMMENT ON SCHEMA locations IS 'Locations module - CEP lookup, address validation, and geocoding services';
