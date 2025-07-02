# Datadog Logging Implementation

This document describes the Datadog logging implementation in the forecast-agent worker.

## Overview

The logging system sends structured logs to Datadog's HTTP intake API, providing visibility into:
- HTTP request/response metrics
- External API calls
- Errors and exceptions
- Custom events

## Components

### 1. DataDogService (`src/services/logging/data-dog.service.ts`)
- Batches logs in memory
- Flushes logs to Datadog API endpoint
- Handles API authentication

### 2. HttpLogger (`src/services/logging/http-logger.service.ts`)
- Logs HTTP requests with detailed metadata
- Tracks request duration and status codes
- Includes geographic data from Cloudflare
- Associates logs with user sessions

### 3. JobLogger (`src/services/logging/job-logger.service.ts`)
- For background jobs and scheduled tasks
- Simpler logging without HTTP context

### 4. Fetcher Service (`src/services/fetcher.service.ts`)
- Wraps fetch() calls to track external API dependencies
- Automatically logs duration and status of external calls
- Captures exceptions from failed requests

### 5. Logger Middleware (`src/middleware/logger.ts`)
- Automatically tracks all incoming requests
- Measures request duration
- Ensures logs are flushed after each request

## Configuration

Add these environment variables to your `wrangler.jsonc`:

```json
{
  "vars": {
    "DATADOG_API_KEY": "your-datadog-api-key",
    "DATADOG_ENV": "development|staging|production",
    "DATADOG_HOST": "your-app-hostname"
  }
}
```

For local development, you can use a `.dev.vars` file:
```
DATADOG_API_KEY=your-datadog-api-key
DATADOG_ENV=development
DATADOG_HOST=localhost:8787
```

## Usage Examples

### Tracking Events
```typescript
ctx.var.logger.trackEvent('Payment processed', 'Info', {
  amount: 100,
  currency: 'USD',
  customerId: 'cust_123'
});
```

### Tracking Exceptions
```typescript
try {
  // risky operation
} catch (error) {
  ctx.var.logger.trackException('Failed to process payment', error as Error, {
    customerId: 'cust_123'
  });
}
```

### Using the Fetcher for External APIs
```typescript
const response = await ctx.var.fetcher.fetch('https://api.example.com/data');
// This automatically logs the request duration and status
```

## Log Structure

Logs are sent to Datadog with the following structure:
- `ddsource`: Always "worker"
- `ddtags`: Environment and app tags
- `service`: Service name (forecast-agent-worker)
- `status`: Log level (Info, Warn, Error, Notice)
- `message`: Human-readable log message
- `http`: HTTP request metadata (URL, method, status, etc.)
- `network`: Geographic data from Cloudflare
- `usr`: User identification (ID and email)
- `performance`: Duration metrics in nanoseconds
- `data`: Custom data payload

## Best Practices

1. **Use structured logging**: Pass relevant data as objects rather than string concatenation
2. **Track key events**: Log important business events (not just errors)
3. **Include context**: Always include relevant IDs (user, project, task, etc.)
4. **Use appropriate log levels**: Info for normal operations, Warn for recoverable issues, Error for failures
5. **Leverage the Fetcher**: Use ctx.var.fetcher for all external API calls to get automatic tracking

## Monitoring in Datadog

Once configured, you can:
1. Search logs by service, environment, or custom tags
2. Create dashboards for request performance
3. Set up alerts for error rates or slow requests
4. Analyze API dependency performance
5. Track user activity patterns