# API Design

## Runtime check endpoint
`POST /v1/check`

Used by: integrating client.

Request:
- `apiKey` (string, required)
- `cost` (int, optional, default: 1)

Response:
- 200 OK `{"allowed": true, "remainingTokens": 42, "limit": 100, "resetInSeconds": 12 }`
- 400 Bad Request – invalid payload
- 401 Unauthorized – unknown or disabled apiKey
- 429 Too Many Requests - user hit limit
- 500 Internal Server Error – unexpected failure

## API Key Management:
`POST /v1/api-keys`

Used by: Admin dashboard.

Header:
- `X-Admin-Token` (required)

Request:
- `name` (string, required)
- `description` (string, optional)

Response:
- 200 OK `{"apiKey": "RAT-123-XYZ"}`
- 400 Bad Request – invalid payload
- 401 Unauthorized – missing or invalid admin token
- 500 Internal Server Error – unexpected failure
---

`GET /v1/api-keys`

Used by: Admin dashboard.

Future fields may include: 
- tags
- lastUpdatedBy
- environment ("prod", "dev")

Header:
- `X-Admin-Token` (required)

Response:
- 200 OK `[{"apiKey": "RAT-123-XYZ"}, {"apiKey": "RAT-456-XYZ"}, {"apiKey": "RAT-789-XYZ"}]`
- 400 Bad Request – invalid payload
- 401 Unauthorized – missing or invalid admin token
- 500 Internal Server Error – unexpected failure

---

`GET /v1/api-keys/{key}`

Used by: Admin dashboard.

Header:
- `X-Admin-Token` (required)

Request:
- `key` (string, required)

Response:
- 200 OK `{"apiKey": "RAT-123-XYZ", "name": "Photo Uploader Service", "description": "Handles user photo uploads", "status": "active", "createdAt": "2024-12-10T10:00:00Z", "limits": { "algorithm": "token_bucket","tokensPerSecond": 10, "burstCapacity": 50 }, "metricsSummary": {"allowedCount": 12345, "throttledCount": 321, "lastRequestAt": "2024-12-10T10:04:00Z"}}`
- 400 Bad Request – invalid payload
- 401 Unauthorized – missing or invalid admin token
- 500 Internal Server Error – unexpected failure

## Rate Limit Configuration:

`PUT /v1/api-keys/{key}/limits`

Used by: Admin dashboard.

Header:
- `X-Admin-Token` (required)

Request:
- `key` (string, required)
- `algorithm` (string, required)
- `tokensPerSecond` (int, required)
- `burstCapacity` (int, required)

Response:
- 200 OK
- 400 Bad Request – invalid payload
- 401 Unauthorized – missing or invalid admin token
- 500 Internal Server Error – unexpected failure

---

`GET /v1/api-keys/{key}/limits`

Used by: Admin dashboard

Header:
- `X-Admin-Token` (required)

Request:
- `key` (string, required)

Response:
- 200 OK `{"algorithm": "token_bucket", "tokensPerSecond": 10, "burstCapacity": 50}`
- 400 Bad Request – invalid payload
- 401 Unauthorized – missing or invalid admin token
- 500 Internal Server Error – unexpected failure

## Metrics:

`GET /v1/api-keys/{key}/metrics?from=...&to=...`

Used by: Admin dashboard.

Header:
- `X-Admin-Token` (required)

Request:
- `key` (string, required)
- `from` (DateTime, required)
- `to` (DateTime, required)

Response:
- 200 OK `{"allowedCount": 1234, "throttledCount": 56, "timeSeries": [{ "timestamp": "2024-12-09T10:00:00Z", "allowed": 100, "throttled": 2 }]}`
- 400 Bad Request – invalid payload
- 401 Unauthorized – missing or invalid admin token
- 500 Internal Server Error – unexpected failure

---

Admin Authentication Note (v1):
* Admin-only endpoints require the X-Admin-Token header.
* This value is stored as an environment variable in the Lambda function.
* The admin dashboard passes this header automatically.
* No user/tenant model exists in v1.