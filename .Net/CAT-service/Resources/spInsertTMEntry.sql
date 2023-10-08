USE [TM Okapi]
GO
/****** Object:  StoredProcedure [dbo].[InsertTMEntry]    Script Date: 12/01/2022 10:18:22 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
--USE [DATABASE_NAME]
/****** Object:  StoredProcedure [dbo].[InsertTMEntry]    Script Date: 07/01/2022 18:53:11 ******/
ALTER PROCEDURE [dbo].[InsertTMEntry]
	@tmTableName NVARCHAR(128), 
	@source nvarchar(4000),
	@sourceHash int,
	@target nvarchar(4000),
	@targetHash int,
	@context varchar(16),
	@user nvarchar(100)
AS
BEGIN
SET NOCOUNT ON;
DECLARE @Sql NVARCHAR(MAX);
SET @Sql = N'
--the table variable. it is faster than temp table for low number of records
declare @exactMatches AS TABLE (entryId int, idSource int, context varchar(16))
insert into @exactMatches (entryId, idSource, context)
Select id, idSource, context  from ' + QUOTENAME(@tmTableName) + ' where sourceHash = @sourceHash and source = @source 

declare @inContextEntryId int = -1 
declare @idSource int = -1
declare @entryId int = -1
declare @newSource bit = 0

Select top(1) @idSource=idSource from @exactMatches
Select @inContextEntryId=entryId from @exactMatches where context=@context
declare @exactMatchesCount int
select @exactMatchesCount = count(*) from @exactMatches

--handle the entry
if (@inContextEntryId > 0 ) begin
	-- update incontext match
	update ' + QUOTENAME(@tmTableName) + ' set target=@target, modifiedBy=@user, dateModified=GETDATE() where id = @inContextEntryId
	set @entryId = @inContextEntryId
end 
else begin
	insert into dbo.' + QUOTENAME(@tmTableName) + ' (source, sourceHash, target, targetHash, context, dateCreated, createdBy, dateModified, modifiedBy) 
	values (@source, @sourceHash, @target, @targetHash, @context, GETDATE(), @user, GETDATE(), @user); 
	set @entryId = @@IDENTITY
	if (@exactMatchesCount > 0) begin
		set @newSource = 0
		--drop the old exact matches. we keep the last three only
		if (@exactMatchesCount >= 5) begin
			delete ' + QUOTENAME(@tmTableName) + ' where id in (Select entryId from @exactMatches order by entryId desc OFFSET 4 ROWS)
		end
	end
	else begin
		--new entry with new source
		set @newSource = 1
		set @idSource = @entryId
	end
	update ' + QUOTENAME(@tmTableName) + ' set idSource=@idSource where id = @entryId
end
Select @entryId as id, @idSource as idSource, @newSource as isNew'
--print(@Sql)
	 EXECUTE sp_executesql @Sql ,N'@source varchar(4000), @sourceHash int, @target varchar(4000), @targetHash int, @context varchar(16), @user nvarchar(100)', 
	 @source=@source, @sourceHash=@sourceHash, @target=@target, @targetHash=@targetHash, @context=@context, @user=@user
END
 