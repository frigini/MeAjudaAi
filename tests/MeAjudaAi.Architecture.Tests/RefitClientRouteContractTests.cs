using MeAjudaAi.Client.Contracts.Api;
using Refit;
using System.Reflection;

namespace MeAjudaAi.Architecture.Tests;

public class RefitClientRouteContractTests
{
    public static IEnumerable<object[]> AllRefitClientInterfaces()
    {
        return typeof(IPaymentsApi).Assembly.GetTypes()
            .Where(t => t.IsInterface && t.Name.StartsWith("I") && t.Name.EndsWith("Api"))
            .Select(t => new object[] { t })
            .ToList();
    }

    [Theory]
    [MemberData(nameof(AllRefitClientInterfaces))]
    public void AllRefitClients_Routes_ShouldBeVersioned(Type apiType)
    {
        var methods = apiType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

        methods.Should().NotBeEmpty(because: $"{apiType.Name} should have at least one API method");

        foreach (var method in methods)
        {
            var route = GetRefitRoute(method);
            route.Should().Contain("/v1/",
                because: $"{apiType.Name}.{method.Name} route should include API version /v1/ to match versioned API endpoints");
        }
    }

    [Fact]
    public void IPaymentsApi_CreateSubscriptionAsync_ShouldHaveIdempotencyKeyHeader()
    {
        var method = typeof(IPaymentsApi).GetMethod(nameof(IPaymentsApi.CreateSubscriptionAsync));
        method.Should().NotBeNull();

        var idempotencyParam = method!.GetParameters().FirstOrDefault(p => p.Name == "idempotencyKey");
        idempotencyParam.Should().NotBeNull(because: "idempotencyKey parameter should exist");

        var headerAttr = idempotencyParam!.GetCustomAttribute<Refit.HeaderAttribute>();
        headerAttr.Should().NotBeNull(because: "idempotencyKey parameter should have [Header] attribute");
    }

    [Theory]
    [MemberData(nameof(AllRefitClientInterfaces))]
    public void AllRefitClients_BodyDTOs_ShouldComeFromContractsNamespace(Type apiType)
    {
        var methods = apiType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

        foreach (var method in methods)
        {
            var bodyParams = method.GetParameters().Where(p => p.GetCustomAttribute<BodyAttribute>() != null);

            foreach (var param in bodyParams)
            {
                param.ParameterType.Namespace.Should().StartWith("MeAjudaAi.Contracts",
                    because: $"DTO parameter '{param.ParameterType.Name}' in {apiType.Name}.{method.Name} should come from Contracts namespace");
            }
        }
    }

    private static string GetRefitRoute(MethodInfo method)
    {
        var customAttributes = method.GetCustomAttributes(true)
            .Where(a => a is GetAttribute or PostAttribute or PutAttribute or DeleteAttribute or PatchAttribute)
            .ToList();

        customAttributes.Should().NotBeEmpty(because: $"Method {method.Name} should have a Refit HTTP method attribute");

        var pathProperty = typeof(HttpMethodAttribute).GetProperty("Path", BindingFlags.Public | BindingFlags.Instance);
        pathProperty.Should().NotBeNull();

        return (string?)pathProperty!.GetValue(customAttributes.First()) ?? "";
    }
}