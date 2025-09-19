namespace MeAjudaAi.Shared.Common;

/// <summary>
/// Configuration options for API versioning
/// </summary>
public class ApiVersioningOptions
{
    public const string SectionName = "ApiVersioning";

    /// <summary>
    /// Default API version (e.g., "v1", "v2")
    /// </summary>
    public string DefaultVersion { get; set; } = "v1";

    /// <summary>
    /// Base API path prefix
    /// </summary>
    public string BaseApiPath { get; set; } = "/api";

    /// <summary>
    /// Whether to include version in URL path
    /// </summary>
    public bool UseVersionInPath { get; set; } = true;

    /// <summary>
    /// Whether to support version in query string (?api-version=1.0)
    /// </summary>
    public bool UseVersionInQuery { get; set; } = false;

    /// <summary>
    /// Whether to support version in headers (Api-Version: 1.0)
    /// </summary>
    public bool UseVersionInHeader { get; set; } = false;

    /// <summary>
    /// Header name for version when using header versioning
    /// </summary>
    public string VersionHeaderName { get; set; } = "Api-Version";

    /// <summary>
    /// Query parameter name for version when using query versioning
    /// </summary>
    public string VersionQueryParameter { get; set; } = "api-version";

    /// <summary>
    /// Gets the full API path with version
    /// </summary>
    /// <param name="module">Module name (e.g., "users", "services")</param>
    /// <returns>Full API path (e.g., "/api/v1/users")</returns>
    public string GetApiPath(string module)
    {
        if (UseVersionInPath)
        {
            return $"{BaseApiPath}/{DefaultVersion}/{module}";
        }
        return $"{BaseApiPath}/{module}";
    }

    /// <summary>
    /// Gets the base API path without module
    /// </summary>
    /// <returns>Base API path with version (e.g., "/api/v1")</returns>
    public string GetBaseApiPath()
    {
        if (UseVersionInPath)
        {
            return $"{BaseApiPath}/{DefaultVersion}";
        }
        return BaseApiPath;
    }
}