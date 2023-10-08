--CheckExactMatches
CREATE PROCEDURE [dbo].[CheckIncontextMatches]
(
	@tmTableName NVARCHAR(128), 
	@queryTable dbo.ContextMatchQueryTable READONLY
)
AS
BEGIN
SET NOCOUNT ON;
DECLARE @Sql NVARCHAR(MAX);
SET @Sql = N'	Select queryTable.id as id from ' + QUOTENAME(@tmTableName) + ' inner join @queryTable as queryTable on ' + QUOTENAME(@tmTableName) + '.sourceHash = queryTable.sourceHash and
	' + QUOTENAME(@tmTableName) + '.context = queryTable.context and ' + QUOTENAME(@tmTableName) + '.source = queryTable.source'
END
