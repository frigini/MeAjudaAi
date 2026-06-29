using MeAjudaAi.Modules.Users.Domain.ValueObjects;
using MeAjudaAi.Modules.Users.Infrastructure.Persistence;
using MeAjudaAi.Modules.Users.Infrastructure.Queries;
using MeAjudaAi.Shared.Tests.TestInfrastructure.Base;
using MeAjudaAi.Shared.Tests.TestInfrastructure.Builders.Modules.Users;

namespace MeAjudaAi.Modules.Users.Tests.Unit.Infrastructure.Queries;

[Trait("Category", "Unit")]
public class DbContextUserQueriesTests : BaseInMemoryDatabaseTest<UsersDbContext>
{
    private readonly DbContextUserQueries _queries;

    public DbContextUserQueriesTests()
        : base(options => new UsersDbContext(options))
    {
        _queries = new DbContextUserQueries(DbContext);
    }

    [Fact]
    public async Task GetByIdAsync_WithExistingUser_ShouldReturnUser()
    {
        var user = new UserBuilder().Build();
        DbContext.Users.Add(user);
        await DbContext.SaveChangesAsync();

        var result = await _queries.GetByIdAsync(user.Id);

        result.Should().NotBeNull();
        result!.Id.Should().Be(user.Id);
    }

    [Fact]
    public async Task GetByIdAsync_WithNonExistingUser_ShouldReturnNull()
    {
        var userId = new UserId(Guid.NewGuid());

        var result = await _queries.GetByIdAsync(userId);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByEmailAsync_WithExistingEmail_ShouldReturnUser()
    {
        var email = new Email("test@example.com");
        var user = new UserBuilder().WithEmail(email.Value).Build();
        DbContext.Users.Add(user);
        await DbContext.SaveChangesAsync();

        var result = await _queries.GetByEmailAsync(email);

        result.Should().NotBeNull();
        result!.Email.Should().Be(email);
    }

    [Fact]
    public async Task GetByEmailAsync_WithNonExistingEmail_ShouldReturnNull()
    {
        var email = new Email("nonexistent@example.com");

        var result = await _queries.GetByEmailAsync(email);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByUsernameAsync_WithExistingUsername_ShouldReturnUser()
    {
        var username = new Username("testuser");
        var user = new UserBuilder().WithUsername(username.Value).Build();
        DbContext.Users.Add(user);
        await DbContext.SaveChangesAsync();

        var result = await _queries.GetByUsernameAsync(username);

        result.Should().NotBeNull();
        result!.Username.Should().Be(username);
    }

    [Fact]
    public async Task GetByUsernameAsync_WithNonExistingUsername_ShouldReturnNull()
    {
        var username = new Username("nonexistent");

        var result = await _queries.GetByUsernameAsync(username);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetUsersByIdsAsync_WithValidIds_ShouldReturnUsers()
    {
        var user1 = new UserBuilder().Build();
        var user2 = new UserBuilder().Build();
        var user3 = new UserBuilder().Build();

        DbContext.Users.AddRange(user1, user2, user3);
        await DbContext.SaveChangesAsync();

        var ids = new List<UserId> { user1.Id, user2.Id };

        var result = await _queries.GetUsersByIdsAsync(ids);

        result.Should().HaveCount(2);
        result.Should().Contain(u => u.Id == user1.Id);
        result.Should().Contain(u => u.Id == user2.Id);
    }

    [Fact]
    public async Task GetUsersByIdsAsync_WithEmptyList_ShouldReturnEmpty()
    {
        var ids = new List<UserId>();

        var result = await _queries.GetUsersByIdsAsync(ids);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetUsersByIdsAsync_WithNullList_ShouldReturnEmpty()
    {
        List<UserId>? ids = null;

        var result = await _queries.GetUsersByIdsAsync(ids!);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetPagedAsync_ShouldReturnPagedUsers()
    {
        var users = Enumerable.Range(1, 25)
            .Select(_ => new UserBuilder().Build())
            .ToList();

        DbContext.Users.AddRange(users);
        await DbContext.SaveChangesAsync();

        var (page1Users, page1Total) = await _queries.GetPagedAsync(1, 10);
        var (page2Users, page2Total) = await _queries.GetPagedAsync(2, 10);
        var (page3Users, page3Total) = await _queries.GetPagedAsync(3, 10);

        page1Users.Should().HaveCount(10);
        page1Total.Should().Be(25);
        page2Users.Should().HaveCount(10);
        page2Total.Should().Be(25);
        page3Users.Should().HaveCount(5);
        page3Total.Should().Be(25);
    }

    [Fact]
    public async Task GetPagedAsync_WithInvalidPageNumber_ShouldUsePage1()
    {
        var users = Enumerable.Range(1, 15)
            .Select(_ => new UserBuilder().Build())
            .ToList();
        DbContext.Users.AddRange(users);
        await DbContext.SaveChangesAsync();

        var (result, total) = await _queries.GetPagedAsync(0, 10);

        result.Should().HaveCount(10);
        total.Should().Be(15);
        result.Should().BeEquivalentTo(users.Take(10));
    }

    [Fact]
    public async Task GetPagedWithSearchAsync_WithSearchTerm_ShouldReturnMatchingUsers()
    {
        var user1 = new UserBuilder()
            .WithEmail("john@example.com")
            .WithUsername("johnuser")
            .WithFirstName("John")
            .WithLastName("Doe")
            .Build();
        var user2 = new UserBuilder()
            .WithEmail("jane@example.com")
            .WithUsername("janeuser")
            .WithFirstName("Jane")
            .WithLastName("Smith")
            .Build();

        DbContext.Users.AddRange(user1, user2);
        await DbContext.SaveChangesAsync();

        var (users, total) = await _queries.GetPagedWithSearchAsync(1, 10, "john");

        users.Should().HaveCount(1);
        total.Should().Be(1);
        users.First().FirstName.Should().Be("John");
    }

    [Fact]
    public async Task GetPagedWithSearchAsync_WithEmailSearch_ShouldReturnMatchingUsers()
    {
        var user1 = new UserBuilder().WithEmail("test@example.com").Build();
        var user2 = new UserBuilder().WithEmail("other@domain.com").Build();

        DbContext.Users.AddRange(user1, user2);
        await DbContext.SaveChangesAsync();

        var (users, total) = await _queries.GetPagedWithSearchAsync(1, 10, "example");

        users.Should().HaveCount(1);
        total.Should().Be(1);
        users.First().Email.Value.Should().Be("test@example.com");
    }

    [Fact]
    public async Task GetPagedWithSearchAsync_WithUsernameSearch_ShouldReturnMatchingUsers()
    {
        var user1 = new UserBuilder().WithUsername("testuser123").Build();
        var user2 = new UserBuilder().WithUsername("otheruser456").Build();

        DbContext.Users.AddRange(user1, user2);
        await DbContext.SaveChangesAsync();

        var (users, total) = await _queries.GetPagedWithSearchAsync(1, 10, "testuser");

        users.Should().HaveCount(1);
        total.Should().Be(1);
        users.First().Username.Value.Should().Be("testuser123");
    }

    [Fact]
    public async Task GetPagedWithSearchAsync_WithLastNameSearch_ShouldReturnMatchingUsers()
    {
        var user1 = new UserBuilder().WithLastName("Smith").WithEmail("user1@example.com").WithUsername("user1").Build();
        var user2 = new UserBuilder().WithLastName("Johnson").WithEmail("user2@example.com").WithUsername("user2").Build();

        DbContext.Users.AddRange(user1, user2);
        await DbContext.SaveChangesAsync();

        var (users, total) = await _queries.GetPagedWithSearchAsync(1, 10, "smith");

        users.Should().HaveCount(1);
        total.Should().Be(1);
        users.First().LastName.Should().Be("Smith");
    }

    [Fact]
    public async Task GetPagedWithSearchAsync_WithEmptySearchTerm_ShouldReturnAllUsers()
    {
        var user1 = new UserBuilder().Build();
        var user2 = new UserBuilder().Build();

        DbContext.Users.AddRange(user1, user2);
        await DbContext.SaveChangesAsync();

        var (users, total) = await _queries.GetPagedWithSearchAsync(1, 10, "");

        users.Should().HaveCount(2);
        total.Should().Be(2);
    }

    [Fact]
    public async Task GetPagedWithSearchAsync_WithNullSearchTerm_ShouldReturnAllUsers()
    {
        var user1 = new UserBuilder().Build();
        var user2 = new UserBuilder().Build();

        DbContext.Users.AddRange(user1, user2);
        await DbContext.SaveChangesAsync();

        var (users, total) = await _queries.GetPagedWithSearchAsync(1, 10, null);

        users.Should().HaveCount(2);
        total.Should().Be(2);
    }

    [Fact]
    public async Task GetByKeycloakIdAsync_WithExistingKeycloakId_ShouldReturnUser()
    {
        var keycloakId = Guid.NewGuid().ToString();
        var user = new UserBuilder().WithKeycloakId(keycloakId).Build();
        DbContext.Users.Add(user);
        await DbContext.SaveChangesAsync();

        var result = await _queries.GetByKeycloakIdAsync(keycloakId);

        result.Should().NotBeNull();
        result!.KeycloakId.Should().Be(keycloakId);
    }

    [Fact]
    public async Task GetByKeycloakIdAsync_WithNonExistingKeycloakId_ShouldReturnNull()
    {
        var keycloakId = Guid.NewGuid().ToString();

        var result = await _queries.GetByKeycloakIdAsync(keycloakId);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByKeycloakIdAsync_WithEmptyKeycloakId_ShouldReturnNull()
    {
        var keycloakId = "";

        var result = await _queries.GetByKeycloakIdAsync(keycloakId);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByKeycloakIdAsync_WithWhitespaceKeycloakId_ShouldReturnNull()
    {
        var keycloakId = "   ";

        var result = await _queries.GetByKeycloakIdAsync(keycloakId);

        result.Should().BeNull();
    }

    [Fact]
    public async Task ExistsAsync_WithExistingUser_ShouldReturnTrue()
    {
        var user = new UserBuilder().Build();
        DbContext.Users.Add(user);
        await DbContext.SaveChangesAsync();

        var result = await _queries.ExistsAsync(user.Id);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsAsync_WithNonExistingUser_ShouldReturnFalse()
    {
        var userId = new UserId(Guid.NewGuid());

        var result = await _queries.ExistsAsync(userId);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task GetPagedAsync_WithInvalidPageSize_ShouldUseMinimumPageSize()
    {
        var users = Enumerable.Range(1, 5)
            .Select(_ => new UserBuilder().Build())
            .ToList();
        DbContext.Users.AddRange(users);
        await DbContext.SaveChangesAsync();

        var (result, total) = await _queries.GetPagedAsync(1, 0);

        result.Should().HaveCount(1);
        total.Should().Be(5);
        result.Should().ContainEquivalentOf(users[0]);
    }

    [Fact]
    public async Task GetPagedWithSearchAsync_WithInvalidPageNumber_ShouldUsePage1()
    {
        var users = Enumerable.Range(1, 15)
            .Select(_ => new UserBuilder().Build())
            .ToList();
        DbContext.Users.AddRange(users);
        await DbContext.SaveChangesAsync();

        var (result, total) = await _queries.GetPagedWithSearchAsync(0, 10, null);

        result.Should().HaveCount(10);
        total.Should().Be(15);
        result.Should().BeEquivalentTo(users.Take(10));
    }

    [Fact]
    public async Task GetPagedWithSearchAsync_WithInvalidPageSize_ShouldUseMinimumPageSize()
    {
        var users = Enumerable.Range(1, 5)
            .Select(_ => new UserBuilder().Build())
            .ToList();
        DbContext.Users.AddRange(users);
        await DbContext.SaveChangesAsync();

        var (result, total) = await _queries.GetPagedWithSearchAsync(1, 0, null);

        result.Should().HaveCount(1);
        total.Should().Be(5);
        result.Should().ContainEquivalentOf(users[0]);
    }

    [Fact]
    public async Task GetPagedWithSearchAsync_WithNoMatches_ShouldReturnEmpty()
    {
        var user = new UserBuilder()
            .WithEmail("test@example.com")
            .WithUsername("testuser")
            .Build();
        DbContext.Users.Add(user);
        await DbContext.SaveChangesAsync();

        var (users, total) = await _queries.GetPagedWithSearchAsync(1, 10, "nonexistent");

        users.Should().BeEmpty();
        total.Should().Be(0);
    }

    [Fact]
    public async Task GetByKeycloakIdAsync_WithNullKeycloakId_ShouldReturnNull()
    {
        string? keycloakId = null;

        var result = await _queries.GetByKeycloakIdAsync(keycloakId!);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetUsersByIdsAsync_WithMoreThanBatchSize_ShouldReturnUsersFromAllChunks()
    {
        var users = Enumerable.Range(1, 2100)
            .Select(_ => new UserBuilder().Build())
            .ToList();

        DbContext.Users.AddRange(users);
        await DbContext.SaveChangesAsync();

        var ids = users.Select(u => u.Id).ToList();

        var result = await _queries.GetUsersByIdsAsync(ids);

        result.Should().HaveCount(2100);
    }
}
