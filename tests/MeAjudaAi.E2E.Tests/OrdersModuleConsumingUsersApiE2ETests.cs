using FluentAssertions;
using MeAjudaAi.Modules.Users.Tests.Base;
using MeAjudaAi.Shared.Contracts.Modules.Users;
using MeAjudaAi.Shared.Contracts.Modules.Users.DTOs;
using MeAjudaAi.Shared.Time;
using Microsoft.Extensions.DependencyInjection;

namespace MeAjudaAi.Tests.E2E.ModuleApis;

/// <summary>
/// Testes E2E simulando um módulo Orders consumindo a API do módulo Users
/// </summary>
public class OrdersModuleConsumingUsersApiE2ETests : IntegrationTestBase
{
    private readonly IUsersModuleApi _usersModuleApi;

    public OrdersModuleConsumingUsersApiE2ETests()
    {
        _usersModuleApi = GetService<IUsersModuleApi>();
    }

    [Fact]
    public async Task OrderModule_ValidatingExistingUser_ShouldSucceed()
    {
        // Arrange - Create a real user in the database
        var user = await CreateUserAsync(
            username: "orderuser1",
            email: "orderuser1@example.com",
            firstName: "Order",
            lastName: "User"
        );

        // Act - Validate user through Module API (as if from Orders module)
        var userExists = await _usersModuleApi.UserExistsAsync(user.Id.Value);

        // Assert
        userExists.IsSuccess.Should().BeTrue();
        userExists.Value.Should().BeTrue();
    }

    [Fact]
    public async Task OrderModule_ValidatingNonExistentUser_ShouldReturnFalse()
    {
        // Arrange
        var nonExistentUserId = UuidGenerator.NewId();

        // Act
        var userExists = await _usersModuleApi.UserExistsAsync(nonExistentUserId);

        // Assert
        userExists.IsSuccess.Should().BeTrue();
        userExists.Value.Should().BeFalse();
    }

    [Fact]
    public async Task OrderModule_GettingMultipleUsers_ShouldReturnBatchData()
    {
        // Arrange - Create multiple users
        var user1 = await CreateUserAsync("order1", "order1@test.com", "Order", "One");
        var user2 = await CreateUserAsync("order2", "order2@test.com", "Order", "Two");
        var user3 = await CreateUserAsync("order3", "order3@test.com", "Order", "Three");

        var userIds = new List<Guid> { user1.Id.Value, user2.Id.Value, user3.Id.Value };

        // Act - Get user data for orders (batch operation)
        var result = await _usersModuleApi.GetUsersBatchAsync(userIds);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(3);
        
        var userDict = result.Value.ToDictionary(u => u.Id);
        userDict[user1.Id.Value].Username.Should().Be("order1");
        userDict[user2.Id.Value].Username.Should().Be("order2");
        userDict[user3.Id.Value].Username.Should().Be("order3");
    }

    [Fact]
    public async Task OrderModule_WithMixOfExistingAndNonExistentUsers_ShouldReturnOnlyExisting()
    {
        // Arrange
        var existingUser = await CreateUserAsync("mixedorder", "mixedorder@test.com", "Mixed", "Order");
        var nonExistentUserId = UuidGenerator.NewId();

        var userIds = new List<Guid> { existingUser.Id.Value, nonExistentUserId };

        // Act
        var result = await _usersModuleApi.GetUsersBatchAsync(userIds);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
        result.Value.First().Id.Should().Be(existingUser.Id.Value);
    }

    [Fact]
    public async Task NotificationModule_ValidatingEmailExists_ShouldSucceed()
    {
        // Arrange
        var user = await CreateUserAsync("specialorder", "specialorder@vip.com", "Special", "Order");

        // Act
        var result = await _usersModuleApi.EmailExistsAsync("specialorder@vip.com");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();
    }

    [Fact]
    public async Task NotificationModule_ValidatingNonExistentEmail_ShouldReturnFalse()
    {
        // Arrange
        var nonExistentEmail = $"nonexistent_{UuidGenerator.NewIdStringCompact()}@nowhere.com";

        // Act
        var result = await _usersModuleApi.EmailExistsAsync(nonExistentEmail);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeFalse();
    }

    [Fact]
    public async Task CompleteOrderFlow_SimulatingRealModuleUsage_ShouldWorkEndToEnd()
    {
        // Arrange - Create a customer
        var customer = await CreateUserAsync("customer1", "customer1@shop.com", "John", "Customer");

        // Step 1: Orders module validates user exists
        var userExists = await _usersModuleApi.UserExistsAsync(customer.Id.Value);
        userExists.IsSuccess.Should().BeTrue();
        userExists.Value.Should().BeTrue();

        // Step 2: Orders module gets user details
        var userDetailsResult = await _usersModuleApi.GetUserByIdAsync(customer.Id.Value);
        userDetailsResult.IsSuccess.Should().BeTrue();
        var customerDetails = userDetailsResult.Value!;

        // Step 3: Notification module validates email exists
        var emailExists = await _usersModuleApi.EmailExistsAsync(customer.Email);
        emailExists.IsSuccess.Should().BeTrue();
        emailExists.Value.Should().BeTrue();

        // Assert - All API calls work as expected for cross-module communication
        customerDetails.Id.Should().Be(customer.Id.Value);
        customerDetails.FullName.Should().Be("John Customer");
        customerDetails.Email.Should().Be("customer1@shop.com");
    }

    [Fact]
    public async Task PerformanceTest_BatchUserLookup_ShouldHandleManyUsers()
    {
        // Arrange - Create many users
        var users = new List<Domain.Entities.User>();
        var userIds = new List<Guid>();

        for (int i = 0; i < 50; i++)
        {
            var user = await CreateUserAsync(
                $"perfuser{i}",
                $"perfuser{i}@perf.test",
                "Perf",
                $"User{i}"
            );
            users.Add(user);
            userIds.Add(user.Id.Value);
        }

        // Act - Measure performance of batch lookup
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var result = await _usersModuleApi.GetUsersBatchAsync(userIds);
        stopwatch.Stop();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(50);
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(5000, "Batch lookup should be reasonably fast");

        // Verify all users are present
        var userDict = result.Value.ToDictionary(u => u.Id);
        foreach (var user in users)
        {
            userDict.Should().ContainKey(user.Id.Value);
            userDict[user.Id.Value].Username.Should().Be(user.Username);
        }
    }

    [Fact]
    public async Task ConcurrencyTest_MultipleModuleApiCalls_ShouldWorkConcurrently()
    {
        // Arrange - Create test users
        var user1 = await CreateUserAsync("concurrent1", "concurrent1@test.com", "Concurrent", "One");
        var user2 = await CreateUserAsync("concurrent2", "concurrent2@test.com", "Concurrent", "Two");
        var user3 = await CreateUserAsync("concurrent3", "concurrent3@test.com", "Concurrent", "Three");

        // Act - Run multiple API calls concurrently
        var tasks = new[]
        {
            _usersModuleApi.UserExistsAsync(user1.Id.Value),
            _usersModuleApi.UserExistsAsync(user2.Id.Value),
            _usersModuleApi.UserExistsAsync(user3.Id.Value),
        };

        var results = await Task.WhenAll(tasks);

        // Assert - All should succeed
        results.Should().AllSatisfy(result => 
        {
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().BeTrue();
        });
    }

    [Fact]
    public async Task ErrorHandling_NonExistentUser_ShouldHandleGracefully()
    {
        // This test demonstrates how Module API handles non-existent data gracefully

        // Arrange
        var nonExistentId = UuidGenerator.NewId();

        // Act - API calls with non-existent data should not throw
        var userExists = await _usersModuleApi.UserExistsAsync(nonExistentId);
        var getUserResult = await _usersModuleApi.GetUserByIdAsync(nonExistentId);

        // Assert - Should handle gracefully, not throw exceptions
        userExists.IsSuccess.Should().BeTrue();
        userExists.Value.Should().BeFalse();

        getUserResult.IsSuccess.Should().BeTrue();
        getUserResult.Value.Should().BeNull();
    }
}
