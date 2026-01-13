-- Identity Schema Initialization for Keycloak
-- This schema is used by Keycloak for user authentication and authorization
-- Must be created before Keycloak starts

-- Create identity schema for Keycloak
CREATE SCHEMA IF NOT EXISTS identity;

-- Grant necessary permissions to postgres user (used by Keycloak)
GRANT ALL PRIVILEGES ON SCHEMA identity TO postgres;
GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA identity TO postgres;
GRANT ALL PRIVILEGES ON ALL SEQUENCES IN SCHEMA identity TO postgres;

-- Set default privileges for future objects created by postgres in identity schema
ALTER DEFAULT PRIVILEGES FOR ROLE postgres IN SCHEMA identity 
    GRANT ALL PRIVILEGES ON TABLES TO postgres;

ALTER DEFAULT PRIVILEGES FOR ROLE postgres IN SCHEMA identity 
    GRANT ALL PRIVILEGES ON SEQUENCES TO postgres;

COMMENT ON SCHEMA identity IS 'Keycloak identity and access management schema';
