using MeAjudaAi.Shared.Database.Abstractions;
using MeAjudaAi.Shared.Database;
using MeAjudaAi.Shared.Database.Abstractions;

namespace MeAjudaAi.Modules.Providers.Application.Queries;

/// <summary>
/// Interface específica para a Unidade de Trabalho do módulo de Providers.
/// Resolve conflitos de injeção de IUnitOfWork em ambientes com múltiplos módulos.
/// </summary>
public interface IProviderUnitOfWork : IUnitOfWork
{
}



