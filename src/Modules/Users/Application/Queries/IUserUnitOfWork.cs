using MeAjudaAi.Shared.Database.Abstractions;

namespace MeAjudaAi.Modules.Users.Application.Queries;

/// <summary>
/// Interface específica para a Unidade de Trabalho do módulo de Users.
/// Resolve conflitos de injeção de IUnitOfWork em ambientes com múltiplos módulos.
/// </summary>
public interface IUserUnitOfWork : IUnitOfWork
{
}



