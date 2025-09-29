-- Standalone PostgreSQL Initialization Script
-- Basic database setup for development and testing

-- Create extensions that might be useful for development
CREATE EXTENSION IF NOT EXISTS "pgcrypto";

-- Create a basic schema for development
CREATE SCHEMA IF NOT EXISTS app;

-- Grant permissions to the default user
GRANT ALL PRIVILEGES ON SCHEMA app TO postgres;
GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA app TO postgres;
GRANT ALL PRIVILEGES ON ALL SEQUENCES IN SCHEMA app TO postgres;
GRANT ALL PRIVILEGES ON ALL FUNCTIONS IN SCHEMA app TO postgres;

-- Create a simple users table for testing
CREATE TABLE IF NOT EXISTS app.users (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    username VARCHAR(255) NOT NULL UNIQUE,
    email VARCHAR(255) NOT NULL UNIQUE,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
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
ON CONFLICT (username) DO NOTHING;

-- Log the initialization
DO $$
BEGIN
    RAISE NOTICE 'Standalone PostgreSQL initialized successfully';
    RAISE NOTICE 'Created schema: app';
    RAISE NOTICE 'Created table: app.users with % sample records', (SELECT COUNT(*) FROM app.users);
END $$;