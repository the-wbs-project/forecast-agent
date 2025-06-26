-- WeatherGuard Complete Database Schema
-- This is a consolidated schema that includes all features:
-- - Core tables (Firms, Users, Projects, etc.)
-- - BIM compliance features
-- - Task hierarchy and project management features
-- Note: Database should be created beforehand in Azure SQL Database
-- This script assumes you're already connected to the correct database

-- Firms table
CREATE TABLE Firms (
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
    FOREIGN KEY (OrganizationId) REFERENCES Firms(Id)
);

-- Projects table (enhanced with BIM fields)
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
    -- BIM-specific fields
    BIMEnabled BIT DEFAULT 0,
    CoordinateSystemId NVARCHAR(50),
    NorthDirection DECIMAL(5, 2), -- Degrees from true north
    BuildingHeight DECIMAL(10, 2), -- Meters
    GrossFloorArea DECIMAL(18, 2), -- Square meters
    NumberOfStoreys INT,
    FOREIGN KEY (OrganizationId) REFERENCES Firms(Id),
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

-- IFC Entity Types lookup table
CREATE TABLE IFCEntityTypes (
    Id INT PRIMARY KEY IDENTITY(1,1),
    EntityType NVARCHAR(100) NOT NULL UNIQUE,
    ParentType NVARCHAR(100),
    Category NVARCHAR(50), -- Structural, Architectural, MEP, etc.
    Description NVARCHAR(500)
);

-- Insert common IFC entity types
INSERT INTO IFCEntityTypes (EntityType, ParentType, Category, Description) VALUES
('IfcProject', NULL, 'Root', 'Root element of the project'),
('IfcSite', 'IfcSpatialStructureElement', 'Spatial', 'Geographic site'),
('IfcBuilding', 'IfcSpatialStructureElement', 'Spatial', 'Building structure'),
('IfcBuildingStorey', 'IfcSpatialStructureElement', 'Spatial', 'Building level/floor'),
('IfcSpace', 'IfcSpatialStructureElement', 'Spatial', 'Bounded space'),
('IfcWall', 'IfcBuildingElement', 'Architectural', 'Wall element'),
('IfcWallStandardCase', 'IfcWall', 'Architectural', 'Standard wall'),
('IfcSlab', 'IfcBuildingElement', 'Structural', 'Slab/floor element'),
('IfcBeam', 'IfcBuildingElement', 'Structural', 'Beam element'),
('IfcColumn', 'IfcBuildingElement', 'Structural', 'Column element'),
('IfcDoor', 'IfcBuildingElement', 'Architectural', 'Door element'),
('IfcWindow', 'IfcBuildingElement', 'Architectural', 'Window element'),
('IfcRoof', 'IfcBuildingElement', 'Architectural', 'Roof element'),
('IfcStair', 'IfcBuildingElement', 'Architectural', 'Stair element'),
('IfcRailing', 'IfcBuildingElement', 'Architectural', 'Railing element'),
('IfcCurtainWall', 'IfcBuildingElement', 'Architectural', 'Curtain wall system'),
('IfcPlate', 'IfcBuildingElement', 'Structural', 'Plate element'),
('IfcMember', 'IfcBuildingElement', 'Structural', 'Structural member'),
('IfcFooting', 'IfcBuildingElement', 'Structural', 'Foundation element'),
('IfcPile', 'IfcBuildingElement', 'Structural', 'Pile foundation'),
('IfcBuildingElementProxy', 'IfcBuildingElement', 'General', 'Generic building element');

-- BIM Models table
CREATE TABLE BIMModels (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    ProjectId UNIQUEIDENTIFIER NOT NULL,
    GlobalId NVARCHAR(22) NOT NULL UNIQUE, -- IFC GUID format
    Name NVARCHAR(255) NOT NULL,
    Description NVARCHAR(MAX),
    AuthoringToolId NVARCHAR(255),
    AuthoringToolVersion NVARCHAR(50),
    IFCVersion NVARCHAR(20) DEFAULT 'IFC4',
    ModelVersion INT DEFAULT 1,
    ModelPurpose NVARCHAR(100), -- Design, Construction, AsBuilt, etc.
    LevelOfDevelopment NVARCHAR(10), -- LOD100, LOD200, LOD300, LOD400, LOD500
    CreatedBy UNIQUEIDENTIFIER NOT NULL,
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 DEFAULT GETUTCDATE(),
    FileReference NVARCHAR(500),
    FOREIGN KEY (ProjectId) REFERENCES Projects(Id),
    FOREIGN KEY (CreatedBy) REFERENCES Users(Id)
);

-- BIM Elements table
CREATE TABLE BIMElements (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    ModelId UNIQUEIDENTIFIER NOT NULL,
    GlobalId NVARCHAR(22) NOT NULL UNIQUE, -- IFC GUID
    EntityTypeId INT NOT NULL,
    Name NVARCHAR(255),
    Description NVARCHAR(MAX),
    Tag NVARCHAR(100), -- Element tag/mark
    ElementType NVARCHAR(255), -- Specific type name
    MaterialIds NVARCHAR(MAX), -- JSON array of material IDs
    PropertySets NVARCHAR(MAX), -- JSON property sets
    ClassificationCode NVARCHAR(100), -- UniClass, OmniClass, etc.
    LevelOfDevelopment NVARCHAR(10),
    ParentElementId UNIQUEIDENTIFIER, -- For hierarchical relationships
    SpatialContainerId UNIQUEIDENTIFIER, -- Which space/storey contains this
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 DEFAULT GETUTCDATE(),
    FOREIGN KEY (ModelId) REFERENCES BIMModels(Id),
    FOREIGN KEY (EntityTypeId) REFERENCES IFCEntityTypes(Id),
    FOREIGN KEY (ParentElementId) REFERENCES BIMElements(Id),
    FOREIGN KEY (SpatialContainerId) REFERENCES BIMElements(Id)
);

-- Tasks table (complete with hierarchy and project management features)
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
    -- Hierarchy fields
    ParentTaskId UNIQUEIDENTIFIER NULL,
    WBSCode NVARCHAR(50),
    TaskLevel INT DEFAULT 0,
    SortOrder INT,
    TaskType NVARCHAR(50) DEFAULT 'Task', -- 'Summary', 'Task', 'Milestone'
    OutlineNumber NVARCHAR(100), -- '1.2.3' style numbering
    IsSummaryTask BIT DEFAULT 0,
    IsMilestone BIT DEFAULT 0,
    -- Project management fields
    PercentComplete DECIMAL(5,2) DEFAULT 0,
    ActualStartDate DATETIME2,
    ActualEndDate DATETIME2,
    BaselineStartDate DATETIME2,
    BaselineEndDate DATETIME2,
    BaselineDuration INT,
    CriticalPath BIT DEFAULT 0,
    TotalFloat INT DEFAULT 0,
    FreeFloat INT DEFAULT 0,
    -- Cost tracking
    PlannedCost DECIMAL(18,2),
    ActualCost DECIMAL(18,2),
    RemainingCost DECIMAL(18,2),
    -- BIM integration
    BIMElementIds NVARCHAR(MAX), -- JSON array of related BIM element IDs
    LocationElementId UNIQUEIDENTIFIER, -- Primary location (e.g., which floor/space)
    FOREIGN KEY (ProjectId) REFERENCES Projects(Id),
    FOREIGN KEY (ScheduleId) REFERENCES ProjectSchedules(Id),
    FOREIGN KEY (ParentTaskId) REFERENCES Tasks(Id),
    FOREIGN KEY (LocationElementId) REFERENCES BIMElements(Id)
);

-- Task Dependencies table
CREATE TABLE TaskDependencies (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    PredecessorTaskId UNIQUEIDENTIFIER NOT NULL,
    SuccessorTaskId UNIQUEIDENTIFIER NOT NULL,
    DependencyType NVARCHAR(10) DEFAULT 'FS', -- FS, FF, SF, SS
    LagDays INT DEFAULT 0,
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
    FOREIGN KEY (PredecessorTaskId) REFERENCES Tasks(Id),
    FOREIGN KEY (SuccessorTaskId) REFERENCES Tasks(Id),
    CONSTRAINT UQ_TaskDependency UNIQUE (PredecessorTaskId, SuccessorTaskId)
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

-- BIM Geometry table
CREATE TABLE BIMGeometry (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    ElementId UNIQUEIDENTIFIER NOT NULL,
    GeometryType NVARCHAR(50), -- BoundingBox, Mesh, Extrusion, etc.
    CoordinateSystem NVARCHAR(50),
    BoundingBoxMin GEOMETRY, -- SQL Server spatial type
    BoundingBoxMax GEOMETRY,
    Centroid GEOMETRY,
    Volume DECIMAL(18, 3),
    Area DECIMAL(18, 3),
    GeometryData NVARCHAR(MAX), -- JSON or WKT representation
    FOREIGN KEY (ElementId) REFERENCES BIMElements(Id)
);

-- BIM Relations table
CREATE TABLE BIMRelations (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    RelationType NVARCHAR(100), -- IfcRelAggregates, IfcRelContainedInSpatialStructure, etc.
    GlobalId NVARCHAR(22) NOT NULL UNIQUE,
    Name NVARCHAR(255),
    Description NVARCHAR(MAX),
    RelatingElementId UNIQUEIDENTIFIER NOT NULL,
    RelatedElementId UNIQUEIDENTIFIER NOT NULL,
    Properties NVARCHAR(MAX), -- JSON for additional properties
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
    FOREIGN KEY (RelatingElementId) REFERENCES BIMElements(Id),
    FOREIGN KEY (RelatedElementId) REFERENCES BIMElements(Id)
);

-- BIM Clash Detection table
CREATE TABLE BIMClashDetection (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    ModelId UNIQUEIDENTIFIER NOT NULL,
    Element1Id UNIQUEIDENTIFIER NOT NULL,
    Element2Id UNIQUEIDENTIFIER NOT NULL,
    ClashType NVARCHAR(50), -- HardClash, SoftClash, Clearance
    Severity NVARCHAR(20), -- Critical, Major, Minor
    Status NVARCHAR(50), -- New, Assigned, Resolved, Approved
    Location GEOMETRY,
    VolumeIntersection DECIMAL(18, 3),
    Description NVARCHAR(MAX),
    Resolution NVARCHAR(MAX),
    AssignedTo UNIQUEIDENTIFIER,
    DetectedAt DATETIME2 DEFAULT GETUTCDATE(),
    ResolvedAt DATETIME2,
    FOREIGN KEY (ModelId) REFERENCES BIMModels(Id),
    FOREIGN KEY (Element1Id) REFERENCES BIMElements(Id),
    FOREIGN KEY (Element2Id) REFERENCES BIMElements(Id),
    FOREIGN KEY (AssignedTo) REFERENCES Users(Id)
);

-- BIM Model Versions table
CREATE TABLE BIMModelVersions (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    ModelId UNIQUEIDENTIFIER NOT NULL,
    VersionNumber INT NOT NULL,
    VersionTag NVARCHAR(50),
    Description NVARCHAR(MAX),
    FileReference NVARCHAR(500),
    FileSize BIGINT,
    FileHash NVARCHAR(64), -- SHA256 hash
    CreatedBy UNIQUEIDENTIFIER NOT NULL,
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
    IsActive BIT DEFAULT 0,
    FOREIGN KEY (ModelId) REFERENCES BIMModels(Id),
    FOREIGN KEY (CreatedBy) REFERENCES Users(Id)
);

-- BIM Coordination Issues table
CREATE TABLE BIMCoordinationIssues (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    ProjectId UNIQUEIDENTIFIER NOT NULL,
    IssueType NVARCHAR(100), -- RFI, DesignIssue, ConstructionIssue, etc.
    Title NVARCHAR(500) NOT NULL,
    Description NVARCHAR(MAX),
    Priority NVARCHAR(20), -- Critical, High, Medium, Low
    Status NVARCHAR(50), -- Open, InProgress, Resolved, Closed
    ElementIds NVARCHAR(MAX), -- JSON array of related element IDs
    ViewpointData NVARCHAR(MAX), -- BCF viewpoint data
    AssignedTo UNIQUEIDENTIFIER,
    CreatedBy UNIQUEIDENTIFIER NOT NULL,
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
    DueDate DATETIME2,
    ResolvedAt DATETIME2,
    FOREIGN KEY (ProjectId) REFERENCES Projects(Id),
    FOREIGN KEY (AssignedTo) REFERENCES Users(Id),
    FOREIGN KEY (CreatedBy) REFERENCES Users(Id)
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

-- Create all indexes
CREATE INDEX IX_Users_OrganizationId ON Users(OrganizationId);
CREATE INDEX IX_Projects_OrganizationId ON Projects(OrganizationId);
CREATE INDEX IX_Tasks_ProjectId ON Tasks(ProjectId);
CREATE INDEX IX_Tasks_ScheduleId ON Tasks(ScheduleId);
CREATE INDEX IX_Tasks_ParentTaskId ON Tasks(ParentTaskId);
CREATE INDEX IX_Tasks_SortOrder ON Tasks(ProjectId, ScheduleId, SortOrder);
CREATE INDEX IX_Tasks_WBSCode ON Tasks(ProjectId, WBSCode);
CREATE INDEX IX_Tasks_OutlineNumber ON Tasks(ProjectId, OutlineNumber);
CREATE INDEX IX_Tasks_TaskType ON Tasks(TaskType);
CREATE INDEX IX_Tasks_CriticalPath ON Tasks(ProjectId, CriticalPath);
CREATE INDEX IX_TaskDependencies_Predecessor ON TaskDependencies(PredecessorTaskId);
CREATE INDEX IX_TaskDependencies_Successor ON TaskDependencies(SuccessorTaskId);
CREATE INDEX IX_WeatherRiskAnalyses_ProjectId ON WeatherRiskAnalyses(ProjectId);
CREATE INDEX IX_AuditLogs_UserId_Timestamp ON AuditLogs(UserId, Timestamp);
CREATE INDEX IX_BIMModels_ProjectId ON BIMModels(ProjectId);
CREATE INDEX IX_BIMElements_ModelId ON BIMElements(ModelId);
CREATE INDEX IX_BIMElements_EntityTypeId ON BIMElements(EntityTypeId);
CREATE INDEX IX_BIMElements_GlobalId ON BIMElements(GlobalId);
CREATE INDEX IX_BIMGeometry_ElementId ON BIMGeometry(ElementId);
CREATE INDEX IX_BIMRelations_RelatingElementId ON BIMRelations(RelatingElementId);
CREATE INDEX IX_BIMRelations_RelatedElementId ON BIMRelations(RelatedElementId);
CREATE INDEX IX_BIMClashDetection_ModelId_Status ON BIMClashDetection(ModelId, Status);
CREATE INDEX IX_BIMCoordinationIssues_ProjectId_Status ON BIMCoordinationIssues(ProjectId, Status);

-- Add spatial indexes for geometry queries
-- Azure SQL Database requires BOUNDING_BOX specification
CREATE SPATIAL INDEX SIX_BIMGeometry_BoundingBoxMin ON BIMGeometry(BoundingBoxMin)
WITH (BOUNDING_BOX = (-180, -90, 180, 90));

CREATE SPATIAL INDEX SIX_BIMGeometry_Centroid ON BIMGeometry(Centroid)
WITH (BOUNDING_BOX = (-180, -90, 180, 90));

CREATE SPATIAL INDEX SIX_BIMClashDetection_Location ON BIMClashDetection(Location)
WITH (BOUNDING_BOX = (-180, -90, 180, 90));
GO

-- Create views
CREATE OR ALTER VIEW vw_TaskHierarchy AS
WITH TaskHierarchyCTE AS (
    -- Anchor: Root level tasks (no parent)
    SELECT 
        t.Id,
        t.ProjectId,
        t.ScheduleId,
        t.Name,
        t.ParentTaskId,
        t.WBSCode,
        t.OutlineNumber,
        t.TaskLevel,
        t.SortOrder,
        t.TaskType,
        t.StartDate,
        t.EndDate,
        t.Duration,
        t.PercentComplete,
        t.WeatherSensitive,
        t.CriticalPath,
        CAST(t.Name AS NVARCHAR(MAX)) AS HierarchyPath,
        CAST(RIGHT('00000' + CAST(t.SortOrder AS NVARCHAR(5)), 5) AS NVARCHAR(MAX)) AS SortPath
    FROM Tasks t
    WHERE t.ParentTaskId IS NULL
    
    UNION ALL
    
    -- Recursive: Child tasks
    SELECT 
        t.Id,
        t.ProjectId,
        t.ScheduleId,
        t.Name,
        t.ParentTaskId,
        t.WBSCode,
        t.OutlineNumber,
        t.TaskLevel,
        t.SortOrder,
        t.TaskType,
        t.StartDate,
        t.EndDate,
        t.Duration,
        t.PercentComplete,
        t.WeatherSensitive,
        t.CriticalPath,
        CAST(cte.HierarchyPath + ' > ' + t.Name AS NVARCHAR(MAX)) AS HierarchyPath,
        CAST(cte.SortPath + '.' + RIGHT('00000' + CAST(t.SortOrder AS NVARCHAR(5)), 5) AS NVARCHAR(MAX)) AS SortPath
    FROM Tasks t
    INNER JOIN TaskHierarchyCTE cte ON t.ParentTaskId = cte.Id
)
SELECT 
    *,
    REPLICATE('  ', TaskLevel) + Name AS IndentedName
FROM TaskHierarchyCTE;
GO

-- Create stored procedures
CREATE OR ALTER PROCEDURE sp_RecalculateTaskLevels
    @ProjectId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    WITH TaskLevelCTE AS (
        -- Root tasks have level 0
        SELECT Id, 0 AS Level
        FROM Tasks
        WHERE ProjectId = @ProjectId AND ParentTaskId IS NULL
        
        UNION ALL
        
        -- Child tasks have parent level + 1
        SELECT t.Id, cte.Level + 1
        FROM Tasks t
        INNER JOIN TaskLevelCTE cte ON t.ParentTaskId = cte.Id
    )
    UPDATE t
    SET TaskLevel = cte.Level
    FROM Tasks t
    INNER JOIN TaskLevelCTE cte ON t.Id = cte.Id;
END;
GO

-- Create functions
CREATE OR ALTER FUNCTION fn_GetDescendantTasks(@TaskId UNIQUEIDENTIFIER)
RETURNS TABLE
AS
RETURN
(
    WITH DescendantsCTE AS (
        SELECT Id
        FROM Tasks
        WHERE Id = @TaskId
        
        UNION ALL
        
        SELECT t.Id
        FROM Tasks t
        INNER JOIN DescendantsCTE d ON t.ParentTaskId = d.Id
    )
    SELECT Id FROM DescendantsCTE
);
GO

CREATE OR ALTER FUNCTION fn_GetTaskPath(@TaskId UNIQUEIDENTIFIER)
RETURNS NVARCHAR(MAX)
AS
BEGIN
    DECLARE @Path NVARCHAR(MAX) = '';
    
    WITH PathCTE AS (
        SELECT Id, Name, ParentTaskId, CAST(Name AS NVARCHAR(MAX)) AS Path
        FROM Tasks
        WHERE Id = @TaskId
        
        UNION ALL
        
        SELECT t.Id, t.Name, t.ParentTaskId, CAST(t.Name + ' > ' + p.Path AS NVARCHAR(MAX))
        FROM Tasks t
        INNER JOIN PathCTE p ON t.Id = p.ParentTaskId
    )
    SELECT TOP 1 @Path = Path
    FROM PathCTE
    WHERE ParentTaskId IS NULL;
    
    RETURN @Path;
END;
GO

PRINT 'WeatherGuard complete schema created successfully!';