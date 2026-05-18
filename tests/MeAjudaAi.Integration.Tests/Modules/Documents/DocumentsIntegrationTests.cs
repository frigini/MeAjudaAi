using FluentAssertions;
using MeAjudaAi.Modules.Documents.Application.Queries;
using MeAjudaAi.Modules.Documents.Infrastructure.Persistence;
using MeAjudaAi.Shared.Database;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace MeAjudaAi.Modules.Documents.Tests.Integration.Modules.Documents;

public class DocumentsIntegrationTests
{
    [Fact]
    public void Test()
    {
        true.Should().BeTrue();
    }
}
