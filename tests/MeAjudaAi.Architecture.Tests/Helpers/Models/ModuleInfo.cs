using System.Reflection;

namespace MeAjudaAi.Architecture.Tests.Helpers.Models
{

    /// <summary>
    /// Informações sobre um módulo descoberto
    /// </summary>
    public class ModuleInfo
    {
        public required string Name { get; init; }
        public Assembly? DomainAssembly { get; init; }
        public Assembly? ApplicationAssembly { get; init; }
        public Assembly? InfrastructureAssembly { get; init; }
        public Assembly? ApiAssembly { get; init; }

        public override string ToString() => Name;
    }
}
