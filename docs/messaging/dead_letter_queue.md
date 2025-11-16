# Dead Letter Queue (DLQ) - Strategy and Implementation Guide

## 🎯 Executive Summary

The Dead Letter Queue strategy has been successfully implemented in MeAjudaAi, providing:

- ✅ **Automatic retry** with exponential backoff
- ✅ **Intelligent classification** of failures (permanent vs. temporary)
- ✅ **Multi-environment support** (RabbitMQ for dev, Service Bus for prod)
- ✅ **Complete observability** with structured logs and metrics
- ✅ **Management operations** (reprocess, purge, list)

## 🏗️ Implemented Architecture

```csharp
┌──────────────────┐    ┌─────────────────────┐    ┌──────────────────────┐
│   Event Handler  │───▶│ MessageRetryMiddleware│───▶│  IDeadLetterService  │
│                  │    │                     │    │                      │
│ - UserCreated    │    │ - Retry Logic       │    │ - RabbitMQ (Dev)     │
│ - OrderProcessed │    │ - Backoff Strategy  │    │ - ServiceBus (Prod)  │
│ - EmailSent      │    │ - Exception         │    │ - NoOp (Testing)     │
└──────────────────┘    │   Classification    │    └──────────────────────┘
                        └─────────────────────┘                 │
                                    │                           │
                                    ▼                           ▼
                        ┌─────────────────────┐    ┌──────────────────────┐
                        │     Retry Queue     │    │   Dead Letter Queue  │
                        │                     │    │                      │
                        │ - Delay: 5s, 10s,  │    │ - Failed Messages    │
                        │   20s, 40s...       │    │ - Failure Analysis   │
                        │ - Max: 300s         │    │ - Reprocess Support  │
                        └─────────────────────┘    └──────────────────────┘
```

## 🔧 Implementations

### 1. RabbitMQ Dead Letter Service
**Environment**: Development/Testing

**Features**:
- Automatic Dead Letter Exchange (DLX)
- Configurable TTL for messages in the DLQ
- Routing based on routing keys
- Optional persistence

### 2. Service Bus Dead Letter Service
**Environment**: Production

**Features**:
- Native Azure Service Bus Dead Letter Queue
- Configurable auto-complete
- Adjustable lock duration
- Integration with Service Bus Management API

## 🔁 Retry Strategy

### Retry Policies

#### 1. **Permanent Failures** (No Retry)
- **Examples**: `ArgumentException`, `BusinessRuleException`
- **Action**: Immediate dispatch to DLQ.

#### 2. **Temporary Failures** (Retry Recommended)
- **Examples**: `TimeoutException`, `HttpRequestException`, `PostgresException`
- **Action**: Retry with exponential backoff.

#### 3. **Critical Failures** (No Retry)
- **Examples**: `OutOfMemoryException`, `StackOverflowException`
- **Action**: Immediate dispatch to DLQ + admin notification.

### Exponential Backoff

The delay between retries increases exponentially using the formula `2^(attemptCount-1) * 2` seconds, capped at 300 seconds (5 minutes).

**Retry intervals**: 2s, 4s, 8s, 16s, 32s, 64s, 128s, 256s (then capped at 300s)

## 🔌 Integration with Handlers

The `MessageRetryMiddleware` automatically intercepts failures in event handlers and applies the retry/DLQ strategy.

## 📊 Monitoring and Observability

### Captured Information

The `FailedMessageInfo` class captures detailed information about failed messages, including:
- Message ID, type, and original content
- Source queue and attempt count
- Failure history and environment metadata

### Available Statistics

The `DeadLetterStatistics` class provides an overview of the DLQ, including:
- Total number of dead-lettered messages
- Messages by queue and exception type
- Failure rate by handler

## 🚀 Setup and Configuration

The DLQ system is automatically configured via `services.AddMessaging(configuration, environment);` in `Program.cs`. Environment-specific settings are loaded from `appsettings.Development.json` and `appsettings.Production.json`.

## 🔄 DLQ Operations

The `IDeadLetterService` provides methods for:
- Listing messages in the DLQ
- Reprocessing a specific message
- Purging a message after analysis
- Getting DLQ statistics

## 🧪 Test Coverage

The implementation is covered by a comprehensive suite of unit and integration tests, ensuring the reliability of the DLQ system.

## 🔐 Security Considerations

- Sensitive information is not included in the `OriginalMessage`.
- PII is masked in logs.
- Access to DLQ operations requires admin permissions.
- Messages have a configurable TTL.
