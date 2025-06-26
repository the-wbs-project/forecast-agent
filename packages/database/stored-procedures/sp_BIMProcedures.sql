-- BIM-specific stored procedures for WeatherGuard

-- Procedure to create a new BIM model with proper GUID generation
CREATE OR ALTER PROCEDURE sp_CreateBIMModel
    @ProjectId UNIQUEIDENTIFIER,
    @Name NVARCHAR(255),
    @Description NVARCHAR(MAX) = NULL,
    @AuthoringToolId NVARCHAR(255) = NULL,
    @AuthoringToolVersion NVARCHAR(50) = NULL,
    @ModelPurpose NVARCHAR(100) = 'Design',
    @LevelOfDevelopment NVARCHAR(10) = 'LOD200',
    @CreatedBy UNIQUEIDENTIFIER,
    @ModelId UNIQUEIDENTIFIER OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Generate IFC-compliant GUID (22 character base64)
    DECLARE @GlobalId NVARCHAR(22);
    EXEC sp_GenerateIFCGuid @GlobalId OUTPUT;
    
    -- Create the model
    SET @ModelId = NEWID();
    
    INSERT INTO BIMModels (
        Id, ProjectId, GlobalId, Name, Description,
        AuthoringToolId, AuthoringToolVersion, ModelPurpose,
        LevelOfDevelopment, CreatedBy
    )
    VALUES (
        @ModelId, @ProjectId, @GlobalId, @Name, @Description,
        @AuthoringToolId, @AuthoringToolVersion, @ModelPurpose,
        @LevelOfDevelopment, @CreatedBy
    );
    
    -- Enable BIM for the project if not already enabled
    UPDATE Projects 
    SET BIMEnabled = 1, UpdatedAt = GETUTCDATE()
    WHERE Id = @ProjectId AND BIMEnabled = 0;
END;
GO

-- Procedure to generate IFC-compliant GUID
CREATE OR ALTER PROCEDURE sp_GenerateIFCGuid
    @GlobalId NVARCHAR(22) OUTPUT
AS
BEGIN
    DECLARE @guid UNIQUEIDENTIFIER = NEWID();
    DECLARE @bytes VARBINARY(16) = CAST(@guid AS VARBINARY(16));
    DECLARE @base64 NVARCHAR(24);
    
    -- Convert to base64
    SET @base64 = CAST(N'' AS XML).value('xs:base64Binary(xs:hexBinary(sql:variable("@bytes")))', 'NVARCHAR(24)');
    
    -- IFC uses a modified base64 encoding
    SET @base64 = REPLACE(@base64, '+', '-');
    SET @base64 = REPLACE(@base64, '/', '_');
    
    -- Remove padding and ensure 22 characters
    SET @GlobalId = LEFT(REPLACE(@base64, '=', ''), 22);
END;
GO

-- Procedure to add a BIM element
CREATE OR ALTER PROCEDURE sp_AddBIMElement
    @ModelId UNIQUEIDENTIFIER,
    @EntityType NVARCHAR(100),
    @Name NVARCHAR(255),
    @Description NVARCHAR(MAX) = NULL,
    @Tag NVARCHAR(100) = NULL,
    @ElementType NVARCHAR(255) = NULL,
    @ClassificationCode NVARCHAR(100) = NULL,
    @LevelOfDevelopment NVARCHAR(10) = NULL,
    @ParentElementId UNIQUEIDENTIFIER = NULL,
    @SpatialContainerId UNIQUEIDENTIFIER = NULL,
    @ElementId UNIQUEIDENTIFIER OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Get entity type ID
    DECLARE @EntityTypeId INT;
    SELECT @EntityTypeId = Id FROM IFCEntityTypes WHERE EntityType = @EntityType;
    
    IF @EntityTypeId IS NULL
    BEGIN
        RAISERROR('Invalid IFC entity type', 16, 1);
        RETURN;
    END
    
    -- Generate IFC GUID
    DECLARE @GlobalId NVARCHAR(22);
    EXEC sp_GenerateIFCGuid @GlobalId OUTPUT;
    
    -- Create the element
    SET @ElementId = NEWID();
    
    INSERT INTO BIMElements (
        Id, ModelId, GlobalId, EntityTypeId, Name, Description,
        Tag, ElementType, ClassificationCode, LevelOfDevelopment,
        ParentElementId, SpatialContainerId
    )
    VALUES (
        @ElementId, @ModelId, @GlobalId, @EntityTypeId, @Name, @Description,
        @Tag, @ElementType, @ClassificationCode, @LevelOfDevelopment,
        @ParentElementId, @SpatialContainerId
    );
END;
GO

-- Procedure to create spatial hierarchy (Site -> Building -> Storey -> Space)
CREATE OR ALTER PROCEDURE sp_CreateBIMSpatialHierarchy
    @ModelId UNIQUEIDENTIFIER,
    @SiteName NVARCHAR(255),
    @BuildingName NVARCHAR(255),
    @StoreyNames NVARCHAR(MAX), -- JSON array of storey names
    @CreatedBy UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @SiteId UNIQUEIDENTIFIER;
    DECLARE @BuildingId UNIQUEIDENTIFIER;
    
    -- Create Site
    EXEC sp_AddBIMElement 
        @ModelId = @ModelId,
        @EntityType = 'IfcSite',
        @Name = @SiteName,
        @ElementId = @SiteId OUTPUT;
    
    -- Create Building
    EXEC sp_AddBIMElement 
        @ModelId = @ModelId,
        @EntityType = 'IfcBuilding',
        @Name = @BuildingName,
        @ParentElementId = @SiteId,
        @SpatialContainerId = @SiteId,
        @ElementId = @BuildingId OUTPUT;
    
    -- Create Storeys from JSON
    DECLARE @StoreyTable TABLE (StoreyName NVARCHAR(255), StoreyOrder INT);
    
    INSERT INTO @StoreyTable (StoreyName, StoreyOrder)
    SELECT 
        JSON_VALUE(value, '$') as StoreyName,
        ROW_NUMBER() OVER (ORDER BY [key]) as StoreyOrder
    FROM OPENJSON(@StoreyNames);
    
    DECLARE @StoreyName NVARCHAR(255);
    DECLARE @StoreyId UNIQUEIDENTIFIER;
    DECLARE storey_cursor CURSOR FOR 
        SELECT StoreyName FROM @StoreyTable ORDER BY StoreyOrder;
    
    OPEN storey_cursor;
    FETCH NEXT FROM storey_cursor INTO @StoreyName;
    
    WHILE @@FETCH_STATUS = 0
    BEGIN
        EXEC sp_AddBIMElement 
            @ModelId = @ModelId,
            @EntityType = 'IfcBuildingStorey',
            @Name = @StoreyName,
            @ParentElementId = @BuildingId,
            @SpatialContainerId = @BuildingId,
            @ElementId = @StoreyId OUTPUT;
        
        FETCH NEXT FROM storey_cursor INTO @StoreyName;
    END;
    
    CLOSE storey_cursor;
    DEALLOCATE storey_cursor;
END;
GO

-- Procedure to detect clashes between elements
CREATE OR ALTER PROCEDURE sp_DetectBIMClashes
    @ModelId UNIQUEIDENTIFIER,
    @ClashTolerance DECIMAL(10, 3) = 0.001 -- meters
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Simple clash detection based on bounding boxes
    INSERT INTO BIMClashDetection (
        ModelId, Element1Id, Element2Id, ClashType, 
        Severity, Status, Location, Description
    )
    SELECT 
        @ModelId,
        g1.ElementId,
        g2.ElementId,
        'HardClash',
        CASE 
            WHEN e1.EntityTypeId IN (SELECT Id FROM IFCEntityTypes WHERE Category = 'Structural') 
                AND e2.EntityTypeId IN (SELECT Id FROM IFCEntityTypes WHERE Category = 'Structural')
            THEN 'Critical'
            ELSE 'Major'
        END,
        'New',
        g1.Centroid,
        CONCAT('Clash between ', e1.Name, ' and ', e2.Name)
    FROM BIMGeometry g1
    INNER JOIN BIMGeometry g2 ON g1.ElementId < g2.ElementId
    INNER JOIN BIMElements e1 ON g1.ElementId = e1.Id
    INNER JOIN BIMElements e2 ON g2.ElementId = e2.Id
    WHERE e1.ModelId = @ModelId 
        AND e2.ModelId = @ModelId
        AND g1.BoundingBoxMin.STDistance(g2.BoundingBoxMax) < @ClashTolerance
        AND g1.BoundingBoxMax.STDistance(g2.BoundingBoxMin) < @ClashTolerance
        AND NOT EXISTS (
            SELECT 1 FROM BIMClashDetection cd 
            WHERE cd.ModelId = @ModelId 
                AND cd.Element1Id = g1.ElementId 
                AND cd.Element2Id = g2.ElementId
        );
END;
GO

-- Procedure to link BIM elements with project tasks
CREATE OR ALTER PROCEDURE sp_LinkBIMElementsToTask
    @TaskId UNIQUEIDENTIFIER,
    @ElementIds NVARCHAR(MAX) -- JSON array of element IDs
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Update the task with BIM element references
    UPDATE Tasks
    SET BIMElementIds = @ElementIds,
        UpdatedAt = GETUTCDATE()
    WHERE Id = @TaskId;
    
    -- Log the linkage in audit
    INSERT INTO AuditLogs (UserId, Action, EntityType, EntityId, NewValues)
    VALUES (
        NULL, -- System action
        'LinkBIMElements',
        'Tasks',
        @TaskId,
        @ElementIds
    );
END;
GO

-- Procedure to get BIM elements affected by weather-sensitive tasks
CREATE OR ALTER PROCEDURE sp_GetWeatherAffectedBIMElements
    @ProjectId UNIQUEIDENTIFIER,
    @StartDate DATETIME2,
    @EndDate DATETIME2
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT DISTINCT
        be.Id,
        be.GlobalId,
        be.Name,
        et.EntityType,
        et.Category,
        t.Name as TaskName,
        t.StartDate,
        t.EndDate,
        t.WeatherCategories,
        be.LevelOfDevelopment
    FROM Tasks t
    CROSS APPLY OPENJSON(t.BIMElementIds) WITH (ElementId UNIQUEIDENTIFIER '$')
    INNER JOIN BIMElements be ON be.Id = ElementId
    INNER JOIN IFCEntityTypes et ON be.EntityTypeId = et.Id
    WHERE t.ProjectId = @ProjectId
        AND t.WeatherSensitive = 1
        AND t.StartDate <= @EndDate
        AND t.EndDate >= @StartDate
    ORDER BY t.StartDate, be.Name;
END;
GO

-- Procedure to update BIM model version
CREATE OR ALTER PROCEDURE sp_CreateBIMModelVersion
    @ModelId UNIQUEIDENTIFIER,
    @Description NVARCHAR(MAX),
    @FileReference NVARCHAR(500),
    @FileSize BIGINT,
    @FileHash NVARCHAR(64),
    @CreatedBy UNIQUEIDENTIFIER,
    @VersionTag NVARCHAR(50) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRANSACTION;
    
    -- Get next version number
    DECLARE @VersionNumber INT;
    SELECT @VersionNumber = ISNULL(MAX(VersionNumber), 0) + 1
    FROM BIMModelVersions
    WHERE ModelId = @ModelId;
    
    -- Deactivate current active version
    UPDATE BIMModelVersions
    SET IsActive = 0
    WHERE ModelId = @ModelId AND IsActive = 1;
    
    -- Create new version
    INSERT INTO BIMModelVersions (
        ModelId, VersionNumber, VersionTag, Description,
        FileReference, FileSize, FileHash, CreatedBy, IsActive
    )
    VALUES (
        @ModelId, @VersionNumber, @VersionTag, @Description,
        @FileReference, @FileSize, @FileHash, @CreatedBy, 1
    );
    
    -- Update model version
    UPDATE BIMModels
    SET ModelVersion = @VersionNumber,
        UpdatedAt = GETUTCDATE()
    WHERE Id = @ModelId;
    
    COMMIT TRANSACTION;
END;
GO