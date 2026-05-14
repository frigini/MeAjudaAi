using MeAjudaAi.Shared.Database;

namespace MeAjudaAi.Modules.Documents.Application.Interfaces;

/// <summary>
/// Interface de marcador para o Unit of Work específico do módulo Documents.
/// Resolve conflitos de injeção em ambientes onde múltiplos módulos compartilham o mesmo container.
/// </summary>
public interface IDocumentsUnitOfWork : IUnitOfWork
{
}
