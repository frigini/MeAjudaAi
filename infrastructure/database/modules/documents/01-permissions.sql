-- Documents Module - Permissions
-- Grant permissions for documents module

-- Create documents schema if it doesn't exist
CREATE SCHEMA IF NOT EXISTS documents;

-- Set explicit schema ownership
ALTER SCHEMA documents OWNER TO documents_owner;

-- Grant schema usage and permissions for documents_role
GRANT USAGE ON SCHEMA documents TO documents_role;
GRANT SELECT, INSERT, UPDATE, DELETE ON ALL TABLES IN SCHEMA documents TO documents_role;
GRANT USAGE, SELECT ON ALL SEQUENCES IN SCHEMA documents TO documents_role;

-- Set default privileges for future tables and sequences created by documents_owner
ALTER DEFAULT PRIVILEGES FOR ROLE documents_owner IN SCHEMA documents GRANT SELECT, INSERT, UPDATE, DELETE ON TABLES TO documents_role;
ALTER DEFAULT PRIVILEGES FOR ROLE documents_owner IN SCHEMA documents GRANT USAGE, SELECT ON SEQUENCES TO documents_role;

-- Set default search path for documents_role
ALTER ROLE documents_role SET search_path = documents, public;

-- Grant cross-schema permissions to app role
GRANT USAGE ON SCHEMA documents TO meajudaai_app_role;
GRANT SELECT, INSERT, UPDATE, DELETE ON ALL TABLES IN SCHEMA documents TO meajudaai_app_role;
GRANT USAGE, SELECT ON ALL SEQUENCES IN SCHEMA documents TO meajudaai_app_role;

-- Set default privileges for app role on objects created by documents_owner
ALTER DEFAULT PRIVILEGES FOR ROLE documents_owner IN SCHEMA documents GRANT SELECT, INSERT, UPDATE, DELETE ON TABLES TO meajudaai_app_role;
ALTER DEFAULT PRIVILEGES FOR ROLE documents_owner IN SCHEMA documents GRANT USAGE, SELECT ON SEQUENCES TO meajudaai_app_role;

-- ============================
-- Hangfire Schema Permissions
-- ============================

-- Create hangfire schema if it doesn't exist (for background jobs storage)
CREATE SCHEMA IF NOT EXISTS hangfire;

-- Set explicit schema ownership
ALTER SCHEMA hangfire OWNER TO meajudaai_app_owner;

-- Grant full permissions on hangfire schema to hangfire_role
GRANT USAGE, CREATE ON SCHEMA hangfire TO hangfire_role;
GRANT ALL PRIVILEGES ON SCHEMA hangfire TO hangfire_role;

-- Grant permissions on all existing tables and sequences in hangfire schema
GRANT SELECT, INSERT, UPDATE, DELETE ON ALL TABLES IN SCHEMA hangfire TO hangfire_role;
GRANT USAGE, SELECT, UPDATE ON ALL SEQUENCES IN SCHEMA hangfire TO hangfire_role;

-- Set default privileges for future objects in hangfire schema
ALTER DEFAULT PRIVILEGES FOR ROLE meajudaai_app_owner IN SCHEMA hangfire GRANT SELECT, INSERT, UPDATE, DELETE ON TABLES TO hangfire_role;
ALTER DEFAULT PRIVILEGES FOR ROLE meajudaai_app_owner IN SCHEMA hangfire GRANT USAGE, SELECT, UPDATE ON SEQUENCES TO hangfire_role;

-- Allow hangfire to create functions (needed for Hangfire internal operations)
ALTER DEFAULT PRIVILEGES FOR ROLE meajudaai_app_owner IN SCHEMA hangfire GRANT EXECUTE ON FUNCTIONS TO hangfire_role;

-- Set search path for hangfire_role
ALTER ROLE hangfire_role SET search_path = hangfire, documents, users, providers, public;

-- Grant hangfire_role read access to documents schema (needed for DocumentVerificationJob)
GRANT USAGE ON SCHEMA documents TO hangfire_role;
GRANT SELECT, UPDATE ON ALL TABLES IN SCHEMA documents TO hangfire_role;
ALTER DEFAULT PRIVILEGES FOR ROLE documents_owner IN SCHEMA documents GRANT SELECT, UPDATE ON TABLES TO hangfire_role;

-- ============================
-- Application Schema (if not created by users module)
-- ============================

-- Create dedicated application schema for cross-cutting objects
CREATE SCHEMA IF NOT EXISTS meajudaai_app;

-- Set explicit schema ownership
ALTER SCHEMA meajudaai_app OWNER TO meajudaai_app_owner;

-- Grant permissions on dedicated application schema
GRANT USAGE, CREATE ON SCHEMA meajudaai_app TO meajudaai_app_role;

-- Update search path for app role (include documents schema)
ALTER ROLE meajudaai_app_role SET search_path = meajudaai_app, documents, users, providers, hangfire, public;

-- Grant limited permissions on public schema (read-only)
GRANT USAGE ON SCHEMA public TO documents_role;
GRANT USAGE ON SCHEMA public TO hangfire_role;
GRANT USAGE ON SCHEMA public TO meajudaai_app_role;

-- Harden public schema by revoking CREATE from PUBLIC (security best practice)
REVOKE CREATE ON SCHEMA public FROM PUBLIC;
