-- ==================================================
-- Seed Script: ServiceCatalogs Module
-- Description: Inserts initial ServiceCategories and Services
-- Usage: Run only in Development environment
-- ==================================================

-- Check if data already exists
DO $$
BEGIN
    IF EXISTS (SELECT 1 FROM meajudaai_service_catalogs."ServiceCategories" LIMIT 1) THEN
        RAISE NOTICE 'ServiceCategories already seeded. Skipping...';
        RETURN;
    END IF;

    RAISE NOTICE 'Seeding ServiceCategories and Services...';

    -- Insert Service Categories
    INSERT INTO meajudaai_service_catalogs."ServiceCategories" ("Id", "Name", "Description", "IsActive", "DisplayOrder", "CreatedAt", "UpdatedAt")
    VALUES
        ('10000000-0000-0000-0000-000000000001'::uuid, 'Saúde', 'Serviços de saúde, cuidados médicos e bem-estar', true, 1, NOW(), NOW()),
        ('10000000-0000-0000-0000-000000000002'::uuid, 'Educação', 'Serviços educacionais, reforço escolar e cursos', true, 2, NOW(), NOW()),
        ('10000000-0000-0000-0000-000000000003'::uuid, 'Assistência Social', 'Apoio social, orientação e assistência humanitária', true, 3, NOW(), NOW()),
        ('10000000-0000-0000-0000-000000000004'::uuid, 'Jurídico', 'Assessoria jurídica, orientação legal e mediação', true, 4, NOW(), NOW()),
        ('10000000-0000-0000-0000-000000000005'::uuid, 'Habitação', 'Serviços relacionados a moradia e infraestrutura residencial', true, 5, NOW(), NOW()),
        ('10000000-0000-0000-0000-000000000006'::uuid, 'Transporte', 'Serviços de transporte e mobilidade urbana', true, 6, NOW(), NOW()),
        ('10000000-0000-0000-0000-000000000007'::uuid, 'Alimentação', 'Apoio alimentar, refeições e programas nutricionais', true, 7, NOW(), NOW()),
        ('10000000-0000-0000-0000-000000000008'::uuid, 'Trabalho e Renda', 'Capacitação profissional, intermediação de emprego e geração de renda', true, 8, NOW(), NOW());

    RAISE NOTICE 'Inserted % service categories', 8;

    -- Insert Services
    INSERT INTO meajudaai_service_catalogs."Services" ("Id", "Name", "Description", "CategoryId", "DisplayOrder", "IsActive", "CreatedAt", "UpdatedAt")
    VALUES
        -- Saúde
        ('20000000-0000-0000-0000-000000000001'::uuid, 'Consulta Médica Geral', 'Atendimento médico clínico geral', '10000000-0000-0000-0000-000000000001'::uuid, 1, true, NOW(), NOW()),
        ('20000000-0000-0000-0000-000000000002'::uuid, 'Atendimento Psicológico', 'Sessões de terapia e apoio psicológico', '10000000-0000-0000-0000-000000000001'::uuid, 2, true, NOW(), NOW()),
        ('20000000-0000-0000-0000-000000000003'::uuid, 'Fisioterapia', 'Tratamento fisioterapêutico e reabilitação', '10000000-0000-0000-0000-000000000001'::uuid, 3, true, NOW(), NOW()),
        
        -- Educação
        ('20000000-0000-0000-0000-000000000004'::uuid, 'Reforço Escolar', 'Aulas de reforço para ensino fundamental e médio', '10000000-0000-0000-0000-000000000002'::uuid, 1, true, NOW(), NOW()),
        ('20000000-0000-0000-0000-000000000005'::uuid, 'Alfabetização de Adultos', 'Programa de alfabetização para adultos', '10000000-0000-0000-0000-000000000002'::uuid, 2, true, NOW(), NOW()),
        
        -- Assistência Social
        ('20000000-0000-0000-0000-000000000006'::uuid, 'Orientação Social', 'Orientação e encaminhamento para programas sociais', '10000000-0000-0000-0000-000000000003'::uuid, 1, true, NOW(), NOW()),
        ('20000000-0000-0000-0000-000000000007'::uuid, 'Apoio a Famílias', 'Assistência e suporte a núcleos familiares', '10000000-0000-0000-0000-000000000003'::uuid, 2, true, NOW(), NOW()),
        
        -- Jurídico
        ('20000000-0000-0000-0000-000000000008'::uuid, 'Orientação Jurídica Gratuita', 'Consulta jurídica básica sem custos', '10000000-0000-0000-0000-000000000004'::uuid, 1, true, NOW(), NOW()),
        ('20000000-0000-0000-0000-000000000009'::uuid, 'Mediação de Conflitos', 'Serviço de mediação para resolução de conflitos', '10000000-0000-0000-0000-000000000004'::uuid, 2, true, NOW(), NOW()),
        
        -- Habitação
        ('20000000-0000-0000-0000-000000000010'::uuid, 'Reparos Residenciais', 'Serviços de manutenção e reparos em residências', '10000000-0000-0000-0000-000000000005'::uuid, 1, true, NOW(), NOW()),
        
        -- Trabalho e Renda
        ('20000000-0000-0000-0000-000000000011'::uuid, 'Capacitação Profissional', 'Cursos de qualificação e capacitação profissional', '10000000-0000-0000-0000-000000000008'::uuid, 1, true, NOW(), NOW()),
        ('20000000-0000-0000-0000-000000000012'::uuid, 'Intermediação de Emprego', 'Apoio na busca e colocação profissional', '10000000-0000-0000-0000-000000000008'::uuid, 2, true, NOW(), NOW());

    RAISE NOTICE 'Inserted % services', 12;
    RAISE NOTICE 'ServiceCatalogs seeding completed successfully!';

END $$;
