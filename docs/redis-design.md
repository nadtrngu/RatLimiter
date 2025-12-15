## Running Redis locally with Docker

Start a local Redis instance:

```bash
docker run -d --name rat-redis redis
```

Connect to it using redis-cli:
```bash
docker exec -it rat-redis redis-cli
```

If you already have the container created:

```bash
docker start rat-redis
docker exec -it rat-redis redis-cli
```

## Redis commands for RatLimiter

I expect to use:

- `SET key value` / `GET key` - store and read scalar values.
- `INCR` / `DECR` / `INCRBY` - update counters atomically.
- `EXPIRE key seconds` - set TTL where useful.
- `HSET key field value` / `HGET key field` / `HGETALL key` - store bucket fields in a hash:
  - tokens
  - capacity
  - refill_rate
  - last_refill

## Token bucket schema in Redis

Per API key, I will store a token bucket as a Redis hash.

Key pattern:

- `rat:bucket:{apiKey}`

Example key:

- `rat:bucket:RAT-123-XYZ`

Fields:

- `tokens` - current number of tokens (int)
- `capacity` - maximum number of tokens the bucket can hold (int)
- `refill_rate` - tokens added per second (int)
- `last_refill` - last refill timestamp as epoch seconds (int)

## API storage schema is Redis

Key pattern:

- `rat:api-keys`: Set of all API keys (used by `GET /v1/api-keys` via `SMEMBERS`)

## Key configurations

Key pattern:

- `rat:config:{apiKey}`

Example key:

- `rat:config:RAT-123-XYZ`

Fields:

- `name` - name assigned to the key (string)
- `description` - description assigned to the key (string, optional)
- `status` - the status of the key, `"active" | "disabled"` (string)
- `algorithm` - the rate limiting algorithm used (string, defaults to "token_bucket")
- `tokens_per_second` - tokens added per second (int)
- `burst_capacity` - max tokens allowed (often equal to `capacity`)
- `created_at` - the timestamp of the api key created

## Endpoints

- `POST /v1/api-keys`

1. Creates a new key:
   a. `HSET rat:config:{apiKey} name {name} description {description} status {status} algorithm {algorithm} tokens_per_second {tokens_per_second} burst_capacity {capacity} created_at {created_at}`
   b. `HSET rat:bucket:{apiKey} tokens {capacity} capacity {capacity} refill_rate {refill_rate} last_refill {last_refill}`
2. Add the new key to the record:
   a. `SADD rat:api-keys {apiKey}`

- `GET /v1/api-keys`:

1. Gets all the API keys:
   a. `SMEMBERS rat:api-keys`
   b. `HGET rat:config:{apiKey}` to fetch specific metadata about the key (e.g., name, status) to show in the list.

- `GET /v1/api-keys/{key}`

1. Get a specific key information:
   a. `HGETALL rat:config:{apiKey}`

- `/v1/check`:

1. Load the hash for `rat:bucket:{apiKey}`.
2. Compute how many tokens to refill based on:
   - `now - last_refill`
   - `refill_rate`
3. Increase `tokens` by that amount, clamp at `capacity`, update `last_refill`.
4. If `tokens >= cost`:
   - decrement `tokens` by `cost`,
   - allow the request.
5. Else:
   - deny the request.

## Atomicity considerations (v1 thoughts)

Multiple Lambda instances may hit the same `rat:bucket:{apiKey}` key at the same time.

A naive `GET` -> compute -> `SET` flow can have race conditions (two Lambdas read the same `tokens` value before either writes back).

For v1 I might:

- Accept some race conditions and keep it simple, **or**
- Use Redis operations like `INCRBY` and `HINCRBY` to do some updates atomically.

For now this is just a note to be careful when implementing the Redis-backed limiter.
