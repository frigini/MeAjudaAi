-- Location Module - Permissions
-- Grant permissions for location module (CEP lookup and geocoding)

-- Create location schema if it doesn't exist
CREATE SCHEMA IF NOT EXISTS location;

-- Set explicit schema ownership
ALTER SCHEMA location OWNER TO location_owner;

-- Lock down PUBLIC access on this schema
REVOKE ALL ON SCHEMA location FROM PUBLIC;
REVOKE ALL ON ALL TABLES IN SCHEMA location FROM PUBLIC;
REVOKE ALL ON ALL SEQUENCES IN SCHEMA location FROM PUBLIC;

GRANT USAGE ON SCHEMA location TO location_role;
GRANT SELECT, INSERT, UPDATE, DELETE ON ALL TABLES IN SCHEMA location TO location_role;
GRANT USAGE, SELECT ON ALL SEQUENCES IN SCHEMA location TO location_role;

-- Set default privileges for future tables and sequences created by location_owner
ALTER DEFAULT PRIVILEGES FOR ROLE location_owner IN SCHEMA location GRANT SELECT, INSERT, UPDATE, DELETE ON TABLES TO location_role;
ALTER DEFAULT PRIVILEGES FOR ROLE location_owner IN SCHEMA location GRANT USAGE, SELECT ON SEQUENCES TO location_role;

-- Set default search path for location_role
ALTER ROLE location_role SET search_path = location, public;

-- Grant cross-schema permissions to app role
GRANT USAGE ON SCHEMA location TO meajudaai_app_role;
GRANT SELECT, INSERT, UPDATE, DELETE ON ALL TABLES IN SCHEMA location TO meajudaai_app_role;
GRANT USAGE, SELECT ON ALL SEQUENCES IN SCHEMA location TO meajudaai_app_role;

-- Set default privileges for app role on objects created by location_owner
ALTER DEFAULT PRIVILEGES FOR ROLE location_owner IN SCHEMA location GRANT SELECT, INSERT, UPDATE, DELETE ON TABLES TO meajudaai_app_role;
ALTER DEFAULT PRIVILEGES FOR ROLE location_owner IN SCHEMA location GRANT USAGE, SELECT ON SEQUENCES TO meajudaai_app_role;

-- Document schema purpose
COMMENT ON SCHEMA location IS 'Location module - CEP lookup, address validation, and geocoding services';
