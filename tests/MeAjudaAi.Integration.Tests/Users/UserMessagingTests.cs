using FluentAssertions;
using MeAjudaAi.Integration.Tests.Auth;
using MeAjudaAi.Shared.Messaging.Messages.Users;
using System.Net.Http.Json;
using System.Text.Json;

namespace MeAjudaAi.Integration.Tests.Users;

/// <summary>
/// Testes que verificam se eventos são publicados corretamente através do sistema de messaging
/// </summary>
public class UserMessagingTests : MessagingIntegrationTestBase
{
    public UserMessagingTests()
    {
        // Inicializa o messaging após a criação do factory
    }

    private async Task EnsureMessagingInitializedAsync()
    {
        if (MessagingMocks == null)
        {
            await InitializeTestAsync();
        }
    }
    [Fact]
    public async Task CreateUser_ShouldPublishUserRegisteredEvent()
    {
        // Preparação
        await CleanMessagesAsync();
        this.AuthenticateAsAdmin(); // Configura usuário admin para o teste

        var request = new
        {
            Username = "testuser",
            Email = "test@example.com",
            FirstName = "Test",
            LastName = "User",
            Password = "Password123!",
            Location = new
            {
                Latitude = -23.5505,
                Longitude = -46.6333,
                Address = "São Paulo, SP"
            }
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/v1/users", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created, 
            $"User creation should succeed. Response: {await response.Content.ReadAsStringAsync()}");

        // Verifica se o evento foi publicado
        var wasEventPublished = WasMessagePublished<UserRegisteredIntegrationEvent>(e => 
            e.Email == request.Email);

        wasEventPublished.Should().BeTrue("UserRegisteredIntegrationEvent should be published when user is created");

        // Verifica detalhes do evento
        var publishedEvents = GetPublishedMessages<UserRegisteredIntegrationEvent>();
        var userRegisteredEvent = publishedEvents.FirstOrDefault();
        
        userRegisteredEvent.Should().NotBeNull();
        userRegisteredEvent!.Email.Should().Be(request.Email);
        userRegisteredEvent.FirstName.Should().Be(request.FirstName);
        userRegisteredEvent.LastName.Should().Be(request.LastName);
        userRegisteredEvent.UserId.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public async Task UpdateUserProfile_ShouldPublishUserProfileUpdatedEvent()
    {
        // Arrange - Criar usuário primeiro
        await EnsureMessagingInitializedAsync();
        this.AuthenticateAsAdmin(); // Configura autenticação como admin para criar o usuário
        
        var createRequest = new
        {
            Username = "updateuser",
            Email = "update-test@example.com",
            FirstName = "Update",
            LastName = "User",
            Password = "Password123!",
            Location = new
            {
                Latitude = -23.5505,
                Longitude = -46.6333,
                Address = "São Paulo, SP"
            }
        };

        var createResponse = await Client.PostAsJsonAsync("/api/v1/users", createRequest);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var createResult = await createResponse.Content.ReadAsStringAsync();
        var createData = JsonSerializer.Deserialize<JsonElement>(createResult);
        var userId = createData.GetProperty("data").GetProperty("id").GetGuid();

        // Limpar mensagens da criação (sem limpar banco de dados)
        if (MessagingMocks == null)
        {
            await InitializeTestAsync();
        }
        MessagingMocks?.ClearAllMessages();

        // Configurar autenticação como o usuário criado (para poder atualizar seus próprios dados)
        this.AuthenticateAsUser(userId.ToString(), "updateuser", "update@example.com");

        // Act - Atualizar perfil
        var updateRequest = new
        {
            FirstName = "Updated",
            LastName = "Name",
            Location = new
            {
                Latitude = -22.9068,
                Longitude = -43.1729,
                Address = "Rio de Janeiro, RJ"
            }
        };

        var updateResponse = await Client.PutAsJsonAsync($"/api/v1/users/{userId}/profile", updateRequest);

        // Assert
        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK, 
            $"User update should succeed. Response: {await updateResponse.Content.ReadAsStringAsync()}");

        // Verifica se o evento foi publicado
        var wasEventPublished = WasMessagePublished<UserProfileUpdatedIntegrationEvent>(e => 
            e.UserId == userId);

        wasEventPublished.Should().BeTrue("UserProfileUpdatedIntegrationEvent should be published when user is updated");

        // Verifica detalhes do evento
        var publishedEvents = GetPublishedMessages<UserProfileUpdatedIntegrationEvent>();
        var userUpdatedEvent = publishedEvents.FirstOrDefault();
        
        userUpdatedEvent.Should().NotBeNull();
        userUpdatedEvent!.UserId.Should().Be(userId);
        userUpdatedEvent.FirstName.Should().Be(updateRequest.FirstName);
        userUpdatedEvent.LastName.Should().Be(updateRequest.LastName);
    }

    [Fact]
    public async Task DeleteUser_ShouldPublishUserDeletedEvent()
    {
        // Arrange - Criar usuário primeiro
        await EnsureMessagingInitializedAsync();
        this.AuthenticateAsAdmin(); // Configura autenticação como admin ANTES de criar o usuário
        
        var createRequest = new
        {
            Username = "deleteuser",
            Email = "delete-test@example.com",
            FirstName = "Delete",
            LastName = "User",
            Password = "Password123!",
            Location = new
            {
                Latitude = -23.5505,
                Longitude = -46.6333,
                Address = "São Paulo, SP"
            }
        };

        var createResponse = await Client.PostAsJsonAsync("/api/v1/users", createRequest);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var createResult = await createResponse.Content.ReadAsStringAsync();
        var createData = JsonSerializer.Deserialize<JsonElement>(createResult);
        var userId = createData.GetProperty("data").GetProperty("id").GetGuid();

        // Limpar mensagens da criação (sem limpar banco de dados)
        if (MessagingMocks == null)
        {
            await InitializeTestAsync();
        }
        MessagingMocks?.ClearAllMessages();

        // Act - Deletar usuário
        var deleteResponse = await Client.DeleteAsync($"/api/v1/users/{userId}");

        // Assert
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent, 
            $"User deletion should succeed. Response: {await deleteResponse.Content.ReadAsStringAsync()}");

        // Verifica se o evento foi publicado
        var wasEventPublished = WasMessagePublished<UserDeletedIntegrationEvent>(e => 
            e.UserId == userId);

        wasEventPublished.Should().BeTrue("UserDeletedIntegrationEvent should be published when user is deleted");

        // Verifica detalhes do evento
        var publishedEvents = GetPublishedMessages<UserDeletedIntegrationEvent>();
        var userDeletedEvent = publishedEvents.FirstOrDefault();
        
        userDeletedEvent.Should().NotBeNull();
        userDeletedEvent!.UserId.Should().Be(userId);
    }

    [Fact]
    public async Task MessagingStatistics_ShouldTrackMessageCounts()
    {
        // Arrange
        await EnsureMessagingInitializedAsync();
        this.AuthenticateAsAdmin(); // Configura usuário admin para o teste
        
        var request = new
        {
            Username = "statsuser",
            Email = "stats-test@example.com",
            FirstName = "Stats",
            LastName = "User",
            Password = "Password123!",
            Location = new
            {
                Latitude = -23.5505,
                Longitude = -46.6333,
                Address = "São Paulo, SP"
            }
        };

        var initialStats = GetMessagingStatistics();
        initialStats.TotalMessageCount.Should().Be(0);

        // Act
        var response = await Client.PostAsJsonAsync("/api/v1/users", request);
        
        // Verify user creation succeeded
        response.StatusCode.Should().Be(HttpStatusCode.Created, 
            $"User creation should succeed. Response: {await response.Content.ReadAsStringAsync()}");

        // Assert
        var finalStats = GetMessagingStatistics();
        finalStats.TotalMessageCount.Should().BeGreaterThan(initialStats.TotalMessageCount);
        
        // Pelo menos 1 mensagem deve ter sido publicada (UserRegisteredIntegrationEvent)
        finalStats.TotalMessageCount.Should().BeGreaterThanOrEqualTo(1);
    }
}