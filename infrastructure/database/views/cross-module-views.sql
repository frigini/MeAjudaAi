-- Cross-Module Database Views
-- These views allow controlled access across module boundaries

-- ============================
-- User Summary View
-- ============================
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

-- ============================
-- Documents Module Views
-- ============================

-- Document Status Summary (for providers module to check verification status)
CREATE OR REPLACE VIEW meajudaai_app.document_status_summary AS
SELECT 
    d.id,
    d.provider_id,
    d.document_type,
    d.status,
    d.uploaded_at,
    d.verified_at,
    CASE 
        WHEN d.status = 3 THEN true  -- EDocumentStatus.Verified
        ELSE false
    END AS is_verified,
    CASE 
        WHEN d.status = 4 THEN true  -- EDocumentStatus.Rejected
        ELSE false
    END AS is_rejected,
    CASE 
        WHEN d.status IN (1, 2) THEN true  -- EDocumentStatus.Uploaded, PendingVerification
        ELSE false
    END AS is_pending
FROM documents.documents d;

-- Grant read access to providers and app roles
GRANT SELECT ON meajudaai_app.document_status_summary TO providers_role;
GRANT SELECT ON meajudaai_app.document_status_summary TO meajudaai_app_role;

-- Provider Documents Summary (aggregate count by provider and status)
CREATE OR REPLACE VIEW meajudaai_app.provider_documents_summary AS
SELECT 
    d.provider_id,
    COUNT(*) AS total_documents,
    COUNT(*) FILTER (WHERE d.status = 3) AS verified_count,
    COUNT(*) FILTER (WHERE d.status = 4) AS rejected_count,
    COUNT(*) FILTER (WHERE d.status IN (1, 2)) AS pending_count,
    MAX(d.uploaded_at) AS last_upload_date,
    MAX(d.verified_at) AS last_verification_date
FROM documents.documents d
GROUP BY d.provider_id;

-- Grant read access to providers and app roles
GRANT SELECT ON meajudaai_app.provider_documents_summary TO providers_role;
GRANT SELECT ON meajudaai_app.provider_documents_summary TO meajudaai_app_role;

COMMENT ON VIEW meajudaai_app.document_status_summary IS 'Cross-module view for document verification status (used by providers module)';
COMMENT ON VIEW meajudaai_app.provider_documents_summary IS 'Aggregated document statistics per provider';