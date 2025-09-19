using System.Reflection;

namespace MeAjudaAi.Architecture.Tests;

/// <summary>
/// Testes de fronteiras de módulos garantindo isolamento adequado entre módulos
/// Crítico para integridade da arquitetura de monólito modular
/// </summary>
public class ModuleBoundaryTests
{
    private static readonly Assembly UsersApiAssembly = typeof(MeAjudaAi.Modules.Users.API.Mappers.RequestMapperExtensions).Assembly;
    private static readonly Assembly UsersApplicationAssembly = typeof(MeAjudaAi.Modules.Users.Application.Extensions).Assembly;
    private static readonly Assembly UsersInfrastructureAssembly = typeof(MeAjudaAi.Modules.Users.Infrastructure.Mappers.DomainEventMapperExtensions).Assembly;
    private static readonly Assembly UsersDomainAssembly = typeof(MeAjudaAi.Modules.Users.Domain.Entities.User).Assembly;

    [Fact]
    public void Users_Module_ShouldNotReference_OtherModules()
    {
        // O módulo Users não deve referenciar diretamente outros módulos
        // A comunicação deve acontecer apenas através de eventos de integração
        
        var userAssemblies = new[]
        {
            UsersApiAssembly,
            UsersApplicationAssembly,
            UsersInfrastructureAssembly,
            UsersDomainAssembly
        };

        foreach (var assembly in userAssemblies)
        {
            var result = Types.InAssembly(assembly)
                .Should()
                .NotHaveDependencyOnAny("MeAjudaAi.Modules.Providers", "MeAjudaAi.Modules.Orders") // Adicionar módulos futuros aqui
                .GetResult();

            result.IsSuccessful.Should().BeTrue(
                "Assembly do módulo Users {0} não deve referenciar outros módulos diretamente. " +
                "Violações: {1}", 
                assembly.GetName().Name,
                string.Join(", ", result.FailingTypes?.Select(t => t.FullName) ?? []));
        }
    }

    [Fact]
    public void Module_Internal_Types_ShouldNotBePublic()
    {
        // Implementações internas do módulo não devem ser expostas publicamente
        var result = Types.InAssembly(UsersInfrastructureAssembly)
            .That()
            .ResideInNamespaceContaining(".Persistence.Repositories")
            .Or()
            .ResideInNamespaceContaining(".Services")
            .Or()
            .ResideInNamespaceContaining(".Events.Handlers")
            .Should()
            .NotBePublic()
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            "Implementações internas do módulo não devem ser públicas. " +
            "Violações: {0}", 
            string.Join(", ", result.FailingTypes?.Select(t => t.FullName) ?? []));
    }

    [Fact]
    public void Module_Domain_ShouldOnlyDependOn_Shared()
    {
        // O domínio do módulo deve depender apenas de abstrações compartilhadas
        var referencedAssemblies = UsersDomainAssembly.GetReferencedAssemblies()
            .Where(a => a.Name?.StartsWith("MeAjudaAi") == true)
            .Select(a => a.Name)
            .ToList();

        referencedAssemblies.Should().OnlyContain(name => 
            name == "MeAjudaAi.Shared" || 
            name.StartsWith("System") ||
            name.StartsWith("Microsoft"),
            "Domínio deve referenciar apenas o projeto Shared e assemblies do framework. " +
            "Referências atuais: {0}", string.Join(", ", referencedAssemblies));
    }

    [Fact(Skip = "LIMITAÇÃO TÉCNICA: DbContext deve ser público para ferramentas de design-time do EF Core, mas conceitualmente deveria ser internal")]
    public void Module_DbContext_ShouldBeInternal()
    {
        // Conceitualmente, DbContext deveria ser internal para melhor encapsulamento do módulo
        // Porém, o EF Core exige que seja público para suas ferramentas de design-time funcionarem
        // Este teste documenta a arquitetura ideal, mesmo que não possa ser aplicada devido à limitação técnica
        var result = Types.InAssembly(UsersInfrastructureAssembly)
            .That()
            .HaveNameEndingWith("DbContext")
            .Should()
            .NotBePublic()
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            "DbContext deveria ser internal ao módulo para melhor encapsulamento. " +
            "Tipos DbContext públicos encontrados: {0}",
            string.Join(", ", result.FailingTypes?.Select(t => t.Name) ?? Array.Empty<string>()));
    }

    [Fact]
    public void Module_DbContext_ShouldNotBeReferencedOutsideInfrastructure()
    {
        // DbContext não deve ser referenciado fora da camada de Infrastructure
        var dbContextTypeNames = Types.InAssembly(UsersInfrastructureAssembly)
            .That()
            .HaveNameEndingWith("DbContext")
            .GetTypes()
            .Select(t => t.FullName!)
            .ToArray();

        // Testar camada Domain
        var domainResult = Types.InAssembly(UsersDomainAssembly)
            .Should()
            .NotHaveDependencyOnAll(dbContextTypeNames)
            .GetResult();

        domainResult.IsSuccessful.Should().BeTrue(
            "Camada Domain não deve referenciar DbContext. Violações encontradas: {0}",
            string.Join(", ", domainResult.FailingTypes?.Select(t => t.Name) ?? Array.Empty<string>()));

        // Testar camada Application
        var applicationResult = Types.InAssembly(UsersApplicationAssembly)
            .Should()
            .NotHaveDependencyOnAll(dbContextTypeNames)
            .GetResult();

        applicationResult.IsSuccessful.Should().BeTrue(
            "Camada Application não deve referenciar DbContext. Violações encontradas: {0}",
            string.Join(", ", applicationResult.FailingTypes?.Select(t => t.Name) ?? Array.Empty<string>()));

        // Testar camada API
        var apiResult = Types.InAssembly(UsersApiAssembly)
            .Should()
            .NotHaveDependencyOnAll(dbContextTypeNames)
            .GetResult();

        apiResult.IsSuccessful.Should().BeTrue(
            "Camada API não deve referenciar DbContext. Violações encontradas: {0}",
            string.Join(", ", apiResult.FailingTypes?.Select(t => t.Name) ?? Array.Empty<string>()));
    }

    [Fact]
    public void Module_Extensions_ShouldBePublic()
    {
        // Classes de extensão para registro de DI devem ser públicas
        var result = Types.InAssembly(UsersInfrastructureAssembly)
            .That()
            .HaveNameEndingWith("Extensions")
            .Should()
            .BePublic()
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            "Classes de extensão devem ser públicas para registro de DI. " +
            "Violações: {0}", 
            string.Join(", ", result.FailingTypes?.Select(t => t.FullName) ?? []));
    }

    [Fact]
    public void Integration_Events_ShouldBeInSharedProject()
    {
        // Eventos de integração devem estar no projeto Shared para comunicação entre módulos
        var usersAssemblies = new[]
        {
            UsersApiAssembly,
            UsersApplicationAssembly,
            UsersInfrastructureAssembly,
            UsersDomainAssembly
        };

        foreach (var assembly in usersAssemblies)
        {
            var integrationEventTypes = Types.InAssembly(assembly)
                .That()
                .HaveNameEndingWith("IntegrationEvent")
                .GetTypes();

            integrationEventTypes.Should().BeEmpty(
                "Eventos de integração não devem existir em assemblies de módulo, devem estar no Shared. " +
                "Encontrados em {0}: {1}", 
                assembly.GetName().Name,
                string.Join(", ", integrationEventTypes.Select(t => t.FullName)));
        }
    }
}