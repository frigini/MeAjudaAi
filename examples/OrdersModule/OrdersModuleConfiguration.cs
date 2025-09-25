using MeAjudaAi.Examples.OrdersModule.Application.Services;
using MeAjudaAi.Modules.Users.Application; // Para registrar o módulo Users
using MeAjudaAi.Shared.Contracts.Modules.Users;
using MeAjudaAi.Shared.Modules;
using Microsoft.Extensions.DependencyInjection;

namespace MeAjudaAi.Examples.OrdersModule;

/// <summary>
/// Exemplo de configuração de DI para um módulo Orders que consome a API do módulo Users
/// </summary>
public static class OrdersModuleConfiguration
{
    /// <summary>
    /// Registra os serviços do módulo Orders e suas dependências
    /// </summary>
    public static IServiceCollection AddOrdersModule(this IServiceCollection services)
    {
        // 1. Registra o módulo Users (com sua API)
        services.AddApplication(); // Extension method do módulo Users

        // 2. Registra automaticamente todas as Module APIs encontradas
        services.AddModuleApis(typeof(IUsersModuleApi).Assembly);

        // 3. Registra os serviços específicos do módulo Orders
        services.AddScoped<OrderValidationService>();

        // 4. Outros serviços do módulo Orders...
        // services.AddScoped<IOrderRepository, OrderRepository>();
        // services.AddScoped<OrderService>();
        // etc.

        return services;
    }

    /// <summary>
    /// Exemplo de como verificar quais Module APIs estão disponíveis em runtime
    /// </summary>
    public static async Task<string> GetModuleHealthStatus(IServiceProvider serviceProvider)
    {
        var moduleInfos = await ModuleApiRegistry.GetRegisteredModulesAsync(serviceProvider);
        
        var status = "Module APIs Status:\n";
        foreach (var module in moduleInfos)
        {
            status += $"- {module.ModuleName} v{module.ApiVersion}: {(module.IsAvailable ? "✅ Available" : "❌ Unavailable")}\n";
        }

        return status;
    }
}

/// <summary>
/// Exemplo de como um Controller no módulo Orders usaria a API do módulo Users
/// </summary>
/// <remarks>
/// Este seria um controller real em um módulo Orders
/// </remarks>
public class ExampleOrderController
{
    private readonly IUsersModuleApi _usersApi;
    private readonly OrderValidationService _orderValidation;

    public ExampleOrderController(IUsersModuleApi usersApi, OrderValidationService orderValidation)
    {
        _usersApi = usersApi;
        _orderValidation = orderValidation;
    }

    /// <summary>
    /// Exemplo: Criar um pedido validando primeiro se o usuário existe
    /// </summary>
    public async Task<string> CreateOrder(Guid userId, string productName)
    {
        // Valida se o usuário pode criar pedidos
        var validationResult = await _orderValidation.ValidateUserCanCreateOrderAsync(userId);
        
        if (validationResult.IsFailure)
        {
            return $"Cannot create order: {validationResult.Error}";
        }

        // Obtém informações do usuário para o pedido
        var userResult = await _usersApi.GetUserByIdAsync(userId);
        
        if (userResult.IsFailure || userResult.Value == null)
        {
            return "Cannot create order: User not found";
        }

        var user = userResult.Value;
        
        // Simula criação do pedido
        return $"Order created successfully for {user.FullName} ({user.Email}) - Product: {productName}";
    }
}