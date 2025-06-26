# WeatherGuard AI

AI-powered weather risk management platform for AEC (Architecture, Engineering, Construction) firms.

## Tech Stack

- **Database**: SQL Server
- **Backend**: ASP.NET Core 8.0 with C#
- **Middleware**: Cloudflare Workers (TypeScript)
- **Frontend**: Angular 17+ (TypeScript)
- **AI**: Cloudflare AI
- **Monorepo**: pnpm + Turbo

## Getting Started

### Prerequisites

- Node.js 18+
- .NET 8.0 SDK
- SQL Server (local or remote)
- pnpm (`npm install -g pnpm`)

### Installation

1. Install dependencies:
```bash
pnpm install
```

2. Set up the database:
```bash
pnpm db:migrate
pnpm db:seed
```

3. Configure environment variables:
```bash
cp apps/api/.env.example apps/api/.env
cp apps/workers/.env.example apps/workers/.env
```

4. Run the development servers:
```bash
pnpm dev
```
