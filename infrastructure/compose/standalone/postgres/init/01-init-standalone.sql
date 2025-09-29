-- Standalone PostgreSQL Initialization Script
-- Basic database setup for development and testing

-- Create extensions that might be useful for development
CREATE EXTENSION IF NOT EXISTS "pgcrypto";
-- Optional: case-insensitive text for username/email
CREATE EXTENSION IF NOT EXISTS "citext";

-- Create a basic schema for development
CREATE SCHEMA IF NOT EXISTS app;

-- Dev-only: application role (consider sourcing credentials from env)
DO $blk$
BEGIN
  PERFORM 1 FROM pg_roles WHERE rolname = 'meajudaai_app';
  IF NOT FOUND THEN
    CREATE ROLE meajudaai_app LOGIN PASSWORD 'change-me-in-dev';
  END IF;
END
$blk$;

ALTER SCHEMA app OWNER TO meajudaai_app;
GRANT USAGE, CREATE ON SCHEMA app TO meajudaai_app;
GRANT SELECT, INSERT, UPDATE, DELETE ON ALL TABLES    IN SCHEMA app TO meajudaai_app;
GRANT USAGE, SELECT                 ON ALL SEQUENCES  IN SCHEMA app TO meajudaai_app;
GRANT EXECUTE                      ON ALL FUNCTIONS  IN SCHEMA app TO meajudaai_app;
ALTER DEFAULT PRIVILEGES IN SCHEMA app
  GRANT SELECT, INSERT, UPDATE, DELETE ON TABLES   TO meajudaai_app;
ALTER DEFAULT PRIVILEGES IN SCHEMA app
  GRANT USAGE, SELECT                ON SEQUENCES TO meajudaai_app;
ALTER DEFAULT PRIVILEGES IN SCHEMA app
  GRANT EXECUTE                      ON FUNCTIONS TO meajudaai_app;

-- Create a simple users table for testing
CREATE TABLE IF NOT EXISTS app.users (
    id UUID PRIMARY KEY DEFAULT public.gen_random_uuid(),
    username CITEXT NOT NULL UNIQUE,
    email    CITEXT NOT NULL UNIQUE,
    created_at TIMESTAMPTZ NOT NULL DEFAULT now(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT now()
);

-- Create trigger function to automatically update updated_at timestamp
CREATE OR REPLACE FUNCTION app.touch_updated_at()
RETURNS trigger LANGUAGE plpgsql AS $$
BEGIN
  NEW.updated_at := CURRENT_TIMESTAMP;
  RETURN NEW;
END $$;

-- Create trigger to automatically update updated_at on row updates
DROP TRIGGER IF EXISTS trg_users_touch ON app.users;
CREATE TRIGGER trg_users_touch
BEFORE UPDATE ON app.users
FOR EACH ROW EXECUTE FUNCTION app.touch_updated_at();

-- Insert sample data for development
INSERT INTO app.users (username, email) 
VALUES 
    ('admin', 'admin@example.com'),
    ('developer', 'dev@example.com'),
    ('tester', 'test@example.com')
ON CONFLICT DO NOTHING;

-- Log the initialization
DO $$
BEGIN
    RAISE NOTICE 'Standalone PostgreSQL initialized successfully';
    RAISE NOTICE 'Created schema: app';
    RAISE NOTICE 'Created table: app.users with % sample records', (SELECT COUNT(*) FROM app.users);
END $$;