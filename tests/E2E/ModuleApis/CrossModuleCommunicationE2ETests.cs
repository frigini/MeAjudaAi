using FluentAssertions;
using MeAjudaAi.Modules.Users.Tests.Base;
using MeAjudaAi.Shared.Contracts.Modules.Users;
using MeAjudaAi.Shared.Contracts.Modules.Users.DTOs;
using MeAjudaAi.Shared.Time;
using Microsoft.Extensions.DependencyInjection;

namespace MeAjudaAi.Tests.E2E.ModuleApis;

/// <summary>
/// Testes E2E focados nos padrões de comunicação entre módulos
/// Demonstra como diferentes módulos podem interagir via Module APIs
/// </summary>
public class CrossModuleCommunicationE2ETests : IntegrationTestBase
{
    private readonly IUsersModuleApi _usersModuleApi;

    public CrossModuleCommunicationE2ETests()
    {
        _usersModuleApi = GetService<IUsersModuleApi>();
    }

    [Theory]
    [InlineData("NotificationModule", "notification@test.com")]
    [InlineData("OrdersModule", "orders@test.com")]
    [InlineData("PaymentModule", "payment@test.com")]
    [InlineData("ReportingModule", "reports@test.com")]
    public async Task ModuleToModuleCommunication_ShouldWorkForDifferentConsumers(string moduleName, string email)
    {
        // Arrange - Simulate different modules consuming Users API
        var user = await CreateUserAsync(
            username: $"user_for_{moduleName.ToLower()}",
            email: email,
            firstName: "Test",
            lastName: moduleName
        );

        // Act & Assert - Each module would have different use patterns
        switch (moduleName)
        {
            case "NotificationModule":
                // Notification module needs user existence and email validation
                var emailExists = await _usersModuleApi.EmailExistsAsync(email);
                emailExists.IsSuccess.Should().BeTrue();
                emailExists.Value.Should().BeTrue();
                break;

            case "OrdersModule":
                // Orders module needs full user details and batch operations
                var orderUser = await _usersModuleApi.GetUserByIdAsync(user.Id.Value);
                orderUser.IsSuccess.Should().BeTrue();
                orderUser.Value.Should().NotBeNull();
                
                var batchResult = await _usersModuleApi.GetUsersBatchAsync(new[] { user.Id.Value });
                batchResult.IsSuccess.Should().BeTrue();
                batchResult.Value.Should().HaveCount(1);
                break;

            case "PaymentModule":
                // Payment module needs user validation for security
                var userExists = await _usersModuleApi.UserExistsAsync(user.Id.Value);
                userExists.IsSuccess.Should().BeTrue();
                userExists.Value.Should().BeTrue();
                break;

            case "ReportingModule":
                // Reporting module needs batch user data
                var reportingUsers = await _usersModuleApi.GetUsersBatchAsync(new[] { user.Id.Value });
                reportingUsers.IsSuccess.Should().BeTrue();
                reportingUsers.Value.Should().HaveCount(1);
                reportingUsers.Value.First().FullName.Should().Be($"Test {moduleName}");
                break;
        }
    }

    [Fact]
    public async Task SimultaneousModuleRequests_ShouldHandleConcurrency()
    {
        // Arrange - Create test users
        var users = new List<Domain.Entities.User>();
        for (int i = 0; i < 10; i++)
        {
            var user = await CreateUserAsync(
                $"concurrent_user_{i}",
                $"concurrent_{i}@test.com",
                "Concurrent",
                $"User{i}"
            );
            users.Add(user);
        }

        // Act - Simulate multiple modules making concurrent requests
        var notificationTasks = users.Take(3).Select(u => 
            _usersModuleApi.EmailExistsAsync(u.Email)).ToList();
        
        var orderTasks = users.Skip(3).Take(3).Select(u => 
            _usersModuleApi.GetUserByIdAsync(u.Id.Value)).ToList();
        
        var paymentTasks = users.Skip(6).Take(4).Select(u => 
            _usersModuleApi.UserExistsAsync(u.Id.Value)).ToList();

        // Wait for all concurrent operations
        await Task.WhenAll(
            Task.WhenAll(notificationTasks),
            Task.WhenAll(orderTasks),
            Task.WhenAll(paymentTasks)
        );

        // Assert - All operations should succeed
        foreach (var task in notificationTasks)
        {
            var result = await task;
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().BeTrue();
        }

        foreach (var task in orderTasks)
        {
            var result = await task;
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
        }

        foreach (var task in paymentTasks)
        {
            var result = await task;
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().BeTrue();
        }
    }

    [Fact]
    public async Task ModuleApiContract_ShouldMaintainConsistentBehavior()
    {
        // Arrange
        var user = await CreateUserAsync("contract_test", "contract@test.com", "Contract", "Test");
        var nonExistentId = UuidGenerator.NewId();
        var nonExistentEmail = $"nonexistent_{UuidGenerator.NewIdStringCompact()}@test.com";

        // Act & Assert - Test all contract methods behave consistently
        
        // 1. GetUserByIdAsync
        var existingUserResult = await _usersModuleApi.GetUserByIdAsync(user.Id.Value);
        existingUserResult.IsSuccess.Should().BeTrue();
        existingUserResult.Value.Should().NotBeNull();
        
        var nonExistentUserResult = await _usersModuleApi.GetUserByIdAsync(nonExistentId);
        nonExistentUserResult.IsSuccess.Should().BeTrue();
        nonExistentUserResult.Value.Should().BeNull();

        // 2. UserExistsAsync
        var userExistsTrue = await _usersModuleApi.UserExistsAsync(user.Id.Value);
        userExistsTrue.IsSuccess.Should().BeTrue();
        userExistsTrue.Value.Should().BeTrue();
        
        var userExistsFalse = await _usersModuleApi.UserExistsAsync(nonExistentId);
        userExistsFalse.IsSuccess.Should().BeTrue();
        userExistsFalse.Value.Should().BeFalse();

        // 3. EmailExistsAsync
        var emailExistsTrue = await _usersModuleApi.EmailExistsAsync(user.Email);
        emailExistsTrue.IsSuccess.Should().BeTrue();
        emailExistsTrue.Value.Should().BeTrue();
        
        var emailExistsFalse = await _usersModuleApi.EmailExistsAsync(nonExistentEmail);
        emailExistsFalse.IsSuccess.Should().BeTrue();
        emailExistsFalse.Value.Should().BeFalse();

        // 4. GetUsersBatchAsync
        var batchWithExisting = await _usersModuleApi.GetUsersBatchAsync(new[] { user.Id.Value, nonExistentId });
        batchWithExisting.IsSuccess.Should().BeTrue();
        batchWithExisting.Value.Should().HaveCount(1); // Only existing user returned
        batchWithExisting.Value.First().Id.Should().Be(user.Id.Value);
    }

    [Fact]
    public async Task DataConsistency_AcrossModuleApiCalls_ShouldBeConsistent()
    {
        // Arrange
        var user = await CreateUserAsync("consistency", "consistency@test.com", "Data", "Consistency");

        // Act - Get user data through different API methods
        var userById = await _usersModuleApi.GetUserByIdAsync(user.Id.Value);
        var userInBatch = await _usersModuleApi.GetUsersBatchAsync(new[] { user.Id.Value });
        var emailExists = await _usersModuleApi.EmailExistsAsync(user.Email);
        var userExists = await _usersModuleApi.UserExistsAsync(user.Id.Value);

        // Assert - All methods should return consistent data
        userById.IsSuccess.Should().BeTrue();
        userInBatch.IsSuccess.Should().BeTrue();
        emailExists.IsSuccess.Should().BeTrue();
        userExists.IsSuccess.Should().BeTrue();

        // Data consistency checks
        var userDto = userById.Value!;
        var batchUserDto = userInBatch.Value.First();

        userDto.Id.Should().Be(user.Id.Value);
        userDto.Username.Should().Be(user.Username);
        userDto.Email.Should().Be(user.Email);
        userDto.FullName.Should().Be("Data Consistency");

        batchUserDto.Id.Should().Be(userDto.Id);
        batchUserDto.Username.Should().Be(userDto.Username);
        batchUserDto.Email.Should().Be(userDto.Email);
        batchUserDto.FullName.Should().Be(userDto.FullName);

        emailExists.Value.Should().BeTrue();
        userExists.Value.Should().BeTrue();
    }

    [Fact]
    public async Task PerformanceComparison_SingleVsBatchOperations_ShouldFavorBatch()
    {
        // Arrange - Create multiple users
        var userIds = new List<Guid>();
        for (int i = 0; i < 20; i++)
        {
            var user = await CreateUserAsync(
                $"perf_user_{i}",
                $"perf_{i}@test.com",
                "Performance",
                $"User{i}"
            );
            userIds.Add(user.Id.Value);
        }

        // Act - Compare single calls vs batch operation
        var singleCallsStopwatch = System.Diagnostics.Stopwatch.StartNew();
        var singleResults = new List<UserDto?>();
        foreach (var userId in userIds)
        {
            var result = await _usersModuleApi.GetUserByIdAsync(userId);
            singleResults.Add(result.Value);
        }
        singleCallsStopwatch.Stop();

        var batchCallStopwatch = System.Diagnostics.Stopwatch.StartNew();
        var batchResult = await _usersModuleApi.GetUsersBatchAsync(userIds);
        batchCallStopwatch.Stop();

        // Assert - Batch should be faster and return same data
        batchResult.IsSuccess.Should().BeTrue();
        batchResult.Value.Should().HaveCount(20);
        
        singleResults.Should().HaveCount(20);
        singleResults.Should().AllSatisfy(user => user.Should().NotBeNull());

        // Batch operation should be significantly faster
        batchCallStopwatch.ElapsedMilliseconds.Should().BeLessThan(
            singleCallsStopwatch.ElapsedMilliseconds,
            "Batch operation should be faster than multiple single calls"
        );

        // Data should be equivalent
        var batchUserIds = batchResult.Value.Select(u => u.Id).OrderBy(id => id).ToList();
        var singleUserIds = singleResults.Select(u => u!.Id).OrderBy(id => id).ToList();
        
        batchUserIds.Should().BeEquivalentTo(singleUserIds);
    }

    [Fact]
    public async Task ErrorRecovery_ModuleApiFailures_ShouldNotAffectOtherModules()
    {
        // This test simulates how failures in one module's usage shouldn't affect others
        
        // Arrange
        var validUser = await CreateUserAsync("recovery_test", "recovery@test.com", "Recovery", "Test");
        var invalidUserId = UuidGenerator.NewId();

        // Act - Mix valid and invalid operations (simulating different modules)
        var validOperations = new[]
        {
            _usersModuleApi.GetUserByIdAsync(validUser.Id.Value),
            _usersModuleApi.UserExistsAsync(validUser.Id.Value),
            _usersModuleApi.EmailExistsAsync(validUser.Email)
        };

        var invalidOperations = new[]
        {
            _usersModuleApi.GetUserByIdAsync(invalidUserId),
            _usersModuleApi.UserExistsAsync(invalidUserId),
            _usersModuleApi.EmailExistsAsync("invalid@nowhere.com")
        };

        var allResults = await Task.WhenAll(
            validOperations.Concat(invalidOperations)
        );

        // Assert - Valid operations succeed, invalid ones fail gracefully
        var validResults = allResults.Take(3).ToArray();
        var invalidResults = allResults.Skip(3).ToArray();

        // Valid operations should all succeed
        validResults[0].IsSuccess.Should().BeTrue(); // GetUserByIdAsync
        validResults[0].Value.Should().NotBeNull();
        
        validResults[1].IsSuccess.Should().BeTrue(); // UserExistsAsync
        validResults[1].Value.Should().Be(true);
        
        validResults[2].IsSuccess.Should().BeTrue(); // EmailExistsAsync
        validResults[2].Value.Should().Be(true);

        // Invalid operations should fail gracefully (not throw exceptions)
        invalidResults[0].IsSuccess.Should().BeTrue(); // GetUserByIdAsync returns null
        invalidResults[0].Value.Should().BeNull();
        
        invalidResults[1].IsSuccess.Should().BeTrue(); // UserExistsAsync returns false
        invalidResults[1].Value.Should().Be(false);
        
        invalidResults[2].IsSuccess.Should().BeTrue(); // EmailExistsAsync returns false
        invalidResults[2].Value.Should().Be(false);
    }
}