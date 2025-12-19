using System.Diagnostics.CodeAnalysis;

// Supressões globais para avisos de análise de código que são aceitáveis nesta base de código

// CA1062: Muitos métodos de extensão e padrões do framework não requerem validação nula
// para parâmetros que são garantidos pelo framework ou contexto de chamada
[assembly: SuppressMessage("Design", "CA1062:Validate arguments of public methods",
    Justification = "Padrões do framework e métodos de extensão frequentemente têm parâmetros garantidamente não-nulos")]

// CA1034: Tipos aninhados usados para organização em classes estáticas (constantes, configuração)
[assembly: SuppressMessage("Design", "CA1034:Nested types should not be visible",
    Scope = "namespaceanddescendants", Target = "~N:MeAjudaAi.Shared.Constants",
    Justification = "Tipos aninhados usados para organização lógica de constantes")]

// CA1819: Propriedades retornando arrays para classes de configuração e opções
[assembly: SuppressMessage("Performance", "CA1819:Properties should not return arrays",
    Scope = "namespaceanddescendants", Target = "~N:MeAjudaAi.Shared.Messaging",
    Justification = "Classes de configuração precisam de propriedades array para integração com o framework")]

// CA2000: Avisos de dispose para meters que são gerenciados pelo container DI
[assembly: SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope",
    Scope = "namespaceanddescendants", Target = "~N:MeAjudaAi.Shared.Caching",
    Justification = "Meters são gerenciados pelo ciclo de vida do container DI")]

[assembly: SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope",
    Scope = "namespaceanddescendants", Target = "~N:MeAjudaAi.Shared.Database",
    Justification = "Meters são gerenciados pelo ciclo de vida do container DI")]

// CA1805: Avisos de inicialização explícita para tipos de valor
[assembly: SuppressMessage("Performance", "CA1805:Do not initialize unnecessarily",
    Scope = "namespaceanddescendants", Target = "~N:MeAjudaAi.Shared.Functional",
    Justification = "Inicialização explícita para clareza em padrões de programação funcional")]

// CA1508: Avisos de código morto para verificações de tipo genérico
[assembly: SuppressMessage("Maintainability", "CA1508:Avoid dead conditional code",
    Scope = "namespaceanddescendants", Target = "~N:MeAjudaAi.Shared.Caching",
    Justification = "Verificações de tipo genérico podem parecer código morto mas são necessárias para comportamento em tempo de execução")]

// CA1008: Enums should have zero value - Suprimimos para enums de negócio onde "None" não faz sentido
[assembly: SuppressMessage("Design", "CA1008:Enums should have zero value",
    Justification = "Enums de negócio não precisam de valor zero/None",
    Scope = "type", Target = "~T:MeAjudaAi.Modules.Providers.Domain.Enums.EDocumentType")]

[assembly: SuppressMessage("Design", "CA1008:Enums should have zero value",
    Justification = "Enums de negócio não precisam de valor zero/None",
    Scope = "type", Target = "~T:MeAjudaAi.Modules.Providers.Domain.Enums.EProviderType")]

[assembly: SuppressMessage("Design", "CA1008:Enums should have zero value",
    Justification = "Enums de negócio não precisam de valor zero/None",
    Scope = "type", Target = "~T:MeAjudaAi.Modules.Providers.Domain.Enums.EVerificationStatus")]
