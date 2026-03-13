using MeAjudaAi.Contracts.Functional;
using FluentAssertions;
using Xunit;

namespace MeAjudaAi.Shared.Tests.Unit.Functional;

[Trait("Category", "Unit")]
public class ResultTests
{
    [Fact]
    public void Success_ShouldCreateSuccessfulResult()
    {
        // Arrange
        var value = "test-value";

        // Act
        var result = Result<string>.Success(value);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.Value.Should().Be(value);
        result.Error.Should().BeNull();
    }

    [Fact]
    public void Failure_WithError_ShouldCreateFailedResult()
    {
        // Arrange
        var error = Error.BadRequest("Test error");

        // Act
        var result = Result<string>.Failure(error);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Value.Should().BeNull();
        result.Error.Should().Be(error);
    }

    [Fact]
    public void Failure_WithMessage_ShouldCreateFailedResultWithBadRequestError()
    {
        // Arrange
        var message = "Test error message";

        // Act
        var result = Result<string>.Failure(message);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Value.Should().BeNull();
        result.Error.Should().NotBeNull();
        result.Error!.Message.Should().Be(message);
    }

    [Fact]
    public void ImplicitConversion_FromValue_ShouldCreateSuccessfulResult()
    {
        // Arrange
        var value = "test-value";

        // Act
        Result<string> result = value;

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(value);
    }

    [Fact]
    public void ImplicitConversion_FromError_ShouldCreateFailedResult()
    {
        // Arrange
        var error = Error.NotFound("Not found");

        // Act
        Result<string> result = error;

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(error);
    }

    [Fact]
    public void ImplicitConversion_FromNullError_ShouldThrowArgumentNullException()
    {
        // Arrange
        Error? error = null;

        // Act
        var act = () => { Result<string> _ = error!; };

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("error");
    }

    [Fact]
    public void Match_WithSuccessfulResult_ShouldExecuteSuccessFunction()
    {
        // Arrange
        var value = "test-value";
        var result = Result<string>.Success(value);
        var successCalled = false;
        var errorCalled = false;

        // Act
        var matchResult = result.Match(
            onSuccess: v => { successCalled = true; return v.ToUpper(); },
            onFailure: e => { errorCalled = true; return "ERROR"; }
        );

        // Assert
        successCalled.Should().BeTrue();
        errorCalled.Should().BeFalse();
        matchResult.Should().Be("TEST-VALUE");
    }

    [Fact]
    public void Match_WithFailedResult_ShouldExecuteFailureFunction()
    {
        // Arrange
        var error = Error.BadRequest("Test error");
        var result = Result<string>.Failure(error);
        var successCalled = false;
        var errorCalled = false;

        // Act
        var matchResult = result.Match(
            onSuccess: v => { successCalled = true; return v.ToUpper(); },
            onFailure: e => { errorCalled = true; return "ERROR"; }
        );

        // Assert
        successCalled.Should().BeFalse();
        errorCalled.Should().BeTrue();
        matchResult.Should().Be("ERROR");
    }

    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateResultCorrectly()
    {
        // Arrange
        var value = "test-value";
        var error = Error.BadRequest("Test error");

        // Act
        var successResult = new Result<string>(true, value, null);
        var failureResult = new Result<string>(false, default, error);

        // Assert
        successResult.IsSuccess.Should().BeTrue();
        successResult.Value.Should().Be(value);
        successResult.Error.Should().BeNull();

        failureResult.IsSuccess.Should().BeFalse();
        failureResult.Value.Should().BeNull();
        failureResult.Error.Should().Be(error);
    }

    [Fact]
    public void Constructor_SuccessWithError_ShouldThrowArgumentException()
    {
        // Arrange
        var error = Error.BadRequest("Error");

        // Act
        var act = () => new Result<string>(true, "Value", error);

        // Assert
        act.Should().Throw<ArgumentException>().WithMessage("*Success result cannot have an error.*");
    }

    [Fact]
    public void Constructor_FailureWithoutError_ShouldThrowArgumentNullException()
    {
        // Act
        var act = () => new Result<string>(false, "Value", null);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("error");
    }
}

[Trait("Category", "Unit")]
public class ResultNonGenericTests
{
    [Fact]
    public void Success_ShouldCreateSuccessfulResult()
    {
        // Act
        var result = Result.Success();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.Error.Should().BeNull();
    }

    [Fact]
    public void Failure_WithError_ShouldCreateFailedResult()
    {
        // Arrange
        var error = Error.BadRequest("Test error");

        // Act
        var result = Result.Failure(error);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(error);
    }

    [Fact]
    public void Failure_WithMessage_ShouldCreateFailedResultWithBadRequestError()
    {
        // Arrange
        var message = "Test error message";

        // Act
        var result = Result.Failure(message);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error!.Message.Should().Be(message);
    }

    [Fact]
    public void ImplicitConversion_FromError_ShouldCreateFailedResult()
    {
        // Arrange
        var error = Error.NotFound("Not found");

        // Act
        Result result = error;

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(error);
    }

    [Fact]
    public void ImplicitConversion_FromNullError_ShouldThrowArgumentNullException()
    {
        // Arrange
        Error? error = null;

        // Act
        var act = () => { Result _ = error!; };

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("error");
    }

    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateResultCorrectly()
    {
        // Arrange
        var error = Error.BadRequest("Test error");

        // Act
        var successResult = new Result(true, null);
        var failureResult = new Result(false, error);

        // Assert
        successResult.IsSuccess.Should().BeTrue();
        successResult.Error.Should().BeNull();

        failureResult.IsSuccess.Should().BeFalse();
        failureResult.Error.Should().Be(error);
    }

    [Fact]
    public void Constructor_SuccessWithError_ShouldThrowArgumentException()
    {
        // Arrange
        var error = Error.BadRequest("Error");

        // Act
        var act = () => new Result(true, error);

        // Assert
        act.Should().Throw<ArgumentException>().WithMessage("*Success result cannot have an error.*");
    }

    [Fact]
    public void Constructor_FailureWithoutError_ShouldThrowArgumentNullException()
    {
        // Act
        var act = () => new Result(false, null);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("error");
    }

    [Fact]
    public void Match_Action_WithSuccessfulResult_ShouldExecuteSuccessAction()
    {
        // Arrange
        var result = Result.Success();
        var successCalled = false;
        var errorCalled = false;

        // Act
        result.Match(
            onSuccess: () => { successCalled = true; },
            onFailure: e => { errorCalled = true; }
        );

        // Assert
        successCalled.Should().BeTrue();
        errorCalled.Should().BeFalse();
    }

    [Fact]
    public void Match_Action_WithFailedResult_ShouldExecuteFailureAction()
    {
        // Arrange
        var error = Error.BadRequest("Error");
        var result = Result.Failure(error);
        var successCalled = false;
        var errorCalled = false;

        // Act
        result.Match(
            onSuccess: () => { successCalled = true; },
            onFailure: e => { errorCalled = true; }
        );

        // Assert
        successCalled.Should().BeFalse();
        errorCalled.Should().BeTrue();
    }

    [Fact]
    public void Match_Func_WithSuccessfulResult_ShouldExecuteSuccessFunc()
    {
        // Arrange
        var result = Result.Success();

        // Act
        var matchResult = result.Match(
            onSuccess: () => "SUCCESS",
            onFailure: e => "FAILURE"
        );

        // Assert
        matchResult.Should().Be("SUCCESS");
    }

    [Fact]
    public void Match_Func_WithFailedResult_ShouldExecuteFailureFunc()
    {
        // Arrange
        var error = Error.BadRequest("Error");
        var result = Result.Failure(error);

        // Act
        var matchResult = result.Match(
            onSuccess: () => "SUCCESS",
            onFailure: e => "FAILURE"
        );

        // Assert
        matchResult.Should().Be("FAILURE");
    }
}

