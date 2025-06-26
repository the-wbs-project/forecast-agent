-- WeatherGuard Database Complete Wipe Script
-- WARNING: This script will DROP ALL TABLES, VIEWS, and STORED PROCEDURES
-- Use with extreme caution - this action cannot be undone
-- 
-- Usage: Execute this script against your WeatherGuard database
-- Note: This completely removes all database objects

-- Drop all views first
DECLARE @viewSql NVARCHAR(MAX) = '';
SELECT @viewSql = @viewSql + 'DROP VIEW [' + SCHEMA_NAME(schema_id) + '].[' + name + '];' + CHAR(13)
FROM sys.views
WHERE is_ms_shipped = 0;

IF @viewSql <> ''
BEGIN
    PRINT 'Dropping views...';
    EXEC sp_executesql @viewSql;
    PRINT 'All views dropped.';
END

-- Drop all stored procedures
DECLARE @procSql NVARCHAR(MAX) = '';
SELECT @procSql = @procSql + 'DROP PROCEDURE [' + SCHEMA_NAME(schema_id) + '].[' + name + '];' + CHAR(13)
FROM sys.procedures
WHERE is_ms_shipped = 0;

IF @procSql <> ''
BEGIN
    PRINT 'Dropping stored procedures...';
    EXEC sp_executesql @procSql;
    PRINT 'All stored procedures dropped.';
END

-- Drop all functions
DECLARE @funcSql NVARCHAR(MAX) = '';
SELECT @funcSql = @funcSql + 'DROP FUNCTION [' + SCHEMA_NAME(schema_id) + '].[' + name + '];' + CHAR(13)
FROM sys.objects
WHERE type IN ('FN', 'IF', 'TF') AND is_ms_shipped = 0;

IF @funcSql <> ''
BEGIN
    PRINT 'Dropping functions...';
    EXEC sp_executesql @funcSql;
    PRINT 'All functions dropped.';
END

-- Drop all foreign key constraints first
DECLARE @fkSql NVARCHAR(MAX) = '';
SELECT @fkSql = @fkSql + 'ALTER TABLE [' + SCHEMA_NAME(fk.schema_id) + '].[' + OBJECT_NAME(fk.parent_object_id) + '] DROP CONSTRAINT [' + fk.name + '];' + CHAR(13)
FROM sys.foreign_keys fk;

IF @fkSql <> ''
BEGIN
    PRINT 'Dropping foreign key constraints...';
    EXEC sp_executesql @fkSql;
    PRINT 'All foreign key constraints dropped.';
END

-- Drop all tables
DECLARE @tableSql NVARCHAR(MAX) = '';
SELECT @tableSql = @tableSql + 'DROP TABLE [' + SCHEMA_NAME(schema_id) + '].[' + name + '];' + CHAR(13)
FROM sys.tables
WHERE is_ms_shipped = 0;

IF @tableSql <> ''
BEGIN
    PRINT 'Dropping tables...';
    EXEC sp_executesql @tableSql;
    PRINT 'All tables dropped.';
END

-- Verify database is clean
SELECT 
    'Tables' as ObjectType,
    COUNT(*) as Count
FROM sys.tables
WHERE is_ms_shipped = 0

UNION ALL

SELECT 
    'Views' as ObjectType,
    COUNT(*) as Count
FROM sys.views
WHERE is_ms_shipped = 0

UNION ALL

SELECT 
    'Stored Procedures' as ObjectType,
    COUNT(*) as Count
FROM sys.procedures
WHERE is_ms_shipped = 0

UNION ALL

SELECT 
    'Functions' as ObjectType,
    COUNT(*) as Count
FROM sys.objects
WHERE type IN ('FN', 'IF', 'TF') AND is_ms_shipped = 0;

PRINT 'Database completely wiped!';
PRINT 'All tables, views, stored procedures, and functions have been dropped.';