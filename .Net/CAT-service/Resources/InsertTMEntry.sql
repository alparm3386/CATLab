/****** Object:  StoredProcedure [dbo].[InsertTMEntry]    Script Date: 11/01/2022 23:04:47 ******/
/*SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO*/

--the table variable. it is faster than temp table for low number of records
declare @exactMatches AS TABLE (entryId int, idSource int, context varchar(16))
insert into @exactMatches (entryId, idSource, context)

Select id, idSource, context  from [TM_TABLE] where sourceHash = @sourceHash and source = @source

declare @inContextEntryId int = -1 
declare @idSource int = -1
declare @entryId int = -1
declare @newSource bit = 0

Select top(1) @idSource=idSource from @exactMatches
Select @inContextEntryId=entryId from @exactMatches where context = @context
declare @exactMatchesCount int
select @exactMatchesCount = count(*) from @exactMatches

--handle the entry
if (@inContextEntryId > 0 ) begin
	-- update incontext match
	update [TM_TABLE] set target=@target, modifiedBy=@user, dateModified=GETDATE(), idTranslation=@idTranslation where id = @inContextEntryId
	set @entryId = @inContextEntryId
end 
else begin
	insert into dbo.[TM_TABLE] (source, sourceHash, target, targetHash, context, dateCreated, createdBy, dateModified, modifiedBy, speciality, idTranslation) 
	values (@source, @sourceHash, @target, @targetHash, @context, GETDATE(), @user, GETDATE(), @user, @speciality, @idTranslation); 
	set @entryId = @@IDENTITY
	if (@exactMatchesCount > 0) begin
		set @newSource = 0
		--drop the old exact matches. we keep the last three only
		if (@exactMatchesCount >= 5) begin
			delete [TM_TABLE] where id in (Select entryId from @exactMatches order by entryId desc OFFSET 4 ROWS)
		end
	end
	else begin
		--new entry with new source
		set @newSource = 1
		set @idSource = @entryId
	end
	update [TM_TABLE] set idSource=@idSource where id = @entryId
end
Select @entryId as id, @idSource as idSource, @newSource as isNew
	
 