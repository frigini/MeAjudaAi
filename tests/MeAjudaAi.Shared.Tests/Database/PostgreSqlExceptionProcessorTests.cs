using FluentAssertions;
using MeAjudaAi.Shared.Database.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace MeAjudaAi.Shared.Tests.Database;

public class PostgreSqlExceptionProcessorTests
{
    [Fact]
    public void ProcessException_WithNullException_ShouldThrowArgumentNullException()
    {
        // Act
        var act = () => PostgreSqlExceptionProcessor.ProcessException(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ProcessException_WithNonPostgresException_ShouldReturnOriginalException()
    {
        // Arrange
        var innerException = new InvalidOperationException("Some error");
        var dbUpdateException = new DbUpdateException("Error", innerException);

        // Act
        var result = PostgreSqlExceptionProcessor.ProcessException(dbUpdateException);

        // Assert
        result.Should().Be(dbUpdateException);
    }

    [Fact]
    public void ProcessException_WithNoInnerException_ShouldReturnOriginalException()
    {
        // Arrange
        var dbUpdateException = new DbUpdateException("Error without inner exception");

        // Act
        var result = PostgreSqlExceptionProcessor.ProcessException(dbUpdateException);

        // Assert
        result.Should().Be(dbUpdateException);
    }

    [Fact]
    public void ProcessException_WithDbUpdateExceptionOnly_ShouldReturnOriginal()
    {
        // Arrange
        var exception = new DbUpdateException("Generic error");

        // Act
        var result = PostgreSqlExceptionProcessor.ProcessException(exception);

        // Assert
        result.Should().Be(exception);
        result.Should().BeOfType<DbUpdateException>();
    }

    [Fact]
    public void ProcessException_WithDifferentInnerException_ShouldNotConvert()
    {
        // Arrange  
        var innerException = new ArgumentException("Not a Postgres exception");
        var dbUpdateException = new DbUpdateException("Error", innerException);

        // Act
        var result = PostgreSqlExceptionProcessor.ProcessException(dbUpdateException);

        // Assert
        result.Should().Be(dbUpdateException);
        result.Should().NotBeOfType<UniqueConstraintException>();
        result.Should().NotBeOfType<NotNullConstraintException>();
        result.Should().NotBeOfType<ForeignKeyConstraintException>();
    }

    [Fact]
    public void UniqueConstraintException_ShouldStoreConstraintAndColumnNames()
    {
        // Arrange
        var innerException = new Exception("Inner");
        
        // Act
        var exception = new UniqueConstraintException("unique_email", "email", innerException);

        // Assert
        exception.ConstraintName.Should().Be("unique_email");
        exception.ColumnName.Should().Be("email");
        exception.InnerException.Should().Be(innerException);
    }

    [Fact]
    public void NotNullConstraintException_ShouldStoreColumnName()
    {
        // Arrange
        var innerException = new Exception("Inner");
        
        // Act
        var exception = new NotNullConstraintException("name", innerException, isColumnName: true);

        // Assert
        exception.ColumnName.Should().Be("name");
        exception.InnerException.Should().Be(innerException);
    }

    [Fact]
    public void ForeignKeyConstraintException_ShouldStoreConstraintAndTableNames()
    {
        // Arrange
        var innerException = new Exception("Inner");
        
        // Act
        var exception = new ForeignKeyConstraintException("fk_provider", "providers", innerException);

        // Assert
        exception.ConstraintName.Should().Be("fk_provider");
        exception.TableName.Should().Be("providers");
        exception.InnerException.Should().Be(innerException);
    }

    [Fact]
    public void UniqueConstraintException_WithNullValues_ShouldHandleGracefully()
    {
        // Arrange & Act
        var exception = new UniqueConstraintException(null, null, new Exception());

        // Assert
        exception.ConstraintName.Should().BeNull();
        exception.ColumnName.Should().BeNull();
    }

    [Fact]
    public void NotNullConstraintException_WithNullColumnName_ShouldHandleGracefully()
    {
        // Arrange & Act
        var exception = new NotNullConstraintException(null, new Exception(), isColumnName: true);

        // Assert
        exception.ColumnName.Should().BeNull();
    }

    [Fact]
    public void ForeignKeyConstraintException_WithNullValues_ShouldHandleGracefully()
    {
        // Arrange & Act
        var exception = new ForeignKeyConstraintException(null, null, new Exception());

        // Assert
        exception.ConstraintName.Should().BeNull();
        exception.TableName.Should().BeNull();
    }
}
