# WeatherGuard Database Package

This package contains the database schema, migrations, and scripts for the WeatherGuard AI system.

## Structure

```
database/
├── schema/                 # SQL migration files
│   ├── 001_initial_schema.sql
│   ├── 002_bim_compliance.sql
│   └── 003_task_hierarchy_support.sql
├── stored-procedures/      # Stored procedure definitions
│   ├── sp_CreateProject.sql
│   └── sp_BIMProcedures.sql
├── scripts/               # Node.js scripts
│   ├── migrate.js         # Database migration tool
│   ├── seed.js           # Database seeding (TBD)
│   └── generate-types.js # TypeScript type generation (TBD)
└── docs/                 # Documentation
    ├── BIM_TABLES_RATIONALE.md
    └── TASK_HIERARCHY_EXAMPLES.md
```

## Prerequisites

- SQL Server 2019 or later (or Azure SQL Database)
- Node.js 18+
- `sqlcmd` command-line tool installed

### Installing sqlcmd

**Windows:**
- Comes with SQL Server Management Studio
- Or install SQL Server Command Line Utilities separately

**macOS:**
```bash
brew install sqlcmd
```

**Linux:**
Follow Microsoft's documentation for installing mssql-tools on your distribution.

## Configuration

Set the following environment variables or create a `.env` file:

```bash
DB_SERVER=localhost
DB_DATABASE=WeatherGuardDB
DB_USER=sa
DB_PASSWORD=YourStrongPassword123!
DB_TRUST_CERT=true  # Set to false for production
```

## Running Migrations

From the monorepo root:
```bash
pnpm db:migrate
```

Or from this package directory:
```bash
pnpm migrate
```

The migration tool will:
1. Check for pending migrations in the `schema/` directory
2. Apply them in order (based on filename)
3. Track applied migrations in `.applied-migrations.json`
4. Apply all stored procedures from `stored-procedures/`

## Migration Files

Migration files should be named with a numeric prefix for ordering:
- `001_initial_schema.sql`
- `002_bim_compliance.sql`
- `003_task_hierarchy_support.sql`

## Features

### BIM Compliance
The database supports Building Information Modeling (BIM) with:
- IFC entity types and classifications
- Spatial geometry support
- Model versioning
- Clash detection
- Element relationships

### Task Hierarchy
Full support for multi-level project structures:
- Parent-child task relationships
- WBS (Work Breakdown Structure) codes
- Explicit task ordering
- Summary task rollups
- Dependency management

### Weather Risk Analysis
Integration between BIM elements and weather forecasting:
- Element-level weather sensitivity
- Spatial weather exposure analysis
- Risk propagation through dependencies
- Historical weather impact tracking

## Development

### Adding a New Migration

1. Create a new SQL file in `schema/` with the next number:
   ```
   004_your_feature.sql
   ```

2. Add your SQL commands with proper GO statements

3. Run migrations:
   ```bash
   pnpm migrate
   ```

### Adding Stored Procedures

1. Create or update files in `stored-procedures/`
2. Use `CREATE OR ALTER PROCEDURE` for idempotency
3. Run migrations to apply changes

## Troubleshooting

### sqlcmd not found
Install SQL Server command-line tools for your platform

### Connection failed
- Check SQL Server is running
- Verify credentials
- Ensure SQL Server authentication is enabled
- Check firewall settings

### Migration failed
- Check SQL syntax
- Ensure GO statements are properly placed
- Review error messages in console output
- Check `.applied-migrations.json` for status