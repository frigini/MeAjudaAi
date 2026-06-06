using FluentAssertions;
using MeAjudaAi.Shared.Authorization.Exceptions;
using Xunit;

namespace MeAjudaAi.Shared.Tests.Unit.Authorization;

public class PermissionServiceExceptionTests
{
    [Fact]
    public void Constructor_Default_SetsMessage()
    {
        var exception = new PermissionServiceException();
        exception.Message.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void Constructor_WithMessage_SetsMessage()
    {
        var message = "Custom error message";
        var exception = new PermissionServiceException(message);
        exception.Message.Should().Be(message);
    }

    [Fact]
    public void Constructor_WithMessageAndInnerException_SetsMessageAndInnerException()
    {
        var message = "Custom error message";
        var innerException = new Exception("Inner exception");
        var exception = new PermissionServiceException(message, innerException);
        exception.Message.Should().Be(message);
        exception.InnerException.Should().Be(innerException);
    }
}
