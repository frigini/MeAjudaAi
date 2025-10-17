using System.Diagnostics.CodeAnalysis;

// Global suppressions for code analysis warnings that are acceptable in this codebase

// CA1062: Many extension methods and framework patterns don't require null validation
// for parameters that are guaranteed by the framework or calling context
[assembly: SuppressMessage("Design", "CA1062:Validate arguments of public methods", 
    Justification = "Framework patterns and extension methods often have guaranteed non-null parameters")]

// CA1034: Nested types used for organization in static classes (constants, configuration)
[assembly: SuppressMessage("Design", "CA1034:Nested types should not be visible", 
    Scope = "namespaceanddescendants", Target = "~N:MeAjudaAi.Shared.Constants",
    Justification = "Nested types used for logical organization of constants")]

// CA1819: Properties returning arrays for configuration and options classes
[assembly: SuppressMessage("Performance", "CA1819:Properties should not return arrays", 
    Scope = "namespaceanddescendants", Target = "~N:MeAjudaAi.Shared.Messaging",
    Justification = "Configuration classes need array properties for framework integration")]

// CA2000: Dispose warnings for meters that are managed by DI container
[assembly: SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope",
    Scope = "namespaceanddescendants", Target = "~N:MeAjudaAi.Shared.Caching",
    Justification = "Meters are managed by DI container lifecycle")]

[assembly: SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope",
    Scope = "namespaceanddescendants", Target = "~N:MeAjudaAi.Shared.Database",
    Justification = "Meters are managed by DI container lifecycle")]

// CA1805: Explicit initialization warnings for value types
[assembly: SuppressMessage("Performance", "CA1805:Do not initialize unnecessarily",
    Scope = "namespaceanddescendants", Target = "~N:MeAjudaAi.Shared.Functional",
    Justification = "Explicit initialization for clarity in functional programming patterns")]

// CA1508: Dead code warnings for generic type checks
[assembly: SuppressMessage("Maintainability", "CA1508:Avoid dead conditional code",
    Scope = "namespaceanddescendants", Target = "~N:MeAjudaAi.Shared.Caching",
    Justification = "Generic type checks may appear as dead code but are needed for runtime behavior")]