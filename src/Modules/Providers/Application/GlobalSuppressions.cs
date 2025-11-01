using System.Diagnostics.CodeAnalysis;

// Suppressões específicas para o módulo Providers

// S1006: Default parameter values - Suprimimos para handlers que seguem padrão CQRS
[assembly: SuppressMessage("Style", "S1006:Add the default parameter value defined in the overridden method", 
    Justification = "Handlers CQRS seguem padrão específico, parâmetros default não são necessários", 
    Scope = "namespaceanddescendants", Target = "~N:MeAjudaAi.Modules.Providers.Application.Handlers")]