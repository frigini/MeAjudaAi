using Microsoft.AspNetCore.Components.WebAssembly.Authentication;

namespace MeAjudaAi.Web.Admin.Authentication.Fakes;

/// <summary>
/// Provedor de access token fake para desenvolvimento local.
/// </summary>
public class FakeAccessTokenProvider : IAccessTokenProvider
{
    public ValueTask<AccessTokenResult> RequestAccessToken()
    {
        var token = new AccessToken { Value = "fake_development_token" };
        var result = new AccessTokenResult(AccessTokenResultStatus.Success, token, string.Empty, null);
        return new ValueTask<AccessTokenResult>(result);
    }

    public ValueTask<AccessTokenResult> RequestAccessToken(AccessTokenRequestOptions options)
    {
        return RequestAccessToken();
    }
}
