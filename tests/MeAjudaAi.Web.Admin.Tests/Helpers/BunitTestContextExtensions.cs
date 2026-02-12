using Bunit;
using Fluxor;
using MeAjudaAi.Web.Admin.Services.Interfaces;
using MeAjudaAi.Web.Admin.Services.Resilience.Interfaces;
using MeAjudaAi.Web.Admin.Services.Resilience.Http;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using MudBlazor.Services;
using Bunit.TestDoubles;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Authorization;
using MudBlazor;
using System.Security.Claims;

namespace MeAjudaAi.Web.Admin.Tests.Helpers;

public static class BunitTestContextExtensions
{
    public static void AddAdminTestServices(this BunitContext ctx)
    {
        // JSInterop Loose for MudBlazor
        ctx.JSInterop.Mode = JSRuntimeMode.Loose;
        ctx.Services.AddMudServices();
        
        // Setup Authorization manual para bUnit v2
        var mockAuthStateProvider = new Mock<AuthenticationStateProvider>();
        var authState = new AuthenticationState(new ClaimsPrincipal(
            new ClaimsIdentity(new[] 
            { 
                new Claim(ClaimTypes.Name, "Admin"),
                new Claim(ClaimTypes.Role, "Admin")
            }, "TestAuth")));
        
        mockAuthStateProvider.Setup(x => x.GetAuthenticationStateAsync()).ReturnsAsync(authState);
        ctx.Services.AddSingleton(mockAuthStateProvider.Object);
        
        // Mock IAuthorizationService para evitar MissingBunitAuthorizationException
        var mockAuthService = new Mock<IAuthorizationService>();
        mockAuthService.Setup(x => x.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), It.IsAny<string>()))
            .ReturnsAsync(AuthorizationResult.Success());
        mockAuthService.Setup(x => x.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), It.IsAny<IEnumerable<IAuthorizationRequirement>>()))
            .ReturnsAsync(AuthorizationResult.Success());
        ctx.Services.AddSingleton(mockAuthService.Object);
        
        ctx.Services.AddAuthorizationCore();
        
        // Use Wrapper para evitar renderização precoce e fornecer o ambiente necessário (Popover + Auth)
        ctx.RenderTree.Add<BunitTestWrapper>();
        
        // Mock PermissionService
        var mockPermissionService = new Mock<IPermissionService>();
        mockPermissionService.Setup(x => x.HasPermissionAsync(It.IsAny<string>())).ReturnsAsync(true);
        mockPermissionService.Setup(x => x.IsAdminAsync()).ReturnsAsync(true);
        mockPermissionService.Setup(x => x.HasAnyRoleAsync(It.IsAny<string[]>())).ReturnsAsync(true);
        ctx.Services.AddSingleton(mockPermissionService.Object);
        
        // Mock ActionSubscriber (Fluxor)
        var mockActionSubscriber = new Mock<IActionSubscriber>();
        ctx.Services.AddSingleton(mockActionSubscriber.Object);

        // Mock ConnectionStatusService
        var mockConnectionStatusService = new Mock<IConnectionStatusService>();
        mockConnectionStatusService.Setup(x => x.CurrentStatus).Returns(ConnectionStatus.Connected);
        ctx.Services.AddSingleton(mockConnectionStatusService.Object);
    }
}
