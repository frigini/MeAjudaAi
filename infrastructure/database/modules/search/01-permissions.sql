-- Search Module - Permissions
-- Grant permissions for search module (geospatial search with PostGIS)

-- Enable PostGIS extension if not already enabled
CREATE EXTENSION IF NOT EXISTS postgis;

-- Create search schema if it doesn't exist
CREATE SCHEMA IF NOT EXISTS search;

-- Set explicit schema ownership
ALTER SCHEMA search OWNER TO search_owner;

GRANT USAGE ON SCHEMA search TO search_role;
GRANT SELECT, INSERT, UPDATE, DELETE ON ALL TABLES IN SCHEMA search TO search_role;
GRANT USAGE, SELECT ON ALL SEQUENCES IN SCHEMA search TO search_role;

-- Set default privileges for future tables and sequences created by search_owner
ALTER DEFAULT PRIVILEGES FOR ROLE search_owner IN SCHEMA search GRANT SELECT, INSERT, UPDATE, DELETE ON TABLES TO search_role;
ALTER DEFAULT PRIVILEGES FOR ROLE search_owner IN SCHEMA search GRANT USAGE, SELECT ON SEQUENCES TO search_role;

-- Set default search path for search_role
ALTER ROLE search_role SET search_path = search, public;

-- Grant cross-schema permissions to app role
GRANT USAGE ON SCHEMA search TO meajudaai_app_role;
GRANT SELECT, INSERT, UPDATE, DELETE ON ALL TABLES IN SCHEMA search TO meajudaai_app_role;
GRANT USAGE, SELECT ON ALL SEQUENCES IN SCHEMA search TO meajudaai_app_role;

-- Set default privileges for app role on objects created by search_owner
ALTER DEFAULT PRIVILEGES FOR ROLE search_owner IN SCHEMA search GRANT SELECT, INSERT, UPDATE, DELETE ON TABLES TO meajudaai_app_role;
ALTER DEFAULT PRIVILEGES FOR ROLE search_owner IN SCHEMA search GRANT USAGE, SELECT ON SEQUENCES TO meajudaai_app_role;

-- Grant read-only access to providers schema (for denormalization sync)
GRANT USAGE ON SCHEMA providers TO search_role;
GRANT SELECT ON ALL TABLES IN SCHEMA providers TO search_role;

-- PostGIS spatial reference system access
GRANT SELECT ON TABLE spatial_ref_sys TO search_role;
GRANT SELECT ON TABLE spatial_ref_sys TO meajudaai_app_role;

-- Document schema purpose
COMMENT ON SCHEMA search IS 'Search & Discovery module - Geospatial provider search with PostGIS';
