#!/bin/bash

# WeatherGuard AI Complete Project Setup Script
# Save this as setup-weatherguard.sh and run: bash setup-weatherguard.sh

set -e

echo "🚀 Setting up WeatherGuard AI project..."

# Create root directory
PROJECT_NAME="weatherguard-ai"
if [ -d "$PROJECT_NAME" ]; then
    echo "❌ Directory $PROJECT_NAME already exists. Please remove it or choose a different location."
    exit 1
fi

mkdir -p $PROJECT_NAME
cd $PROJECT_NAME

# Initialize git
git init

# Create directory structure
echo "📁 Creating directory structure..."
mkdir -p apps/api/{Controllers,Core/{Entities,Interfaces,Models},Infrastructure/{Data,Services},Models,Tests/{Controllers,Services,Integration},Properties}
mkdir -p apps/web
mkdir -p apps/workers
mkdir -p packages/database/{schema,stored-procedures,scripts}
mkdir -p packages/shared
mkdir -p packages/config

# Create root configuration files
echo "📝 Creating root configuration files..."

# package.json
cat > package.json << 'EOF'
{
  "name": "weatherguard-ai",
  "version": "1.0.0",
  "private": true,
  "packageManager": "pnpm@8.15.0",
  "scripts": {
    "dev": "turbo run dev",
    "build": "turbo run build",
    "test": "turbo run test",
    "lint": "turbo run lint",
    "clean": "turbo run clean",
    "db:migrate": "pnpm --filter database migrate",
    "db:seed": "pnpm --filter database seed"
  },
  "devDependencies": {
    "turbo": "^1.11.3",
    "@types/node": "^20.11.0"
  },
  "engines": {
    "node": ">=18.0.0",
    "pnpm": ">=8.0.0"
  }
}
EOF

# pnpm-workspace.yaml
cat > pnpm-workspace.yaml << 'EOF'
packages:
  - 'apps/*'
  - 'packages/*'
EOF

# turbo.json
cat > turbo.json << 'EOF'
{
  "$schema": "https://turbo.build/schema.json",
  "pipeline": {
    "build": {
      "dependsOn": ["^build"],
      "outputs": ["dist/**", ".next/**", "bin/**", "obj/**"]
    },
    "test": {
      "dependsOn": ["build"],
      "outputs": ["coverage/**"]
    },
    "lint": {},
    "dev": {
      "cache": false,
      "persistent": true
    },
    "clean": {
      "cache": false
    }
  }
}
EOF

# .gitignore
cat > .gitignore << 'EOF'
# Dependencies
node_modules/
.pnpm-store/

# Build outputs
dist/
bin/
obj/
*.dll
*.exe
*.pdb

# IDE
.vs/
.vscode/
.idea/
*.user
*.suo

# Environment
.env
.env.local
.env.*.local

# Logs
*.log
logs/

# OS
.DS_Store
Thumbs.db

# Test coverage
coverage/
.nyc_output/

# Turbo
.turbo/
EOF

# README.md
cat > README.md << 'EOF'
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
EOF

# Create Database Project Files
echo "🗄️ Creating database project..."

# Database package.json
cat > packages/database/package.json << 'EOF'
{
  "name": "@weatherguard/database",
  "version": "1.0.0",
  "private": true,
  "scripts": {
    "migrate": "node scripts/migrate.js",
    "seed": "node scripts/seed.js",
    "generate-types": "node scripts/generate-types.js"
  }
}
EOF

# Initial schema
cat > packages/database/schema/001_initial_schema.sql << 'EOF'
-- Create database
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'WeatherGuardDB')
BEGIN
    CREATE DATABASE WeatherGuardDB;
END
GO

USE WeatherGuardDB;
GO

-- Organizations table
CREATE TABLE Organizations (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    Name NVARCHAR(255) NOT NULL,
    Subdomain NVARCHAR(100) UNIQUE NOT NULL,
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 DEFAULT GETUTCDATE(),
    IsActive BIT DEFAULT 1
);

-- Users table
CREATE TABLE Users (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    OrganizationId UNIQUEIDENTIFIER NOT NULL,
    Email NVARCHAR(255) UNIQUE NOT NULL,
    FirstName NVARCHAR(100),
    LastName NVARCHAR(100),
    PasswordHash NVARCHAR(500),
    Role NVARCHAR(50) NOT NULL DEFAULT 'User',
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 DEFAULT GETUTCDATE(),
    LastLoginAt DATETIME2,
    IsActive BIT DEFAULT 1,
    FOREIGN KEY (OrganizationId) REFERENCES Organizations(Id)
);

-- Projects table
CREATE TABLE Projects (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    OrganizationId UNIQUEIDENTIFIER NOT NULL,
    Name NVARCHAR(255) NOT NULL,
    Description NVARCHAR(MAX),
    Location NVARCHAR(500),
    Latitude DECIMAL(10, 8),
    Longitude DECIMAL(11, 8),
    StartDate DATE,
    EndDate DATE,
    CreatedBy UNIQUEIDENTIFIER NOT NULL,
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 DEFAULT GETUTCDATE(),
    Status NVARCHAR(50) DEFAULT 'Active',
    FOREIGN KEY (OrganizationId) REFERENCES Organizations(Id),
    FOREIGN KEY (CreatedBy) REFERENCES Users(Id)
);

-- Project schedules table
CREATE TABLE ProjectSchedules (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    ProjectId UNIQUEIDENTIFIER NOT NULL,
    FileName NVARCHAR(255),
    FileType NVARCHAR(50),
    FileContent VARBINARY(MAX),
    ParsedData NVARCHAR(MAX),
    UploadedBy UNIQUEIDENTIFIER NOT NULL,
    UploadedAt DATETIME2 DEFAULT GETUTCDATE(),
    ProcessedAt DATETIME2,
    Status NVARCHAR(50) DEFAULT 'Pending',
    ErrorMessage NVARCHAR(MAX),
    FOREIGN KEY (ProjectId) REFERENCES Projects(Id),
    FOREIGN KEY (UploadedBy) REFERENCES Users(Id)
);

-- Tasks table
CREATE TABLE Tasks (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    ProjectId UNIQUEIDENTIFIER NOT NULL,
    ScheduleId UNIQUEIDENTIFIER NOT NULL,
    ExternalId NVARCHAR(255),
    Name NVARCHAR(500) NOT NULL,
    Description NVARCHAR(MAX),
    StartDate DATETIME2,
    EndDate DATETIME2,
    Duration INT,
    PredecessorIds NVARCHAR(MAX),
    WeatherSensitive BIT DEFAULT 0,
    WeatherCategories NVARCHAR(MAX),
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 DEFAULT GETUTCDATE(),
    FOREIGN KEY (ProjectId) REFERENCES Projects(Id),
    FOREIGN KEY (ScheduleId) REFERENCES ProjectSchedules(Id)
);

-- Weather risk analyses table
CREATE TABLE WeatherRiskAnalyses (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    ProjectId UNIQUEIDENTIFIER NOT NULL,
    ScheduleId UNIQUEIDENTIFIER NOT NULL,
    AnalysisDate DATETIME2 DEFAULT GETUTCDATE(),
    WeatherDataSource NVARCHAR(100),
    RiskScore DECIMAL(5, 2),
    TotalDelayDays INT,
    TotalCostImpact DECIMAL(18, 2),
    AnalysisResults NVARCHAR(MAX),
    GeneratedBy UNIQUEIDENTIFIER NOT NULL,
    FOREIGN KEY (ProjectId) REFERENCES Projects(Id),
    FOREIGN KEY (ScheduleId) REFERENCES ProjectSchedules(Id),
    FOREIGN KEY (GeneratedBy) REFERENCES Users(Id)
);

-- Task risk details table
CREATE TABLE TaskRiskDetails (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    AnalysisId UNIQUEIDENTIFIER NOT NULL,
    TaskId UNIQUEIDENTIFIER NOT NULL,
    RiskType NVARCHAR(100),
    Probability DECIMAL(5, 2),
    ImpactDays INT,
    ImpactCost DECIMAL(18, 2),
    MitigationSuggestions NVARCHAR(MAX),
    WeatherForecast NVARCHAR(MAX),
    FOREIGN KEY (AnalysisId) REFERENCES WeatherRiskAnalyses(Id),
    FOREIGN KEY (TaskId) REFERENCES Tasks(Id)
);

-- Audit log table
CREATE TABLE AuditLogs (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    UserId UNIQUEIDENTIFIER,
    Action NVARCHAR(100) NOT NULL,
    EntityType NVARCHAR(100),
    EntityId UNIQUEIDENTIFIER,
    OldValues NVARCHAR(MAX),
    NewValues NVARCHAR(MAX),
    IpAddress NVARCHAR(45),
    UserAgent NVARCHAR(500),
    Timestamp DATETIME2 DEFAULT GETUTCDATE(),
    FOREIGN KEY (UserId) REFERENCES Users(Id)
);

-- Indexes
CREATE INDEX IX_Users_OrganizationId ON Users(OrganizationId);
CREATE INDEX IX_Projects_OrganizationId ON Projects(OrganizationId);
CREATE INDEX IX_Tasks_ProjectId ON Tasks(ProjectId);
CREATE INDEX IX_Tasks_ScheduleId ON Tasks(ScheduleId);
CREATE INDEX IX_WeatherRiskAnalyses_ProjectId ON WeatherRiskAnalyses(ProjectId);
CREATE INDEX IX_AuditLogs_UserId_Timestamp ON AuditLogs(UserId, Timestamp);
EOF

# Create stored procedures
cat > packages/database/stored-procedures/sp_CreateProject.sql << 'EOF'
CREATE OR ALTER PROCEDURE sp_CreateProject
    @OrganizationId UNIQUEIDENTIFIER,
    @Name NVARCHAR(255),
    @Description NVARCHAR(MAX) = NULL,
    @Location NVARCHAR(500) = NULL,
    @Latitude DECIMAL(10, 8) = NULL,
    @Longitude DECIMAL(11, 8) = NULL,
    @StartDate DATE = NULL,
    @EndDate DATE = NULL,
    @CreatedBy UNIQUEIDENTIFIER,
    @ProjectId UNIQUEIDENTIFIER OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    SET @ProjectId = NEWID();
    
    INSERT INTO Projects (
        Id, OrganizationId, Name, Description, Location,
        Latitude, Longitude, StartDate, EndDate, CreatedBy
    )
    VALUES (
        @ProjectId, @OrganizationId, @Name, @Description, @Location,
        @Latitude, @Longitude, @StartDate, @EndDate, @CreatedBy
    );
    
    INSERT INTO AuditLogs (UserId, Action, EntityType, EntityId)
    VALUES (@CreatedBy, 'Create', 'Project', @ProjectId);
END
GO
EOF

# Create ASP.NET Core API
echo "🔧 Creating ASP.NET Core API..."

# Create solution file
cat > WeatherGuard.sln << 'EOF'
Microsoft Visual Studio Solution File, Format Version 12.00
# Visual Studio Version 17
VisualStudioVersion = 17.0.31903.59
MinimumVisualStudioVersion = 10.0.40219.1
Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "WeatherGuard.Api", "apps\api\WeatherGuard.Api.csproj", "{API-GUID}"
EndProject
Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "WeatherGuard.Api.Tests", "apps\api\Tests\WeatherGuard.Api.Tests.csproj", "{TEST-GUID}"
EndProject
Global
	GlobalSection(SolutionConfigurationPlatforms) = preSolution
		Debug|Any CPU = Debug|Any CPU
		Release|Any CPU = Release|Any CPU
	EndGlobalSection
EndGlobal
EOF

# API Project file
cat > apps/api/WeatherGuard.Api.csproj << 'EOF'
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.AspNetCore.OpenApi" Version="8.0.0" />
    <PackageReference Include="Swashbuckle.AspNetCore" Version="6.5.0" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="8.0.0" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="8.0.0" />
    <PackageReference Include="AutoMapper.Extensions.Microsoft.DependencyInjection" Version="12.0.1" />
    <PackageReference Include="FluentValidation.AspNetCore" Version="11.3.0" />
    <PackageReference Include="Serilog.AspNetCore" Version="8.0.0" />
    <PackageReference Include="net.sf.mpxj" Version="12.9.0" />
    <PackageReference Include="net.sf.mpxj-for-csharp" Version="12.9.0" />
    <PackageReference Include="Newtonsoft.Json" Version="13.0.3" />
    <PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="8.0.0" />
  </ItemGroup>
</Project>
EOF

# Program.cs
cat > apps/api/Program.cs << 'EOF'
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/weatherguard-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();

public partial class Program { }
EOF

# Create appsettings.json
cat > apps/api/appsettings.json << 'EOF'
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=WeatherGuardDB;Trusted_Connection=True;MultipleActiveResultSets=true"
  },
  "Cors": {
    "AllowedOrigins": [
      "http://localhost:4200",
      "https://*.workers.dev"
    ]
  },
  "Authentication": {
    "Jwt": {
      "Key": "your-secret-key-here-make-it-long-and-secure",
      "Issuer": "WeatherGuardApi",
      "Audience": "WeatherGuardClient",
      "ExpirationDays": 7
    }
  },
  "WeatherApi": {
    "Provider": "OpenWeatherMap",
    "ApiKey": "your-api-key-here",
    "BaseUrl": "https://api.openweathermap.org/data/2.5/"
  },
  "AllowedHosts": "*"
}
EOF

# Create test project
cat > apps/api/Tests/WeatherGuard.Api.Tests.csproj << 'EOF'
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <IsPackable>false</IsPackable>
    <Nullable>enable</Nullable>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.8.0" />
    <PackageReference Include="xunit" Version="2.6.5" />
    <PackageReference Include="xunit.runner.visualstudio" Version="2.5.6" />
    <PackageReference Include="Moq" Version="4.20.70" />
    <PackageReference Include="FluentAssertions" Version="6.12.0" />
    <PackageReference Include="Microsoft.AspNetCore.Mvc.Testing" Version="8.0.0" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.InMemory" Version="8.0.0" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\WeatherGuard.Api.csproj" />
  </ItemGroup>
</Project>
EOF

# Create a sample controller to verify everything works
cat > apps/api/Controllers/HealthController.cs << 'EOF'
using Microsoft.AspNetCore.Mvc;

namespace WeatherGuard.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new { status = "healthy", timestamp = DateTime.UtcNow });
    }
}
EOF

# Create Entity base classes
mkdir -p apps/api/Core/Entities
cat > apps/api/Core/Entities/BaseEntity.cs << 'EOF'
namespace WeatherGuard.Core.Entities;

public abstract class BaseEntity
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
EOF

# Create placeholder files for other apps
echo "📱 Creating placeholder files for other apps..."
echo "{}" > apps/web/package.json
echo "{}" > apps/workers/package.json

# Create .env.example files
cat > apps/api/.env.example << 'EOF'
# Database
ConnectionStrings__DefaultConnection=Server=(localdb)\\mssqllocaldb;Database=WeatherGuardDB;Trusted_Connection=True

# Weather API
WeatherApi__ApiKey=your-openweathermap-api-key

# JWT
Authentication__Jwt__Key=your-super-secret-jwt-key-minimum-32-characters
EOF

echo "✅ Project structure created successfully!"
echo ""
echo "📋 Next steps:"
echo "1. Install pnpm globally: npm install -g pnpm"
echo "2. Install dependencies: pnpm install"
echo "3. Restore .NET packages: cd apps/api && dotnet restore"
echo "4. Update database connection string in appsettings.json"
echo "5. Run database migrations (after setting up SQL Server)"
echo "6. Start development: pnpm dev"
echo ""
echo "🎉 Happy coding!"
