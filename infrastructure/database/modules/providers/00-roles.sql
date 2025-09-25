-- Users Module - Database Roles
-- Create dedicated role for users module
CREATE ROLE users_role LOGIN PASSWORD 'users_secret';

-- Create general application role for cross-cutting operations
CREATE ROLE meajudaai_app_role LOGIN PASSWORD 'app_secret';

-- Grant users role to app role for cross-module access
GRANT users_role TO meajudaai_app_role;