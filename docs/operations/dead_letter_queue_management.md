# Dead Letter Queue Management Guide

## 🛠️ Operational Tasks

### 1. Monitoring Dead Letter Queues

#### Check DLQ Status (Development - RabbitMQ)
```bash
# Connect to RabbitMQ container
docker exec -it meajudaai-rabbitmq rabbitmqctl list_queues name messages

# Filter DLQ queues
docker exec -it meajudaai-rabbitmq rabbitmqctl list_queues name messages | grep dlq

# Detailed queue information
docker exec -it meajudaai-rabbitmq rabbitmqctl list_queues name messages consumers memory
```csharp
#### Check DLQ Status (Production - Azure Service Bus)
```bash
# Using Azure CLI
az servicebus queue list --resource-group meajudaai-rg --namespace-name meajudaai-sb --query "[?contains(name, 'DeadLetter')]"

# Get specific queue details
az servicebus queue show --resource-group meajudaai-rg --namespace-name meajudaai-sb --name "users-events\$DeadLetterQueue"
```text
### 2. Application-Level Monitoring

#### Get DLQ Statistics via API
```csharp
// In a controller or management service
[HttpGet("admin/deadletter/statistics")]
public async Task<IActionResult> GetDeadLetterStatistics()
{
    var statistics = await _deadLetterService.GetDeadLetterStatisticsAsync();
    return Ok(statistics);
}

// Response example:
{
    "totalDeadLetterMessages": 15,
    "messagesByQueue": {
        "dlq.users-events": 8,
        "dlq.billing-events": 5,
        "dlq.notification-events": 2
    },
    "messagesByExceptionType": {
        "TimeoutException": 7,
        "ArgumentException": 5,
        "PostgresException": 3
    },
    "failureRateByHandler": {
        "UserCreatedEventHandler": {
            "totalMessages": 1000,
            "failedMessages": 8,
            "failurePercentage": 0.8,
            "lastFailure": "2025-10-08T10:30:00Z"
        }
    },
    "lastUpdated": "2025-10-08T12:00:00Z"
}
```csharp
### 3. Message Analysis and Recovery

#### List Messages in DLQ
```csharp
// Get first 20 messages from DLQ for analysis
var messages = await _deadLetterService.ListDeadLetterMessagesAsync("dlq.users-events", 20);

foreach (var message in messages)
{
    Console.WriteLine($"Message ID: {message.MessageId}");
    Console.WriteLine($"Type: {message.MessageType}");
    Console.WriteLine($"Failed {message.AttemptCount} times");
    Console.WriteLine($"Last failure: {message.LastFailureReason}");
    Console.WriteLine($"First attempt: {message.FirstAttemptAt}");
    Console.WriteLine($"Last attempt: {message.LastAttemptAt}");
    Console.WriteLine("---");
}
```csharp
#### Reprocess Messages
```csharp
// Reprocess single message
await _deadLetterService.ReprocessDeadLetterMessageAsync("dlq.users-events", "message-id-123");

// Bulk reprocessing with conditions
var messages = await _deadLetterService.ListDeadLetterMessagesAsync("dlq.users-events", 50);
var reprocessableMessages = messages.Where(m => 
    m.AttemptCount <= 3 && 
    m.LastAttemptAt > DateTime.UtcNow.AddHours(-1) &&
    !m.LastFailureReason.Contains("ArgumentException"));

foreach (var message in reprocessableMessages)
{
    try
    {
        await _deadLetterService.ReprocessDeadLetterMessageAsync("dlq.users-events", message.MessageId);
        Console.WriteLine($"Reprocessed message {message.MessageId}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Failed to reprocess message {message.MessageId}: {ex.Message}");
    }
}
```text
#### Clean Up Old Messages
```csharp
// Purge messages older than 7 days
var messages = await _deadLetterService.ListDeadLetterMessagesAsync("dlq.users-events", 100);
var oldMessages = messages.Where(m => m.FirstAttemptAt < DateTime.UtcNow.AddDays(-7));

foreach (var message in oldMessages)
{
    await _deadLetterService.PurgeDeadLetterMessageAsync("dlq.users-events", message.MessageId);
    Console.WriteLine($"Purged old message {message.MessageId}");
}
```csharp
### 4. Automated Scripts

#### PowerShell Script for DLQ Monitoring
```powershell
# DeadLetterMonitor.ps1
param(
    [string]$Environment = "Development",
    [int]$MaxMessages = 50
)

function Get-DLQStatistics {
    param([string]$ApiBaseUrl)
    
    $response = Invoke-RestMethod -Uri "$ApiBaseUrl/admin/deadletter/statistics" -Method Get
    return $response
}

function Send-DLQAlert {
    param([object]$Statistics)
    
    if ($Statistics.totalDeadLetterMessages -gt 10) {
        Write-Warning "High number of DLQ messages: $($Statistics.totalDeadLetterMessages)"
        
        # Send notification (Teams, Slack, Email, etc.)
        # Invoke-RestMethod -Uri $TeamsWebhookUrl -Method Post -Body $alertPayload
    }
}

# Main execution
$apiUrl = if ($Environment -eq "Production") { "https://api.meajudaai.com" } else { "https://localhost:5001" }

try {
    $stats = Get-DLQStatistics -ApiBaseUrl $apiUrl
    Write-Output "DLQ Statistics:"
    Write-Output "Total Messages: $($stats.totalDeadLetterMessages)"
    
    foreach ($queue in $stats.messagesByQueue.PSObject.Properties) {
        Write-Output "  $($queue.Name): $($queue.Value) messages"
    }
    
    Send-DLQAlert -Statistics $stats
}
catch {
    Write-Error "Failed to retrieve DLQ statistics: $($_.Exception.Message)"
}
```yaml
#### Bash Script for RabbitMQ DLQ Monitoring
```bash
#!/bin/bash
# dlq_monitor.sh

RABBITMQ_CONTAINER="meajudaai-rabbitmq"
ALERT_THRESHOLD=10

# Function to get DLQ message count
get_dlq_count() {
    docker exec $RABBITMQ_CONTAINER rabbitmqctl list_queues name messages | grep dlq | awk '{sum += $2} END {print sum+0}'
}

# Function to send alert
send_alert() {
    local count=$1
    echo "ALERT: $count messages in Dead Letter Queues"
    
    # Example: Send to Slack
    # curl -X POST -H 'Content-type: application/json' \
    #   --data "{\"text\":\"DLQ Alert: $count messages pending\"}" \
    #   $SLACK_WEBHOOK_URL
}

# Main execution
DLQ_COUNT=$(get_dlq_count)
echo "Current DLQ message count: $DLQ_COUNT"

if [ "$DLQ_COUNT" -gt "$ALERT_THRESHOLD" ]; then
    send_alert $DLQ_COUNT
fi

# Detailed breakdown
echo "DLQ Breakdown:"
docker exec $RABBITMQ_CONTAINER rabbitmqctl list_queues name messages | grep dlq
```csharp
### 5. Troubleshooting Common Issues

#### Issue: Messages Not Going to DLQ
```csharp
// Check configuration
var options = serviceProvider.GetRequiredService<IOptions<DeadLetterOptions>>().Value;
Console.WriteLine($"DLQ Enabled: {options.Enabled}");
Console.WriteLine($"Max Retry Attempts: {options.MaxRetryAttempts}");

// Check if exception is classified correctly
var exception = new TimeoutException("Test");
var failureType = exception.ClassifyFailure();
Console.WriteLine($"Exception classified as: {failureType}");

// Test retry logic
var deadLetterService = serviceProvider.GetRequiredService<IDeadLetterService>();
var shouldRetry = deadLetterService.ShouldRetry(exception, attemptCount: 1);
Console.WriteLine($"Should retry: {shouldRetry}");
```text
#### Issue: Too Many Retries
```json
// Adjust configuration
{
  "Messaging": {
    "DeadLetter": {
      "MaxRetryAttempts": 2,
      "InitialRetryDelaySeconds": 1,
      "BackoffMultiplier": 1.5,
      "NonRetryableExceptions": [
        "YourCustom.BusinessException",
        "System.ArgumentException"
      ]
    }
  }
}
```csharp
#### Issue: Messages Lost in DLQ
```csharp
// Increase TTL for investigation
{
  "Messaging": {
    "DeadLetter": {
      "DeadLetterTtlHours": 168  // 7 days instead of 3
    }
  }
}

// Enable detailed logging
{
  "Logging": {
    "LogLevel": {
      "MeAjudaAi.Shared.Messaging.DeadLetter": "Debug"
    }
  }
}
```bash
### 6. Performance Optimization

#### Batch Processing for DLQ Operations
```csharp
public async Task ProcessDLQInBatches(string queueName, int batchSize = 10)
{
    bool hasMoreMessages = true;
    
    while (hasMoreMessages)
    {
        var messages = await _deadLetterService.ListDeadLetterMessagesAsync(queueName, batchSize);
        
        if (!messages.Any())
        {
            hasMoreMessages = false;
            continue;
        }
        
        var tasks = messages.Select(async message =>
        {
            try
            {
                if (ShouldReprocess(message))
                {
                    await _deadLetterService.ReprocessDeadLetterMessageAsync(queueName, message.MessageId);
                }
                else if (ShouldPurge(message))
                {
                    await _deadLetterService.PurgeDeadLetterMessageAsync(queueName, message.MessageId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process DLQ message {MessageId}", message.MessageId);
            }
        });
        
        await Task.WhenAll(tasks);
        
        // Small delay to avoid overwhelming the system
        await Task.Delay(TimeSpan.FromSeconds(1));
    }
}

private bool ShouldReprocess(FailedMessageInfo message)
{
    return message.AttemptCount <= 3 && 
           message.LastAttemptAt > DateTime.UtcNow.AddHours(-1) &&
           !IsKnownPermanentFailure(message.LastFailureReason);
}

private bool ShouldPurge(FailedMessageInfo message)
{
    return message.FirstAttemptAt < DateTime.UtcNow.AddDays(-7) ||
           IsKnownPermanentFailure(message.LastFailureReason);
}
```text
This guide provides comprehensive operational procedures for managing Dead Letter Queues in both development and production environments.