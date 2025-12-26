# RatLimiter Admin UI

This is the React + TypeScript + Vite frontend for **RatLimiter**, a rate limiting service backed by AWS Lambda and Redis.

The Admin UI lets you:

- View all registered API keys and their current limits
- Edit rate limits (capacity and refill rate) for an existing key
- Create new API keys from the UI
- (Soon) View basic telemetry and usage metrics

> Note: This app is intended as an internal admin console, not a public-facing UI.

---

## Tech Stack

- **React** (TypeScript)
- **Vite** for bundling & dev server
- **MUI (Material UI)** for styling and layout
- **Axios** for HTTP calls to the backend

---

## Prerequisites

- Node.js (LTS version)
- RatLimiter backend running locally (via `sam local start-api` or equivalent)
- A valid **admin token** configured on the backend (`ADMIN_TOKEN` env var)

---

## Environment Variables

Create a `.env` file in the `frontend` folder:

```bash
VITE_BASE_URL=http://127.0.0.1:3000
VITE_ADMIN_TOKEN=RAT-LIMITER-xxxxxx
```

- `VITE_BASE_URL` should point at your running RatLimiter backend.
- `VITE_ADMIN_TOKEN` must match the `ADMIN_TOKEN` environment variable configured for the Lambda.

---

## Getting Started

### Install dependencies

```bash
cd frontend
npm install
```

### Run the dev server

```bash
npm run dev
```

By default Vite will start on `http://localhost:5173`.  
Make sure the backend is running (for example with `sam local start-api --port 3000`) so the UI can talk to the API.

---

## How the UI Talks to the Backend

All HTTP calls are made through a small Axios client (`src/api/apiClient.ts`).

- `baseURL` is taken from `VITE_BASE_URL`
- Every request includes the `X-Admin-Token` header

Example:

```ts
// src/api/apiClient.ts
import axios from "axios";

const apiClient = axios.create({
  baseURL: import.meta.env.VITE_BASE_URL,
  headers: {
    "Content-Type": "application/json",
    "X-Admin-Token": import.meta.env.VITE_ADMIN_TOKEN,
  },
});

export default apiClient;
```

If the admin token is missing or incorrect, the backend will return `401 Unauthorized`.

---

## Current Screens

### API Keys Page (`/api-keys`)

- Fetches all API keys via `GET /v1/api-keys`
- Renders a table with:
  - API key
  - Name
  - Capacity
  - Refill rate (per second)
  - Status
  - Algorithm
  - Created date
- Shows a friendly empty state when there are no keys
- Allows editing limits for an existing key

When you click the edit icon on a row:

- A modal opens with a small form
- You can change `capacity` and `refillRate`
- On submit the UI calls `PUT /v1/api-keys/{key}/limits`
- After a successful update, the table data is refreshed

A "CREATE API KEY" button above the API keys table:

- A modal opens with a create new key form with the field:
  - Name (string, required)
  - Description (string, optional)
  - Capacity (number, required)
  - Refill Rate (number, required)
  - Algorithm -> defaults to Token Bucket
  - Status: Active | Disabled

---

## Scripts

From the `frontend` directory:

- `npm run dev` – start the Vite dev server
- `npm run build` – create a production build
- `npm run preview` – preview the production build locally
- `npm run lint` – run ESLint

---

## Notes & Future Improvements

Planned improvements for the Admin UI:

- **Create API key flow** – a form + dialog that calls `POST /v1/api-keys`
- **Basic metrics / telemetry view** – surface `allowed / throttled` counts per key
- **Better validation & error messages** in forms
- **Theming & polish** – more consistent spacing, colors, and dark mode tweaks

This UI is intentionally small and focused: it’s meant to showcase how a simple React admin console can sit in front of a Lambda + Redis-backed rate limiter.
