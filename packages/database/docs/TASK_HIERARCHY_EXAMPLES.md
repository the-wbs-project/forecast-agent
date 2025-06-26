# Task Hierarchy Examples and Usage

## Overview
The updated Tasks table now supports full multi-level project hierarchies with proper ordering and WBS (Work Breakdown Structure) capabilities.

## New Features Added

### 1. **Hierarchical Structure**
- `ParentTaskId`: Links tasks in parent-child relationships
- `TaskLevel`: Depth in the hierarchy (0 = root level)
- `IsSummaryTask`: Identifies summary/parent tasks
- `TaskType`: Distinguishes between Summary, Task, and Milestone

### 2. **Ordering and Numbering**
- `SortOrder`: Maintains explicit task sequence
- `WBSCode`: Hierarchical numbering (1.2.3 format)
- `OutlineNumber`: Alternative numbering scheme
- Order preserved independent of dates

### 3. **Enhanced Tracking**
- `PercentComplete`: Progress tracking
- `ActualStartDate`/`ActualEndDate`: Actual vs planned dates
- `CriticalPath`: Critical path identification
- `TotalFloat`/`FreeFloat`: Schedule flexibility

### 4. **Dependency Management**
- New `TaskDependencies` table replaces JSON storage
- Supports all dependency types (FS, FF, SF, SS)
- Lag time support

## Example Usage

### Creating a Multi-Level Project Structure
```sql
-- Create a project with hierarchical tasks
DECLARE @ProjectId UNIQUEIDENTIFIER = 'YOUR-PROJECT-ID';
DECLARE @ScheduleId UNIQUEIDENTIFIER = 'YOUR-SCHEDULE-ID';
DECLARE @TaskId UNIQUEIDENTIFIER;

-- Level 0: Project Summary
EXEC sp_InsertTaskWithOrder 
    @ProjectId = @ProjectId,
    @ScheduleId = @ScheduleId,
    @Name = 'Shopping Center Construction',
    @TaskType = 'Summary',
    @TaskId = @TaskId OUTPUT;

DECLARE @ProjectSummaryId UNIQUEIDENTIFIER = @TaskId;

-- Level 1: Phase Summaries
EXEC sp_InsertTaskWithOrder 
    @ProjectId = @ProjectId,
    @ScheduleId = @ScheduleId,
    @Name = 'Site Preparation',
    @ParentTaskId = @ProjectSummaryId,
    @TaskType = 'Summary',
    @TaskId = @TaskId OUTPUT;

DECLARE @SitePrepId UNIQUEIDENTIFIER = @TaskId;

-- Level 2: Work Tasks
EXEC sp_InsertTaskWithOrder 
    @ProjectId = @ProjectId,
    @ScheduleId = @ScheduleId,
    @Name = 'Clear and Grub',
    @ParentTaskId = @SitePrepId,
    @Duration = 5,
    @WeatherSensitive = 1,
    @TaskId = @TaskId OUTPUT;

EXEC sp_InsertTaskWithOrder 
    @ProjectId = @ProjectId,
    @ScheduleId = @ScheduleId,
    @Name = 'Excavation',
    @ParentTaskId = @SitePrepId,
    @Duration = 10,
    @WeatherSensitive = 1,
    @TaskId = @TaskId OUTPUT;
```

### Viewing Task Hierarchy
```sql
-- View tasks with proper indentation and hierarchy
SELECT 
    IndentedName,
    WBSCode,
    TaskType,
    Duration,
    StartDate,
    EndDate,
    WeatherSensitive,
    PercentComplete
FROM vw_TaskHierarchy
WHERE ProjectId = @ProjectId
ORDER BY SortPath;
```

### Finding Weather-Sensitive Tasks by Level
```sql
-- Get all weather-sensitive tasks at the work level (not summaries)
SELECT 
    t.WBSCode,
    t.Name,
    dbo.fn_GetTaskPath(t.Id) AS FullPath,
    t.StartDate,
    t.EndDate,
    t.Duration,
    p.Name AS ParentTask
FROM Tasks t
LEFT JOIN Tasks p ON t.ParentTaskId = p.Id
WHERE t.ProjectId = @ProjectId
    AND t.WeatherSensitive = 1
    AND t.IsSummaryTask = 0
ORDER BY t.SortOrder;
```

### Rolling Up Weather Impact to Summary Levels
```sql
-- Get summary tasks with aggregated weather risk
WITH WeatherRiskRollup AS (
    SELECT 
        ParentTaskId,
        COUNT(*) AS WeatherSensitiveTasks,
        SUM(Duration) AS TotalWeatherSensitiveDays,
        MIN(StartDate) AS EarliestRiskDate,
        MAX(EndDate) AS LatestRiskDate
    FROM Tasks
    WHERE WeatherSensitive = 1 AND IsSummaryTask = 0
    GROUP BY ParentTaskId
)
SELECT 
    t.WBSCode,
    t.Name,
    t.TaskLevel,
    w.WeatherSensitiveTasks,
    w.TotalWeatherSensitiveDays,
    w.EarliestRiskDate,
    w.LatestRiskDate
FROM Tasks t
INNER JOIN WeatherRiskRollup w ON t.Id = w.ParentTaskId
ORDER BY t.SortOrder;
```

### Managing Dependencies
```sql
-- Create a finish-to-start dependency with 2-day lag
INSERT INTO TaskDependencies (PredecessorTaskId, SuccessorTaskId, DependencyType, LagDays)
VALUES (@ExcavationTaskId, @FoundationTaskId, 'FS', 2);

-- Find all tasks dependent on weather-sensitive tasks
SELECT 
    pred.Name AS WeatherSensitiveTask,
    succ.Name AS DependentTask,
    td.DependencyType,
    td.LagDays,
    succ.StartDate AS PlannedStart
FROM TaskDependencies td
INNER JOIN Tasks pred ON td.PredecessorTaskId = pred.Id
INNER JOIN Tasks succ ON td.SuccessorTaskId = succ.Id
WHERE pred.WeatherSensitive = 1
ORDER BY pred.SortOrder, succ.SortOrder;
```

### Maintaining Task Order
```sql
-- Insert a new task after a specific task
DECLARE @NewTaskId UNIQUEIDENTIFIER;
EXEC sp_InsertTaskWithOrder 
    @ProjectId = @ProjectId,
    @ScheduleId = @ScheduleId,
    @Name = 'Soil Testing',
    @InsertAfterTaskId = @ClearGrubTaskId,
    @Duration = 2,
    @WeatherSensitive = 1,
    @TaskId = @NewTaskId OUTPUT;

-- The procedure automatically:
-- 1. Adjusts SortOrder for all subsequent tasks
-- 2. Maintains the same parent as the reference task
-- 3. Regenerates WBS codes
```

### Weather Impact Analysis with Hierarchy
```sql
-- Analyze weather impact by project phase
SELECT 
    phase.Name AS Phase,
    phase.WBSCode,
    COUNT(DISTINCT task.Id) AS TotalTasks,
    SUM(CASE WHEN task.WeatherSensitive = 1 THEN 1 ELSE 0 END) AS WeatherSensitiveTasks,
    SUM(CASE WHEN task.WeatherSensitive = 1 THEN task.Duration ELSE 0 END) AS WeatherRiskDays,
    CAST(SUM(CASE WHEN task.WeatherSensitive = 1 THEN task.Duration ELSE 0 END) * 100.0 / 
         NULLIF(SUM(task.Duration), 0) AS DECIMAL(5,2)) AS WeatherRiskPercentage
FROM Tasks phase
INNER JOIN Tasks task ON task.Id IN (SELECT Id FROM dbo.fn_GetDescendantTasks(phase.Id))
WHERE phase.ProjectId = @ProjectId
    AND phase.TaskLevel = 1  -- Phase level
    AND task.IsSummaryTask = 0  -- Only count work tasks
GROUP BY phase.Name, phase.WBSCode, phase.SortOrder
ORDER BY phase.SortOrder;
```

## Benefits for Weather Forecasting

1. **Precise Impact Assessment**: Weather delays can be tracked at any level of the hierarchy
2. **Cascade Analysis**: Understanding how weather impacts on low-level tasks affect summary milestones
3. **Phase-Based Planning**: Schedule weather-sensitive phases during favorable seasons
4. **Risk Aggregation**: Roll up weather risks from detailed tasks to executive summary level
5. **Dependency Tracking**: Identify knock-on effects of weather delays through the dependency chain