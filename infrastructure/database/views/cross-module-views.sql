-- Cross-Module Database Views
-- These views allow controlled access across module boundaries

-- User Summary View (for other modules that need user information)
-- CREATE VIEW public.user_summary AS
-- SELECT 
--     id,
--     username,
--     email,
--     created_at
-- FROM users.users 
-- WHERE is_active = true;

-- Grant read access to other modules when implemented
-- GRANT SELECT ON public.user_summary TO other_module_role;