using MeAjudaAi.Modules.Documents.Application;
using MeAjudaAi.Modules.Documents.Application.Handlers.Commands;
using MeAjudaAi.Modules.Documents.Application.Queries.Interfaces;
using MeAjudaAi.Modules.Documents.Domain.Entities;
using MeAjudaAi.Modules.Documents.Infrastructure.Persistence;
using MeAjudaAi.Modules.Documents.Infrastructure.Queries;
using MeAjudaAi.Shared.Database.Abstractions;
using MeAjudaAi.Shared.Database.Constants;
using MeAjudaAi.Shared.Tests.TestInfrastructure.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace MeAjudaAi.Modules.Documents.Tests.Integration.Infrastructure;

public static class DocumentsTestInfrastructureExtensions
{
    public static IServiceCollection AddDocumentsTestInfrastructure(
        this IServiceCollection services,
        TestInfrastructureOptions options)
    {
        services.AddDbContext<DocumentsDbContext>(dbOptions =>
        {
            dbOptions.UseInMemoryDatabase(options.Database.DatabaseName);
        });

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<DocumentsDbContext>());
        services.AddKeyedScoped<IUnitOfWork>(ModuleKeys.Documents,
            (sp, key) => sp.GetRequiredService<DocumentsDbContext>());

        services.AddScoped<IRepository<Document, Guid>>(sp => sp.GetRequiredService<DocumentsDbContext>());
        services.AddScoped<IDocumentQueries, DbContextDocumentQueries>();

        services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();

        var configuration = new ConfigurationBuilder().Build();
        services.AddApplication(configuration);

        services.AddLocalization();
        services.AddLogging();

        return services;
    }
}
