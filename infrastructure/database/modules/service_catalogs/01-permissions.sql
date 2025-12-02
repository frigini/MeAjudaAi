-- Service Catalogs Module - Permissions
-- Grant permissions for service_catalogs module (service catalog management)

-- Create service_catalogs schema if it doesn't exist
CREATE SCHEMA IF NOT EXISTS meajudaai_servicecatalogs;

-- Set explicit schema ownership
ALTER SCHEMA meajudaai_servicecatalogs OWNER TO catalogs_owner;

GRANT USAGE ON SCHEMA meajudaai_servicecatalogs TO catalogs_role;
GRANT SELECT, INSERT, UPDATE, DELETE ON ALL TABLES IN SCHEMA meajudaai_servicecatalogs TO catalogs_role;
GRANT USAGE, SELECT ON ALL SEQUENCES IN SCHEMA meajudaai_servicecatalogs TO catalogs_role;

-- Set default privileges for future tables and sequences created by catalogs_owner
ALTER DEFAULT PRIVILEGES FOR ROLE catalogs_owner IN SCHEMA meajudaai_servicecatalogs GRANT SELECT, INSERT, UPDATE, DELETE ON TABLES TO catalogs_role;
ALTER DEFAULT PRIVILEGES FOR ROLE catalogs_owner IN SCHEMA meajudaai_servicecatalogs GRANT USAGE, SELECT ON SEQUENCES TO catalogs_role;

-- Set default search path for catalogs_role
ALTER ROLE catalogs_role SET search_path = meajudaai_servicecatalogs, public;

-- Grant cross-schema permissions to app role
GRANT USAGE ON SCHEMA meajudaai_servicecatalogs TO meajudaai_app_role;
GRANT SELECT, INSERT, UPDATE, DELETE ON ALL TABLES IN SCHEMA meajudaai_servicecatalogs TO meajudaai_app_role;
GRANT USAGE, SELECT ON ALL SEQUENCES IN SCHEMA meajudaai_servicecatalogs TO meajudaai_app_role;

-- Set default privileges for app role on objects created by catalogs_owner
ALTER DEFAULT PRIVILEGES FOR ROLE catalogs_owner IN SCHEMA meajudaai_servicecatalogs GRANT SELECT, INSERT, UPDATE, DELETE ON TABLES TO meajudaai_app_role;
ALTER DEFAULT PRIVILEGES FOR ROLE catalogs_owner IN SCHEMA meajudaai_servicecatalogs GRANT USAGE, SELECT ON SEQUENCES TO meajudaai_app_role;

-- Grant read-only access to providers schema (for future ProviderServices integration)
GRANT USAGE ON SCHEMA meajudaai_providers TO catalogs_role;
GRANT SELECT ON ALL TABLES IN SCHEMA meajudaai_providers TO catalogs_role;

-- Grant read-only access to search_providers schema (for denormalization of services)
GRANT USAGE ON SCHEMA meajudaai_searchproviders TO catalogs_role;
GRANT SELECT ON ALL TABLES IN SCHEMA meajudaai_searchproviders TO catalogs_role;

-- Document schema purpose
COMMENT ON SCHEMA meajudaai_servicecatalogs IS 'Service Catalog module - Admin-managed service categories and services';
