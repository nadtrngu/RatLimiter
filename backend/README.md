# RatLimiter Backend

## Run Lambda locally with SAM

```bash
cd backend/RatLimiter.Lambda
sam build
sam local start-api
# In another terminal:
curl -X POST http://127.0.0.1:3000/v1/check \
  -H "Content-Type: application/json" \
  -d '{ "apiKey": "RAT-TEST-123" }'
```