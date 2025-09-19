using System.Text.Json;
using Aspire.Hosting;
using Aspire.Hosting.Testing;
using Bogus;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace MeAjudaAi.Integration.Tests.Base;

public abstract class EndToEndTestBase : IAsyncLifetime
{
    protected DistributedApplication App { get; private set; } = null!;
    protected HttpClient ApiClient { get; private set; } = null!;
    protected ResourceNotificationService ResourceNotificationService { get; private set; } = null!;
    protected readonly Faker Faker = new();

    protected static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    public virtual async Task InitializeAsync()
    {
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Testing");
        
        var appHost = await DistributedApplicationTestingBuilder.CreateAsync<Projects.MeAjudaAi_AppHost>();
        App = await appHost.BuildAsync();
        ResourceNotificationService = App.Services.GetRequiredService<ResourceNotificationService>();
        
        await App.StartAsync();
        ApiClient = App.CreateHttpClient("apiservice");
        await WaitForServicesAsync();
    }

    public virtual async Task DisposeAsync()
    {
        ApiClient?.Dispose();
        if (App != null)
        {
            await App.DisposeAsync();
        }
    }

    protected virtual async Task WaitForServicesAsync()
    {
        var timeout = TimeSpan.FromMinutes(5);
        Console.WriteLine("⏳ Waiting for services...");

        try
        {
            await ResourceNotificationService
                .WaitForResourceAsync("postgres-test", KnownResourceStates.Running)
                .WaitAsync(timeout);
            
            await ResourceNotificationService
                .WaitForResourceAsync("redis-test", KnownResourceStates.Running)
                .WaitAsync(timeout);
            
            await ResourceNotificationService
                .WaitForResourceAsync("apiservice", KnownResourceStates.Running)
                .WaitAsync(timeout);
                
            Console.WriteLine("✅ All services ready");
        }
        catch (TimeoutException ex)
        {
            Console.WriteLine($"❌ Timeout: {ex.Message}");
            throw;
        }
    }

    protected async Task<HttpResponseMessage> PostJsonAsync<T>(string requestUri, T content)
    {
        var json = JsonSerializer.Serialize(content, SerializerOptions);
        var stringContent = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
        return await ApiClient.PostAsync(requestUri, stringContent);
    }

    protected async Task<HttpResponseMessage> PutJsonAsync<T>(string requestUri, T content)
    {
        var json = JsonSerializer.Serialize(content, SerializerOptions);
        var stringContent = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
        return await ApiClient.PutAsync(requestUri, stringContent);
    }

    protected Task<HttpResponseMessage> GetAsync(string requestUri) =>
        ApiClient.GetAsync(requestUri);

    protected async Task<T> ReadJsonAsync<T>(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<T>(content, SerializerOptions);
        result.Should().NotBeNull($"Failed to deserialize: {content}");
        return result!;
    }

    protected HttpClient? KeycloakClient => null;
    
    protected Task<string> GetAccessTokenAsync() => Task.FromResult("dummy-token");
    protected Task<string> GetAccessTokenAsync(string username, string password) => Task.FromResult("dummy-token");
    
    protected void SetAuthorizationHeader(string token)
    {
        ApiClient.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
    }

    protected void ClearAuthorizationHeader()
    {
        ApiClient.DefaultRequestHeaders.Authorization = null;
    }

    protected string GetTestEmail() => $"test-{Guid.NewGuid():N}@example.com";
    protected string GetTestUsername() => $"testuser{Guid.NewGuid():N}";

    protected object CreateTestUserRequest()
    {
        return new
        {
            Email = GetTestEmail(),
            Username = GetTestUsername(),
            Name = Faker.Name.FullName(),
            Password = "TestPassword123!",
            PhoneNumber = Faker.Phone.PhoneNumber(),
            DateOfBirth = Faker.Date.Past(30, DateTime.Now.AddYears(-18)).ToString("yyyy-MM-dd")
        };
    }
}
