using MeAjudaAi.Shared.Messaging;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Shared.Tests.TestInfrastructure.Base;

/// <summary>
/// Classe base para testes de Event Handlers com mocks comuns e configuração.
/// Fornece configuração consistente do AutoFixture para testes determinísticos.
/// </summary>
public abstract class EventHandlerTestBase<THandler>
    where THandler : class
{
    protected Mock<IMessageBus> MessageBusMock { get; }
    protected Mock<ILogger<THandler>> LoggerMock { get; }
    protected Fixture Fixture { get; }

    /// <summary>
    /// Data/hora base para testes determinísticos
    /// </summary>
    protected DateTime BaseDateTime { get; }

    protected EventHandlerTestBase()
    {
        MessageBusMock = new Mock<IMessageBus>();
        LoggerMock = new Mock<ILogger<THandler>>();

        // Define uma data base fixa para testes determinísticos
        BaseDateTime = new DateTime(2025, 9, 23, 10, 0, 0, DateTimeKind.Utc);

        Fixture = new Fixture();

        // Configura AutoFixture para funcionar bem com nosso domínio
        ConfigureFixture();
    }

    /// <summary>
    /// Sobrescreva para personalizar AutoFixture para cenários de teste específicos
    /// </summary>
    protected virtual void ConfigureFixture()
    {
        // Previne AutoFixture de criar referências circulares
        Fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => Fixture.Behaviors.Remove(b));
        Fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        // Configura para criar Guids realistas
        Fixture.Customize<Guid>(composer => composer.FromFactory(() => Guid.NewGuid()));

#pragma warning disable CA5394 // Random is acceptable for test data generation
        // Usa Random com seed fixo para testes determinísticos
        var seededRandom = new Random(42);

        // Configura DateTime para ser determinístico baseado na data base
        Fixture.Customize<DateTime>(composer =>
            composer.FromFactory(() => BaseDateTime.AddDays(seededRandom.Next(0, 30))));
#pragma warning restore CA5394
    }

    /// <summary>
    /// Verifica se uma mensagem foi publicada no message bus
    /// </summary>
    protected void VerifyMessagePublished<TMessage>(Times? times = null)
        where TMessage : class
    {
        MessageBusMock.Verify(
            x => x.PublishAsync(
                It.IsAny<TMessage>(),
                It.IsAny<string?>(),
                It.IsAny<CancellationToken>()),
            times ?? Times.Once());
    }

    /// <summary>
    /// Verifica se uma mensagem específica foi publicada no message bus
    /// </summary>
    protected void VerifyMessagePublished<TMessage>(TMessage expectedMessage, Times? times = null)
        where TMessage : class
    {
        MessageBusMock.Verify(
            x => x.PublishAsync(
                It.Is<TMessage>(msg => msg.Equals(expectedMessage)),
                It.IsAny<string?>(),
                It.IsAny<CancellationToken>()),
            times ?? Times.Once());
    }

    /// <summary>
    /// Verifica se nenhuma mensagem foi publicada no message bus
    /// </summary>
    protected void VerifyNoMessagesPublished()
    {
        MessageBusMock.Verify(
            x => x.PublishAsync(
                It.IsAny<object>(),
                It.IsAny<string?>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    /// <summary>
    /// Verifica se um erro foi logado
    /// </summary>
    protected void VerifyErrorLogged()
    {
        LoggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    /// <summary>
    /// Verifica se informações foram logadas
    /// </summary>
    protected void VerifyInformationLogged()
    {
        LoggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }
}
