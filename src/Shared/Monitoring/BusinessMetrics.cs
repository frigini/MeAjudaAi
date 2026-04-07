using System.Diagnostics.Metrics;
using Microsoft.Extensions.DependencyInjection;

namespace MeAjudaAi.Shared.Monitoring;

/// <summary>
/// Métricas customizadas de negócio para MeAjudaAi
/// </summary>
internal class BusinessMetrics : IDisposable
{
    private readonly Meter _meter;
    private readonly Counter<long> _userRegistrations;
    private readonly Counter<long> _userLogins;
    private readonly Counter<long> _helpRequests;
    private readonly Counter<long> _helpRequestsCompleted;
    private readonly Histogram<double> _helpRequestDuration;
    private readonly Counter<long> _apiCalls;
    private readonly Histogram<double> _databaseQueryDuration;
    private readonly Gauge<long> _activeUsers;
    private readonly Gauge<long> _pendingHelpRequests;

    public BusinessMetrics()
    {
        _meter = new Meter("MeAjudaAi.Business", "1.0.0");

        // User metrics
        _userRegistrations = _meter.CreateCounter<long>(
            "meajudaai.users.registrations.total",
            description: "Total number of user registrations");

        _userLogins = _meter.CreateCounter<long>(
            "meajudaai.users.logins.total",
            description: "Total number of user logins");

        _activeUsers = _meter.CreateGauge<long>(
            "meajudaai.users.active.current",
            description: "Current number of active users");

        // Help request metrics
        _helpRequests = _meter.CreateCounter<long>(
            "meajudaai.help_requests.created.total",
            description: "Total number of help requests created");

        _helpRequestsCompleted = _meter.CreateCounter<long>(
            "meajudaai.help_requests.completed.total",
            description: "Total number of help requests completed");

        _helpRequestDuration = _meter.CreateHistogram<double>(
            "meajudaai.help_requests.duration.seconds",
            unit: "s",
            description: "Duration of help requests from creation to completion");

        _pendingHelpRequests = _meter.CreateGauge<long>(
            "meajudaai.help_requests.pending.current",
            description: "Current number of pending help requests");

        // API metrics
        _apiCalls = _meter.CreateCounter<long>(
            "meajudaai.api.calls.total",
            description: "Total number of API calls by endpoint");

        _databaseQueryDuration = _meter.CreateHistogram<double>(
            "meajudaai.database.query.duration.seconds",
            unit: "s",
            description: "Duration of database queries");
    }

    // User metrics
    public void RecordUserRegistration(string source = "web") =>
        _userRegistrations.Add(1, new KeyValuePair<string, object?>("source", source));

    public void RecordUserLogin(string userId, string method = "password") =>
        _userLogins.Add(1,
            new KeyValuePair<string, object?>("user_id", userId),
            new KeyValuePair<string, object?>("method", method));

    public void UpdateActiveUsers(long count) =>
        _activeUsers.Record(count);

    // Help request metrics
    public void RecordHelpRequestCreated(string category, string urgency) =>
        _helpRequests.Add(1,
            new KeyValuePair<string, object?>("category", category),
            new KeyValuePair<string, object?>("urgency", urgency));

    public void RecordHelpRequestCompleted(string category, TimeSpan duration) =>
        _helpRequestsCompleted.Add(1,
            new KeyValuePair<string, object?>("category", category));

    public void RecordHelpRequestDuration(TimeSpan duration, string category) =>
        _helpRequestDuration.Record(duration.TotalSeconds,
            new KeyValuePair<string, object?>("category", category));

    public void UpdatePendingHelpRequests(long count) =>
        _pendingHelpRequests.Record(count);

    // API metrics
    public void RecordApiCall(string endpoint, string method, int statusCode) =>
        _apiCalls.Add(1,
            new KeyValuePair<string, object?>("endpoint", endpoint),
            new KeyValuePair<string, object?>("method", method),
            new KeyValuePair<string, object?>("status_code", statusCode));

    public void RecordDatabaseQuery(TimeSpan duration, string operation) =>
        _databaseQueryDuration.Record(duration.TotalSeconds,
            new KeyValuePair<string, object?>("operation", operation));

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        _meter.Dispose();
    }
}
