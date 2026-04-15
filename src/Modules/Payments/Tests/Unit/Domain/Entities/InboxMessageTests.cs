using MeAjudaAi.Modules.Payments.Domain.Entities;

namespace MeAjudaAi.Modules.Payments.Tests.Unit.Domain.Entities;

public class InboxMessageTests
{
    [Fact]
    public void Defaults_ShouldBeCorrect()
    {
        // Act
        var message = InboxMessage.CreateEmpty();

        // Assert
        message.Id.Should().NotBeEmpty();
        message.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
        message.ProcessedAt.Should().BeNull();
        message.Error.Should().BeNull();
        message.RetryCount.Should().Be(0);
        message.MaxRetries.Should().Be(5);
        message.NextAttemptAt.Should().BeNull();
    }

    [Fact]
    public void ShouldRetry_ShouldReturnTrue_WhenNotProcessedAndUnderMaxRetries()
    {
        // Arrange
        var message = new InboxMessage("test", "{}")
        {
            RetryCount = 0,
            MaxRetries = 5
        };

        // Assert
        message.ShouldRetry.Should().BeTrue();
    }

    [Fact]
    public void ShouldRetry_ShouldReturnFalse_WhenProcessed()
    {
        // Arrange
        var message = new InboxMessage("test", "{}")
        {
            ProcessedAt = DateTime.UtcNow
        };

        // Assert
        message.ShouldRetry.Should().BeFalse();
    }

    [Fact]
    public void ShouldRetry_ShouldReturnFalse_WhenMaxRetriesReached()
    {
        // Arrange
        var message = new InboxMessage("test", "{}")
        {
            RetryCount = 5,
            MaxRetries = 5
        };

        // Assert
        message.ShouldRetry.Should().BeFalse();
    }

    [Fact]
    public void ShouldRetry_ShouldReturnFalse_WhenNextAttemptInFuture()
    {
        // Arrange
        var message = new InboxMessage("test", "{}")
        {
            RetryCount = 1,
            MaxRetries = 5,
            NextAttemptAt = DateTime.UtcNow.AddMinutes(10)
        };

        // Assert
        message.ShouldRetry.Should().BeFalse();
    }

    [Fact]
    public void ShouldRetry_ShouldReturnTrue_WhenNextAttemptInPast()
    {
        // Arrange
        var message = new InboxMessage("test", "{}")
        {
            RetryCount = 1,
            MaxRetries = 5,
            NextAttemptAt = DateTime.UtcNow.AddMinutes(-1)
        };

        // Assert
        message.ShouldRetry.Should().BeTrue();
    }

    [Fact]
    public void ShouldRetry_ShouldReturnTrue_WhenRetryCountLessThanMax_AndNoNextAttempt()
    {
        // Arrange
        var message = new InboxMessage("test", "{}")
        {
            RetryCount = 4,
            MaxRetries = 5,
            NextAttemptAt = null
        };

        // Assert
        message.ShouldRetry.Should().BeTrue();
    }

    [Fact]
    public void ShouldRetry_EdgeCase_WhenRetryCountEqualsMaxRetries()
    {
        // Arrange
        var message = new InboxMessage("test", "{}")
        {
            RetryCount = 3,
            MaxRetries = 3,
            NextAttemptAt = null
        };

        // Assert
        message.ShouldRetry.Should().BeFalse();
    }

    [Fact]
    public void Properties_ShouldBeSettable()
    {
        // Arrange
        var id = Guid.NewGuid();
        var now = DateTime.UtcNow;

        // Act
        var message = new InboxMessage("checkout.session.completed", "{\"event\":\"test\"}")
        {
            Id = id,
            CreatedAt = now,
            ProcessedAt = now.AddMinutes(1),
            Error = "Some error",
            RetryCount = 2,
            MaxRetries = 10,
            NextAttemptAt = now.AddMinutes(5)
        };

        // Assert
        message.Id.Should().Be(id);
        message.Type.Should().Be("checkout.session.completed");
        message.Content.Should().Be("{\"event\":\"test\"}");
        message.CreatedAt.Should().Be(now);
        message.ProcessedAt.Should().Be(now.AddMinutes(1));
        message.Error.Should().Be("Some error");
        message.RetryCount.Should().Be(2);
        message.MaxRetries.Should().Be(10);
        message.NextAttemptAt.Should().Be(now.AddMinutes(5));
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenTypeIsNull()
    {
        // Act
        var act = () => new InboxMessage(null!, "{}");

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenTypeIsEmpty()
    {
        // Act
        var act = () => new InboxMessage("  ", "{}");

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenContentIsNull()
    {
        // Act
        var act = () => new InboxMessage("test", null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenContentIsEmpty()
    {
        // Act
        var act = () => new InboxMessage("test", "   ");

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_ShouldSetProperties_WhenValidInput()
    {
        // Act
        var message = new InboxMessage("checkout.session.completed", "{\"id\":\"evt_123\"}");

        // Assert
        message.Type.Should().Be("checkout.session.completed");
        message.Content.Should().Be("{\"id\":\"evt_123\"}");
        message.ProcessedAt.Should().BeNull();
        message.RetryCount.Should().Be(0);
        message.MaxRetries.Should().Be(5);
    }
}