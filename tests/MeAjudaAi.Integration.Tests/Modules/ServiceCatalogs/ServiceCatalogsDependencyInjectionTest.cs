using FluentAssertions;
using MeAjudaAi.Integration.Tests.Base;
using MeAjudaAi.Modules.ServiceCatalogs.Application.Commands.Service;
using MeAjudaAi.Modules.ServiceCatalogs.Application.Commands.ServiceCategory;
using MeAjudaAi.Modules.ServiceCatalogs.Application.DTOs;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Functional;
using Microsoft.Extensions.DependencyInjection;

namespace MeAjudaAi.Integration.Tests.Modules.ServiceCatalogs;

/// <summary>
/// 🧪 TESTE DIAGNÓSTICO PARA SERVICE CATALOGS MODULE DEPENDENCY INJECTION
/// 
/// Verifica se todos os command handlers do módulo ServiceCatalogs estão registrados
/// </summary>
public class ServiceCatalogsDependencyInjectionTest(ITestOutputHelper testOutput) : ApiTestBase
{
    [Fact]
    public void Should_Have_CommandDispatcher_Registered()
    {
        // Arrange & Act
        var commandDispatcher = Services.GetService<ICommandDispatcher>();

        // Assert
        testOutput.WriteLine($"CommandDispatcher registration: {commandDispatcher != null}");
        commandDispatcher.Should().NotBeNull("ICommandDispatcher should be registered");
    }

    [Fact]
    public void Should_Have_CreateServiceCategoryCommandHandler_Registered()
    {
        // Arrange & Act
        // Try to resolve handler
        var handler = Services.GetService<ICommandHandler<CreateServiceCategoryCommand, Result<ServiceCategoryDto>>>();

        // Assert
        testOutput.WriteLine($"CreateServiceCategoryCommandHandler registration: {handler != null}");
        testOutput.WriteLine($"Handler type: {handler?.GetType().FullName}");
        handler.Should().NotBeNull("CreateServiceCategoryCommandHandler should be registered");
    }

    [Fact]
    public void Should_Have_ServiceCategoryRepository_Registered()
    {
        // Arrange & Act
        var repository = Services.GetService<MeAjudaAi.Modules.ServiceCatalogs.Domain.Repositories.IServiceCategoryRepository>();

        // Assert
        testOutput.WriteLine($"IServiceCategoryRepository registration: {repository != null}");
        testOutput.WriteLine($"Repository type: {repository?.GetType().FullName}");
        repository.Should().NotBeNull("IServiceCategoryRepository should be registered");
    }

    [Fact]
    public void Should_Have_ServiceCatalogsDbContext_Registered()
    {
        // Arrange & Act
        var dbContext = Services.GetService<MeAjudaAi.Modules.ServiceCatalogs.Infrastructure.Persistence.ServiceCatalogsDbContext>();

        // Assert
        testOutput.WriteLine($"ServiceCatalogsDbContext registration: {dbContext != null}");
        dbContext.Should().NotBeNull("ServiceCatalogsDbContext should be registered");
    }

    [Fact]
    public void Should_List_All_Registered_CommandHandlers()
    {
        // Arrange - Scan ServiceCatalogs assembly for command handler types
        var catalogsAssembly = typeof(CreateServiceCategoryCommand).Assembly;
        var commandHandlerType = typeof(ICommandHandler<,>);

        // Act - Find all types that implement ICommandHandler<,>
        var handlerTypes = catalogsAssembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract)
            .Where(t => t.GetInterfaces().Any(i =>
                i.IsGenericType &&
                i.GetGenericTypeDefinition() == commandHandlerType))
            .ToList();

        testOutput.WriteLine($"Found {handlerTypes.Count} command handler types in ServiceCatalogs assembly:");

        // Assert - Verify each handler can be resolved from DI
        handlerTypes.Should().NotBeEmpty("ServiceCatalogs assembly should contain command handlers");

        foreach (var handlerType in handlerTypes)
        {
            // Get the ICommandHandler<TCommand, TResult> interface this type implements
            var handlerInterface = handlerType.GetInterfaces()
                .First(i => i.IsGenericType && i.GetGenericTypeDefinition() == commandHandlerType);

            testOutput.WriteLine($"- {handlerType.Name} implements {handlerInterface.Name}");

            // Verify the handler can be resolved from DI
            var resolvedHandler = Services.GetService(handlerInterface);
            resolvedHandler.Should().NotBeNull($"{handlerType.Name} should be registered in DI container");
        }

        testOutput.WriteLine($"\n✅ All {handlerTypes.Count} command handlers are properly registered");
    }

    [Fact]
    public async Task Should_Be_Able_To_Resolve_And_Execute_CreateServiceCategoryCommandHandler()
    {
        // Arrange
        var commandDispatcher = Services.GetRequiredService<ICommandDispatcher>();
        var command = new CreateServiceCategoryCommand(
            Name: $"Test Category {Guid.NewGuid():N}",
            Description: "Test Description",
            DisplayOrder: 1
        );

        // Act
        Result<ServiceCategoryDto>? result = null;
        Exception? exception = null;

        try
        {
            result = await commandDispatcher.SendAsync<CreateServiceCategoryCommand, Result<ServiceCategoryDto>>(command);
        }
        catch (Exception ex)
        {
            exception = ex;
            testOutput.WriteLine($"Exception: {ex.GetType().Name}");
            testOutput.WriteLine($"Message: {ex.Message}");
            testOutput.WriteLine($"StackTrace: {ex.StackTrace}");

            if (ex.InnerException != null)
            {
                testOutput.WriteLine($"InnerException: {ex.InnerException.GetType().Name}");
                testOutput.WriteLine($"InnerMessage: {ex.InnerException.Message}");
                testOutput.WriteLine($"InnerStackTrace: {ex.InnerException.StackTrace}");
            }
        }

        // Assert
        testOutput.WriteLine($"Result IsSuccess: {result?.IsSuccess}");
        testOutput.WriteLine($"Result Value: {result?.Value}");
        testOutput.WriteLine($"Result Error: {result?.Error}");

        exception.Should().BeNull("Command execution should not throw exception");
        result.Should().NotBeNull("Command should return a result");
        result.IsSuccess.Should().BeTrue("Command should succeed");
        result.Value.Should().NotBeNull("Result should contain created DTO");
        result.Value.Id.Should().NotBe(Guid.Empty, "Created entity should have a valid ID");
        result.Value.Name.Should().Be(command.Name, "Created entity name should match command");
    }
}
