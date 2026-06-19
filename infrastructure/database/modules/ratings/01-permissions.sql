-- Ratings Module - Permissions

DO $$
BEGIN
    IF EXISTS (SELECT 1 FROM information_schema.schemata WHERE schema_name = '{{SCHEMA_NAME_LITERAL}}') THEN
        -- Grant immediate permissions on existing objects
        GRANT USAGE ON SCHEMA {{SCHEMA_NAME}} TO {{ROLE_NAME}};
        GRANT SELECT, INSERT, UPDATE, DELETE ON ALL TABLES IN SCHEMA {{SCHEMA_NAME}} TO {{ROLE_NAME}};
        GRANT USAGE, SELECT ON ALL SEQUENCES IN SCHEMA {{SCHEMA_NAME}} TO {{ROLE_NAME}};
        
        -- Grant cross-schema permissions to app role
        GRANT USAGE ON SCHEMA {{SCHEMA_NAME}} TO {{APP_ROLE_NAME}};
        GRANT SELECT, INSERT, UPDATE, DELETE ON ALL TABLES IN SCHEMA {{SCHEMA_NAME}} TO {{APP_ROLE_NAME}};
        GRANT USAGE, SELECT ON ALL SEQUENCES IN SCHEMA {{SCHEMA_NAME}} TO {{APP_ROLE_NAME}};
        
        -- Set default privileges for future tables and sequences created by schema owner
        ALTER DEFAULT PRIVILEGES FOR ROLE {{ROLE_OWNER_NAME}} IN SCHEMA {{SCHEMA_NAME}} GRANT SELECT, INSERT, UPDATE, DELETE ON TABLES TO {{ROLE_NAME}};
        ALTER DEFAULT PRIVILEGES FOR ROLE {{ROLE_OWNER_NAME}} IN SCHEMA {{SCHEMA_NAME}} GRANT USAGE, SELECT ON SEQUENCES TO {{ROLE_NAME}};
        
        -- Set default privileges for app role on objects created by schema owner
        ALTER DEFAULT PRIVILEGES FOR ROLE {{ROLE_OWNER_NAME}} IN SCHEMA {{SCHEMA_NAME}} GRANT SELECT, INSERT, UPDATE, DELETE ON TABLES TO {{APP_ROLE_NAME}};
        ALTER DEFAULT PRIVILEGES FOR ROLE {{ROLE_OWNER_NAME}} IN SCHEMA {{SCHEMA_NAME}} GRANT USAGE, SELECT ON SEQUENCES TO {{APP_ROLE_NAME}};
        
        -- Set default search path
        ALTER ROLE {{ROLE_NAME}} SET search_path = {{SCHEMA_NAME}}, public;
        ALTER ROLE {{APP_ROLE_NAME}} SET search_path = {{SCHEMA_NAME}}, public;
    END IF;
END;
$$ LANGUAGE plpgsql;
