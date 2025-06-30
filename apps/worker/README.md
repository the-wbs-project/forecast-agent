# WeatherGuard Cloudflare Worker

A Cloudflare Worker built with Hono for the WeatherGuard application, providing weather risk analysis and project management APIs.

## Features

- **Authentication**: JWT-based authentication with refresh tokens
- **Project Management**: CRUD operations for construction projects
- **Task Management**: Hierarchical task management with weather sensitivity
- **Weather Integration**: Weather forecasting and risk analysis
- **KV Storage**: Cloudflare KV for metadata and caching

## API Endpoints

### Authentication (`/api/auth`)
- `POST /login` - User login
- `POST /register` - User registration
- `POST /refresh` - Refresh JWT token
- `POST /logout` - User logout
- `POST /change-password` - Change user password
- `POST /reset-password` - Request password reset
- `POST /reset-password/confirm` - Confirm password reset
- `GET /me` - Get current user info

### Projects (`/api/projects`)
- `GET /` - List projects with pagination and search
- `GET /:id` - Get project by ID
- `POST /` - Create new project
- `PUT /:id` - Update project
- `DELETE /:id` - Delete project

### Tasks (`/api/tasks`)
- `GET /project/:projectId` - Get tasks by project
- `GET /schedule/:scheduleId` - Get tasks by schedule
- `GET /:id` - Get task by ID
- `GET /:id/children` - Get child tasks
- `GET /project/:projectId/weather-sensitive` - Get weather sensitive tasks
- `POST /` - Create new task
- `PUT /:id` - Update task
- `DELETE /:id` - Delete task

### Weather (`/api/weather`)
- `GET /forecast` - Get weather forecast
- `GET /analyses/project/:projectId` - Get weather analyses by project
- `GET /analyses/:id` - Get weather analysis by ID
- `GET /analyses/high-risk` - Get high-risk analyses
- `POST /analyses` - Create weather risk analysis
- `POST /analyses/project/:projectId/generate` - Generate analysis for project
- `PUT /analyses/:id` - Update weather analysis
- `DELETE /analyses/:id` - Delete weather analysis

## Development

### Prerequisites

- Node.js 18+
- npm or yarn
- Wrangler CLI

### Setup

1. Install dependencies:
```bash
npm install
```

2. Set up environment variables in `.dev.vars`:
```bash
JWT_SECRET=your-super-secret-jwt-key
OPENWEATHER_API_KEY=your-openweather-api-key
ALLOWED_ORIGINS=http://localhost:3000,http://localhost:5173
```

3. Create KV namespace:
```bash
wrangler kv:namespace create "METADATA_KV"
wrangler kv:namespace create "METADATA_KV" --preview
```

4. Update `wrangler.toml` with your KV namespace IDs.

### Running Locally

```bash
npm run dev
```

The worker will be available at `http://localhost:8787`

### Type Checking

```bash
npm run type-check
```

### Deployment

1. Deploy to Cloudflare:
```bash
npm run deploy
```

2. Set production environment variables:
```bash
wrangler secret put JWT_SECRET
wrangler secret put OPENWEATHER_API_KEY
```

## Architecture

### Folder Structure

```
src/
├── dto/                    # TypeScript interfaces and types
├── data-services/          # KV storage operations
├── http-services/          # External API services and auth
├── routes/                 # Hono route handlers
└── index.ts               # Main application entry point
```

### Key Components

- **DTOs**: TypeScript interfaces for data transfer objects
- **Data Services**: Abstraction layer for Cloudflare KV operations
- **HTTP Services**: External API integrations and authentication logic
- **Routes**: API endpoint handlers using Hono framework

### Weather Integration

The worker supports multiple weather data sources:
- OpenWeatherMap API (with API key)
- Weather.gov API (free, US only)

Weather data is cached in KV storage to reduce API calls and improve performance.

### Authentication

- JWT tokens for API authentication
- Refresh tokens stored in KV
- Password reset functionality
- User management with organization-based access control

## Environment Variables

### Required
- `JWT_SECRET`: Secret key for JWT token signing
- `METADATA_KV`: Cloudflare KV namespace binding

### Optional
- `OPENWEATHER_API_KEY`: API key for OpenWeatherMap (falls back to Weather.gov)
- `ALLOWED_ORIGINS`: Comma-separated list of allowed CORS origins