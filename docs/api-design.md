# API Design

## Runtime check endpoint
`POST /v1/check`

Used by: integrating client.

Request:
- `apiKey` (string, required)
- `cost` (int, optional, default: 1)

Response:
- 200 OK `{"allowed": true, "remainingTokens": 42, "limit": 100, "resetInSeconds": 12 }`
- 400 Bad Request - invalid payload
- 401 Unauthorized - unknown or disabled apiKey
- 429 Too Many Requests - user hit limit
- 500 Internal Server Error - unexpected failure

Notes:

* `limit` = bucket capacity for that key.
* `resetInSeconds` = estimated time until enough tokens are available again.
    * If `refillRate` is zero or misconfigured, this may be negative or a sentinel value (e.g. -1) to mean "won't reset".

## API Key Management:
`POST /v1/api-keys`

Used by: Admin dashboard.

Header:
- `X-Admin-Token` (required)

Request:
- `name` (string, required)
- `description` (string, optional)
- `status` (string, optional, default: "ACTIVE") - "ACTIVE" or "DISABLED"
- `algorithm` (string, optional, default: "TokenBucket")
- `refillRate` (int, optional, default: 5) - tokens added per second
- `capacity` (int, optional, default: 100) - max tokens in the bucket

Response:
- 200 OK `{"apiKey": "sBu4qbBvFNbRX4D5LsF9"}`
- 400 Bad Request - invalid payload
- 401 Unauthorized - missing or invalid admin token
- 500 Internal Server Error - unexpected failure
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
- 200 OK

```json
{
  "sBu4qbBvFNbRX4D5LsF9": {
    "name": "Photo Uploader Service",
    "description": null,
    "status": "ACTIVE",
    "algorithm": "TokenBucket",
    "refillRate": 5,
    "capacity": 100,
    "createdAt": "2025-12-16T10:15:30Z"
  },
  "9eyUc567QzB1NaiOx7uw": {
    "name": "Reporting Service",
    "description": "Generates daily reports",
    "status": "ACTIVE",
    "algorithm": "TokenBucket",
    "refillRate": 5,
    "capacity": 100,
    "createdAt": "2025-12-16T10:20:00Z"
  }
}
```

- 401 Unauthorized - missing or invalid admin token
- 500 Internal Server Error - unexpected failure

---

`GET /v1/api-keys/{key}`

Used by: Admin dashboard.

Header:
- `X-Admin-Token` (required)

Request:
- `key` (string, required)

Response:
- 200 OK

```json
{
  "name": "Photo Uploader Service",
  "description": "Handles user photo uploads",
  "status": "ACTIVE",
  "algorithm": "TokenBucket",
  "refillRate": 5,
  "capacity": 100,
  "createdAt": "2025-12-16T10:15:30Z"
}
```

- 400 Bad Request - invalid payload
- 401 Unauthorized - missing or invalid admin token
- 404 Not Found - API key not found in Redis
- 500 Internal Server Error - unexpected failure

## Rate Limit Configuration:

`PUT /v1/api-keys/{key}/limits`

Used by: Admin dashboard.

Header:
- `X-Admin-Token` (required)

Request:
- `key` (string, required)
- `algorithm` (string, required)
- `refillRate` (int, required)
- `capacity` (int, required)

Response:
- 204 No Content
- 400 Bad Request - invalid payload
- 401 Unauthorized - missing or invalid admin token
- 404 Not Found - API key not found
- 500 Internal Server Error - unexpected failure

---

`GET /v1/api-keys/{key}/limits`

Used by: Admin dashboard

Header:
- `X-Admin-Token` (required)

Request:
- `key` (string, required)

Response:
- 200 OK `{"algorithm": "token_bucket", "refillRate": 10, "capacity": 50}`
- 400 Bad Request - invalid payload
- 401 Unauthorized - missing or invalid admin token
- 500 Internal Server Error - unexpected failure

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
- 400 Bad Request - invalid payload
- 401 Unauthorized - missing or invalid admin token
- 500 Internal Server Error - unexpected failure

---

Admin Authentication Note (v1):
* Admin-only endpoints require the X-Admin-Token header.
* This value is stored as an environment variable in the Lambda function.
* The admin dashboard passes this header automatically.
* No user/tenant model exists in v1.