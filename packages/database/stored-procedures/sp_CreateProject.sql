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
