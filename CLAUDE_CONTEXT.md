# WeatherGuard AI - Claude Code Context

## Project Overview
I'm building WeatherGuard AI, an AI-powered weather risk management platform for AEC (Architecture, Engineering, Construction) firms. This allows project managers to upload project plans and use AI to identify weather-related risks that could delay projects.

## Business Context
- Target: Mid-market AEC firms ($10M-$500M revenue)
- Problem: Weather delays cost US construction $4B annually
- Solution: AI analyzes project schedules + weather forecasts to predict delays
- MVP: File upload → Weather risk analysis → PDF report

## Technical Stack (Decided)
- **Database**: SQL Server
- **Backend**: ASP.NET Core 8.0 with C#
- **Middleware**: Cloudflare Workers (TypeScript)
- **Frontend**: Angular + TypeScript
- **AI**: Cloudflare AI/Workflows (chosen over n8n)
- **Monorepo**: pnpm + Turbo

## Current Implementation Status
✅ Database schema created (Organizations, Projects, Tasks, WeatherRiskAnalyses)
✅ Monorepo structure with pnpm/Turbo
✅ ASP.NET Core API scaffolded
✅ Schedule parsers planned (using net.sf.mpxj for .mpp and .xer files)
✅ Basic project CRUD controllers
⏳ Need to implement actual parsing logic
⏳ Need to implement weather analysis service
⏳ Need to create Angular frontend
⏳ Need to create Cloudflare Workers

## Key Design Decisions

### MVP Approach
- Start WITHOUT integrations (Procore, Autodesk, etc.)
- Users upload project files directly
- Ad-hoc analysis, not continuous monitoring
- This reduces initial investment from $750K-1.2M to $150-250K

### File Parsing Strategy
```csharp
// Using net.sf.mpxj library for parsing
// Automatically detect weather-sensitive tasks by keywords:
var weatherSensitiveKeywords = new[] {
    "excavat", "concrete", "pour", "paving", "roof",
    "exterior", "foundation", "steel", "crane", "paint",
    "landscap", "grade", "site work", "asphalt"
};
```

### Weather Data Strategy
- Primary: OpenWeatherMap ($49-899/month)
- Backup: Visual Crossing ($0.0001/record)
- Free: NOAA for US data

### AI Architecture
Using Cloudflare AI because:
- Native integration with CF Workers
- Edge performance
- Cost-effective (~$0.01 per 1K requests)
- TypeScript native

## Business Model
- Freemium: 1 free analysis/month
- Professional: $99/month for 10 analyses
- Team: $299/month for 50 analyses
- Enterprise: $999/month unlimited

## Next Implementation Steps

1. **Complete Schedule Parser Service**
   - Implement MSProject parsing
   - Implement P6 XER parsing
   - Add weather sensitivity detection

2. **Create Weather Analysis Service**
   - Integrate weather APIs
   - Build risk scoring algorithm
   - Generate reports

3. **Build Cloudflare Worker**
   - File upload handling
   - Weather data fetching
   - AI analysis orchestration

4. **Create Angular Frontend**
   - File upload interface
   - Results dashboard
   - Report download

## Key Code Patterns

### Entity Structure
```csharp
public class Project {
    public Guid Id { get; set; }
    public Guid OrganizationId { get; set; }
    public string Name { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}
```

### Service Pattern
```csharp
public interface IScheduleParserService {
    Task<ScheduleParseResult> ParseMicrosoftProjectAsync(Stream fileStream);
    Task<ScheduleParseResult> ParseP6XerAsync(Stream fileStream);
}
```

## Important Context
- Weather sensitivity is determined by task names
- All times should be in UTC
- Multi-tenant by OrganizationId
- Audit logging on all changes
- JWT authentication required

## Questions/Decisions Needed
1. How to handle multi-location projects?
2. Weather alert thresholds (rain > X inches?)
3. Cost impact calculation methodology
4. Report template design

## References
- net.sf.mpxj docs: https://www.mpxj.org/
- OpenWeatherMap API: https://openweathermap.org/api
- Cloudflare Workers AI: https://developers.cloudflare.com/workers-ai/