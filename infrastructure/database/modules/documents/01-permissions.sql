-- Documents Module - Permissions
-- Grant permissions for documents module

-- Create documents schema if it doesn't exist
-- CREATE SCHEMA IF NOT EXISTS documents; -- DISABLED: Schema created by EF Core

-- Set explicit schema ownership
-- ALTER SCHEMA documents OWNER TO documents_owner; -- DISABLED: Set after EF creates schema

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

-- Grant schema permissions to hangfire_role (USAGE only - objects are created by migrations)
GRANT USAGE ON SCHEMA hangfire TO hangfire_role;

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
-- Following principle of least privilege: only UPDATE on specific table, not all tables
GRANT USAGE ON SCHEMA documents TO hangfire_role;
-- NOTE: GRANT on specific tables commented out - will be applied after EF Core migrations create the tables
-- GRANT SELECT ON ALL TABLES IN SCHEMA documents TO hangfire_role;
-- GRANT UPDATE ON documents.documents TO hangfire_role; -- Only documents table needs UPDATE
ALTER DEFAULT PRIVILEGES FOR ROLE documents_owner IN SCHEMA documents GRANT SELECT ON TABLES TO hangfire_role;
-- Note: Future tables will only get SELECT by default; UPDATE must be explicitly granted

-- ============================
-- Application Schema (if not created by users module)
-- ============================

-- Create dedicated application schema for cross-cutting objects
CREATE SCHEMA IF NOT EXISTS meajudaai_app;

-- Set explicit schema ownership
ALTER SCHEMA meajudaai_app OWNER TO meajudaai_app_owner;

-- Grant permissions on dedicated application schema
GRANT USAGE, CREATE ON SCHEMA meajudaai_app TO meajudaai_app_role;

-- Set search_path for app role (centralized here to avoid conflicts)
-- NOTE: This must include ALL module schemas to ensure cross-module queries work.
-- Documents module runs last alphabetically, so this setting takes precedence.
ALTER ROLE meajudaai_app_role SET search_path = meajudaai_app, documents, users, providers, hangfire, public;

-- Grant limited permissions on public schema (read-only)
GRANT USAGE ON SCHEMA public TO documents_role;
GRANT USAGE ON SCHEMA public TO hangfire_role;
GRANT USAGE ON SCHEMA public TO meajudaai_app_role;

-- Harden public schema by revoking CREATE from PUBLIC (security best practice)
REVOKE CREATE ON SCHEMA public FROM PUBLIC;
