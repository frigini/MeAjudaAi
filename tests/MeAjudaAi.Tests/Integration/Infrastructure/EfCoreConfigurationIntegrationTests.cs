using FluentAssertions;
using MeAjudaAi.Modules.Users.Domain.Entities;
using MeAjudaAi.Modules.Users.Domain.ValueObjects;
using MeAjudaAi.Modules.Users.Infrastructure.Persistence;
using MeAjudaAi.Shared.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace MeAjudaAi.Tests.Integration.Infrastructure;

[Collection("Database")]
public class EfCoreConfigurationIntegrationTests : BaseIntegrationTest
{
    private readonly UsersDbContext _context;

    public EfCoreConfigurationIntegrationTests(TestApplicationFactory factory) : base(factory)
    {
        _context = Services.GetRequiredService<UsersDbContext>();
    }

    [Fact]
    public async Task UserEntity_ShouldBeMappedCorrectlyToDatabase()
    {
        // Arrange
        var email = Email.Create("config.test@example.com");
        var username = Username.Create("configtest");
        var profile = UserProfile.Create("Config", "Test", "+5511999999999");
        
        var user = User.Create(email, username, profile);

        // Act
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Clear context to ensure fresh read
        _context.ChangeTracker.Clear();

        // Assert - Test database mapping
        var savedUser = await _context.Users
            .FirstOrDefaultAsync(u => u.Email.Value == "config.test@example.com");

        savedUser.Should().NotBeNull();
        
        // Testa mapeamentos de Value Objects
        savedUser!.Email.Value.Should().Be("config.test@example.com");
        savedUser.Username.Value.Should().Be("configtest");
        savedUser.Profile.FirstName.Should().Be("Config");
        savedUser.Profile.LastName.Should().Be("Test");
        savedUser.Profile.PhoneNumber.Value.Should().Be("+5511999999999");
        
        // Testa propriedades da entidade
        savedUser.Id.Should().NotBeNull();
        savedUser.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
        savedUser.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
    }

    [Fact]
    public async Task EmailValueObject_ShouldBeStoredAsStringInDatabase()
    {
        // Arrange
        var email = Email.Create("email.vo.test@example.com");
        var username = Username.Create("emailvotest");
        var profile = UserProfile.Create("Email", "Test", "+5511999999999");
        
        var user = User.Create(email, username, profile);

        // Act
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Assert - Check raw database value
        var emailValue = await _context.Database
            .SqlQueryRaw<string>("SELECT email FROM users WHERE email = 'email.vo.test@example.com'")
            .FirstOrDefaultAsync();

        emailValue.Should().Be("email.vo.test@example.com");
    }

    [Fact]
    public async Task UsernameValueObject_ShouldBeStoredAsStringInDatabase()
    {
        // Arrange
        var email = Email.Create("username.vo.test@example.com");
        var username = Username.Create("usernametest");
        var profile = UserProfile.Create("Username", "Test", "+5511999999999");
        
        var user = User.Create(email, username, profile);

        // Act
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Assert - Check raw database value
        var usernameValue = await _context.Database
            .SqlQueryRaw<string>("SELECT username FROM users WHERE username = 'usernametest'")
            .FirstOrDefaultAsync();

        usernameValue.Should().Be("usernametest");
    }

    [Fact]
    public async Task UserProfileValueObject_ShouldBeStoredAsJsonInDatabase()
    {
        // Arrange
        var email = Email.Create("profile.vo.test@example.com");
        var username = Username.Create("profiletest");
        var profile = UserProfile.Create("Profile", "Test", "+5511999999999");
        
        var user = User.Create(email, username, profile);

        // Act
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Assert - Check that profile fields are stored correctly
        var storedProfile = await _context.Database
            .SqlQueryRaw<string>("SELECT profile::text FROM users WHERE email = 'profile.vo.test@example.com'")
            .FirstOrDefaultAsync();

        storedProfile.Should().NotBeNullOrEmpty();
        storedProfile.Should().Contain("Profile");
        storedProfile.Should().Contain("Test");
        storedProfile.Should().Contain("+5511999999999");
    }

    [Fact]
    public async Task UserId_ShouldBeStoredAsUuidInDatabase()
    {
        // Arrange
        var email = Email.Create("userid.test@example.com");
        var username = Username.Create("useridtest");
        var profile = UserProfile.Create("UserId", "Test", "+5511999999999");
        
        var user = User.Create(email, username, profile);

        // Act
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Assert - Check that UserId is stored as UUID
        var userIdValue = await _context.Database
            .SqlQueryRaw<Guid>("SELECT id FROM users WHERE email = 'userid.test@example.com'")
            .FirstOrDefaultAsync();

        userIdValue.Should().NotBeEmpty();
        userIdValue.Should().Be(user.Id.Value);
    }

    [Fact]
    public async Task DatabaseConstraints_ShouldBeEnforced()
    {
        // Arrange
        var email = Email.Create("constraint.test@example.com");
        var username = Username.Create("constrainttest");
        var profile = UserProfile.Create("Constraint", "Test", "+5511999999999");
        
        var user1 = User.Create(email, username, profile);
        var user2 = User.Create(email, username, profile); // Same email and username

        // Act & Assert - First user should save successfully
        _context.Users.Add(user1);
        await _context.SaveChangesAsync();

        // Second user with same email should fail due to unique constraint
        _context.Users.Add(user2);
        
        var act = async () => await _context.SaveChangesAsync();
        await act.Should().ThrowAsync<DbUpdateException>();
    }

    [Fact]
    public async Task TableName_ShouldBeCorrect()
    {
        // Assert - Check that table name is correctly mapped
        var tableNames = await _context.Database
            .SqlQueryRaw<string>(
                "SELECT table_name FROM information_schema.tables WHERE table_schema = 'users' AND table_type = 'BASE TABLE'")
            .ToListAsync();

        tableNames.Should().Contain("users");
    }

    [Fact]
    public async Task ColumnNames_ShouldBeSnakeCase()
    {
        // Assert - Check that column names follow snake_case convention
        var columnNames = await _context.Database
            .SqlQueryRaw<string>(
                @"SELECT column_name 
                  FROM information_schema.columns 
                  WHERE table_schema = 'users' 
                  AND table_name = 'users'
                  ORDER BY ordinal_position")
            .ToListAsync();

        columnNames.Should().Contain("id");
        columnNames.Should().Contain("email");
        columnNames.Should().Contain("username");
        columnNames.Should().Contain("profile");
        columnNames.Should().Contain("created_at");
        columnNames.Should().Contain("updated_at");
        columnNames.Should().Contain("last_username_change_at");
    }

    [Fact]
    public async Task EmailIndex_ShouldExist()
    {
        // Assert - Check that email index exists
        var emailIndexes = await _context.Database
            .SqlQueryRaw<string>(
                @"SELECT indexname 
                  FROM pg_indexes 
                  WHERE tablename = 'users' 
                  AND schemaname = 'users'
                  AND indexname LIKE '%email%'")
            .ToListAsync();

        emailIndexes.Should().NotBeEmpty();
    }

    [Fact]
    public async Task UsernameIndex_ShouldExist()
    {
        // Assert - Check that username index exists
        var usernameIndexes = await _context.Database
            .SqlQueryRaw<string>(
                @"SELECT indexname 
                  FROM pg_indexes 
                  WHERE tablename = 'users' 
                  AND schemaname = 'users'
                  AND indexname LIKE '%username%'")
            .ToListAsync();

        usernameIndexes.Should().NotBeEmpty();
    }

    [Fact]
    public async Task CreatedAtColumn_ShouldHaveDefaultValue()
    {
        // Arrange
        var email = Email.Create("createdat.test@example.com");
        var username = Username.Create("createdattest");
        var profile = UserProfile.Create("CreatedAt", "Test", "+5511999999999");
        
        var user = User.Create(email, username, profile);

        // Act
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Assert
        var createdAt = await _context.Database
            .SqlQueryRaw<DateTime>("SELECT created_at FROM users WHERE email = 'createdat.test@example.com'")
            .FirstOrDefaultAsync();

        createdAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
    }

    [Fact]
    public async Task UpdatedAtColumn_ShouldBeUpdatedOnModification()
    {
        // Arrange
        var email = Email.Create("updatedat.test@example.com");
        var username = Username.Create("updatedattest");
        var profile = UserProfile.Create("UpdatedAt", "Test", "+5511999999999");
        
        var user = User.Create(email, username, profile);

        // Act - Initial save
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        
        var initialUpdatedAt = user.UpdatedAt;

        // Wait a moment to ensure time difference
        await Task.Delay(100);

        // Atualiza o usu�rio
        var newProfile = UserProfile.Create("UpdatedAt", "Modified", "+5511888888888");
        user.UpdateProfile(newProfile);
        
        await _context.SaveChangesAsync();

        // Assert
        var updatedAt = await _context.Database
            .SqlQueryRaw<DateTime>("SELECT updated_at FROM users WHERE email = 'updatedat.test@example.com'")
            .FirstOrDefaultAsync();

        updatedAt.Should().BeAfter(initialUpdatedAt);
    }

    [Fact]
    public async Task DatabaseSchema_ShouldBeCorrect()
    {
        // Assert - Check that users schema exists
        var schemaExists = await _context.Database
            .SqlQueryRaw<bool>(
                "SELECT EXISTS(SELECT 1 FROM information_schema.schemata WHERE schema_name = 'users')")
            .FirstOrDefaultAsync();

        schemaExists.Should().BeTrue();
    }

    [Fact]
    public async Task ValueObjectConversions_ShouldWorkBidirectionally()
    {
        // Arrange
        var originalEmail = Email.Create("conversion.test@example.com");
        var originalUsername = Username.Create("conversiontest");
        var originalProfile = UserProfile.Create("Conversion", "Test", "+5511999999999");
        
        var user = User.Create(originalEmail, originalUsername, originalProfile);

        // Act - Save and reload
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        
        _context.ChangeTracker.Clear();
        
        var reloadedUser = await _context.Users
            .FirstOrDefaultAsync(u => u.Email.Value == "conversion.test@example.com");

        // Assert - Value objects should be correctly reconstructed
        reloadedUser.Should().NotBeNull();
        reloadedUser!.Email.Should().BeEquivalentTo(originalEmail);
        reloadedUser.Username.Should().BeEquivalentTo(originalUsername);
        reloadedUser.Profile.FirstName.Should().Be(originalProfile.FirstName);
        reloadedUser.Profile.LastName.Should().Be(originalProfile.LastName);
        reloadedUser.Profile.PhoneNumber.Value.Should().Be(originalProfile.PhoneNumber.Value);
    }

    protected override async Task CleanupAsync()
    {
        // Cleanup all test data
        var testUsers = await _context.Users
            .Where(u => u.Email.Value.Contains(".test@example.com"))
            .ToListAsync();

        _context.Users.RemoveRange(testUsers);
        await _context.SaveChangesAsync();
    }
}
