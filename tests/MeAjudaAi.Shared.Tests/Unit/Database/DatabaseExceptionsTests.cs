using FluentAssertions;
using MeAjudaAi.Shared.Database.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace MeAjudaAi.Shared.Tests.Unit.Database;

public class DatabaseExceptionsTests
{
    [Fact]
    public void UniqueConstraintException_DefaultConstructor_ShouldHaveDefaultMessage()
    {
        // Act
        var exception = new UniqueConstraintException();

        // Assert
        exception.Message.Should().Be("Unique constraint violation");
        exception.ConstraintName.Should().BeNull();
        exception.ColumnName.Should().BeNull();
    }

    [Fact]
    public void UniqueConstraintException_WithMessage_ShouldStoreMessage()
    {
        // Arrange
        var message = "Duplicate email address";

        // Act
        var exception = new UniqueConstraintException(message);

        // Assert
        exception.Message.Should().Be(message);
    }

    [Fact]
    public void UniqueConstraintException_WithConstraintAndColumn_ShouldStoreDetails()
    {
        // Arrange
        var constraintName = "uk_users_email";
        var columnName = "email";
        var innerException = new Exception("Inner error");

        // Act
        var exception = new UniqueConstraintException(constraintName, columnName, innerException);

        // Assert
        exception.ConstraintName.Should().Be(constraintName);
        exception.ColumnName.Should().Be(columnName);
        exception.InnerException.Should().Be(innerException);
        exception.Message.Should().Contain(columnName);
    }

    [Fact]
    public void UniqueConstraintException_WithNullColumn_ShouldUseDefaultText()
    {
        // Arrange
        var innerException = new Exception("Inner error");

        // Act
        var exception = new UniqueConstraintException(null, null, innerException);

        // Assert
        exception.Message.Should().Contain("unknown column");
        exception.ConstraintName.Should().BeNull();
        exception.ColumnName.Should().BeNull();
    }

    [Fact]
    public void UniqueConstraintException_ShouldInheritFromDbUpdateException()
    {
        // Arrange & Act
        var exception = new UniqueConstraintException();

        // Assert
        exception.Should().BeAssignableTo<DbUpdateException>();
    }

    [Fact]
    public void NotNullConstraintException_DefaultConstructor_ShouldHaveDefaultMessage()
    {
        // Act
        var exception = new NotNullConstraintException();

        // Assert
        exception.Message.Should().Be("Cannot insert null value");
        exception.ColumnName.Should().BeNull();
    }

    [Fact]
    public void NotNullConstraintException_WithMessage_ShouldStoreMessage()
    {
        // Arrange
        var message = "Name cannot be null";

        // Act
        var exception = new NotNullConstraintException(message);

        // Assert
        exception.Message.Should().Be(message);
    }

    [Fact]
    public void NotNullConstraintException_WithColumnName_ShouldStoreColumn()
    {
        // Arrange
        var columnName = "name";
        var innerException = new Exception("Inner error");

        // Act
        var exception = new NotNullConstraintException(columnName, innerException, isColumnName: true);

        // Assert
        exception.ColumnName.Should().Be(columnName);
        exception.InnerException.Should().Be(innerException);
        exception.Message.Should().Contain(columnName);
    }

    [Fact]
    public void NotNullConstraintException_WithNullColumn_ShouldUseDefaultText()
    {
        // Arrange
        var innerException = new Exception("Inner error");

        // Act
        var exception = new NotNullConstraintException(null, innerException, isColumnName: true);

        // Assert
        exception.Message.Should().Contain("unknown column");
        exception.ColumnName.Should().BeNull();
    }

    [Fact]
    public void NotNullConstraintException_ShouldInheritFromDbUpdateException()
    {
        // Arrange & Act
        var exception = new NotNullConstraintException();

        // Assert
        exception.Should().BeAssignableTo<DbUpdateException>();
    }

    [Fact]
    public void ForeignKeyConstraintException_DefaultConstructor_ShouldHaveDefaultMessage()
    {
        // Act
        var exception = new ForeignKeyConstraintException();

        // Assert
        exception.Message.Should().Be("Foreign key constraint violation");
        exception.ConstraintName.Should().BeNull();
        exception.TableName.Should().BeNull();
    }

    [Fact]
    public void ForeignKeyConstraintException_WithMessage_ShouldStoreMessage()
    {
        // Arrange
        var message = "Referenced user does not exist";

        // Act
        var exception = new ForeignKeyConstraintException(message);

        // Assert
        exception.Message.Should().Be(message);
    }

    [Fact]
    public void ForeignKeyConstraintException_WithConstraintAndTable_ShouldStoreDetails()
    {
        // Arrange
        var constraintName = "fk_orders_user_id";
        var tableName = "orders";
        var innerException = new Exception("Inner error");

        // Act
        var exception = new ForeignKeyConstraintException(constraintName, tableName, innerException);

        // Assert
        exception.ConstraintName.Should().Be(constraintName);
        exception.TableName.Should().Be(tableName);
        exception.InnerException.Should().Be(innerException);
        exception.Message.Should().Contain(tableName);
    }

    [Fact]
    public void ForeignKeyConstraintException_WithNullTable_ShouldUseDefaultText()
    {
        // Arrange
        var innerException = new Exception("Inner error");

        // Act
        var exception = new ForeignKeyConstraintException(null, null, innerException);

        // Assert
        exception.Message.Should().Contain("unknown table");
        exception.ConstraintName.Should().BeNull();
        exception.TableName.Should().BeNull();
    }

    [Fact]
    public void ForeignKeyConstraintException_ShouldInheritFromDbUpdateException()
    {
        // Arrange & Act
        var exception = new ForeignKeyConstraintException();

        // Assert
        exception.Should().BeAssignableTo<DbUpdateException>();
    }

    [Fact]
    public void ProcessException_WithNullArgument_ShouldThrowArgumentNullException()
    {
        // Arrange
        DbUpdateException? nullException = null;

        // Act
        var act = () => PostgreSqlExceptionProcessor.ProcessException(nullException!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void PostgreSqlExceptionProcessor_ProcessException_WithNonPostgresException_ShouldReturnOriginal()
    {
        // Arrange
        var dbUpdateException = new DbUpdateException("Test error");

        // Act
        var result = PostgreSqlExceptionProcessor.ProcessException(dbUpdateException);

        // Assert
        result.Should().Be(dbUpdateException);
    }

    [Fact]
    public void PostgreSqlExceptionProcessor_ProcessException_WithUniqueViolation23505_ShouldReturnUniqueConstraintException()
    {
        // Arrange
        var postgresException = new Npgsql.PostgresException("23505", "duplicate_key", "Key (email)=(test@test.com) already exists.", "users");
        var dbUpdateException = new DbUpdateException("Update failed", postgresException);

        // Act
        var result = PostgreSqlExceptionProcessor.ProcessException(dbUpdateException);

        // Assert
        result.Should().BeOfType<UniqueConstraintException>();
        var uniqueException = (UniqueConstraintException)result;
        uniqueException.Message.Should().Contain("email");
    }

    [Fact]
    public void PostgreSqlExceptionProcessor_ProcessException_WithNotNullViolation23502_ShouldReturnNotNullConstraintException()
    {
        // Arrange
        var postgresException = new Npgsql.PostgresException("23502", "not_null_violation", "null value in column 'name'", "users");
        var dbUpdateException = new DbUpdateException("Update failed", postgresException);

        // Act
        var result = PostgreSqlExceptionProcessor.ProcessException(dbUpdateException);

        // Assert
        result.Should().BeOfType<NotNullConstraintException>();
    }

    [Fact]
    public void PostgreSqlExceptionProcessor_ProcessException_WithForeignKeyViolation23503_ShouldReturnForeignKeyConstraintException()
    {
        // Arrange
        var postgresException = new Npgsql.PostgresException("23503", "foreign_key_violation", "violates foreign key constraint \"fk_orders_user\" on table \"orders\"", "orders");
        var dbUpdateException = new DbUpdateException("Update failed", postgresException);

        // Act
        var result = PostgreSqlExceptionProcessor.ProcessException(dbUpdateException);

        // Assert
        result.Should().BeOfType<ForeignKeyConstraintException>();
        var fkException = (ForeignKeyConstraintException)result;
        fkException.Message.Should().Contain("orders");
    }

    [Fact]
    public void PostgreSqlExceptionProcessor_ProcessException_WithUnknownState_ShouldReturnOriginal()
    {
        // Arrange
        var postgresException = new Npgsql.PostgresException("99999", "unknown_error", "Some unknown error", "table");
        var dbUpdateException = new DbUpdateException("Update failed", postgresException);

        // Act
        var result = PostgreSqlExceptionProcessor.ProcessException(dbUpdateException);

        // Assert
        result.Should().Be(dbUpdateException);
    }

    [Fact]
    public void PostgreSqlExceptionProcessor_ProcessException_WithEmptyDetail_ShouldHandleGracefully()
    {
        // Arrange
        var postgresException = new Npgsql.PostgresException("23505", "duplicate_key", "", "table");
        var dbUpdateException = new DbUpdateException("Update failed", postgresException);

        // Act
        var result = PostgreSqlExceptionProcessor.ProcessException(dbUpdateException);

        // Assert
        result.Should().BeOfType<UniqueConstraintException>();
    }

    [Fact]
    public void PostgreSqlExceptionProcessor_ProcessException_WithInnerExceptionNull_ShouldHandleGracefully()
    {
        // Arrange
        var dbUpdateException = new DbUpdateException("Update failed");

        // Act
        var result = PostgreSqlExceptionProcessor.ProcessException(dbUpdateException);

        // Assert
        result.Should().Be(dbUpdateException);
    }
}
