-- Create a temporary table to emulate the table variable
CREATE TEMP TABLE exactMatches (entryId INT, idSource INT, context TEXT, speciality INT);

INSERT INTO exactMatches (entryId, idSource, context, speciality)
SELECT id, idSource, context, speciality FROM TM_TABLE WHERE sourceHash = ? AND source = ?;

-- Initialize variables
DECLARE @inContextEntryId INT = -1;
DECLARE @idSource INT = -1;
DECLARE @entryId INT = -1;
DECLARE @newSource BIT = 0;
DECLARE @oldSpecialities TEXT = '';
DECLARE @newSpecialities TEXT = '';

-- Calculating oldSpecialities
SELECT GROUP_CONCAT(speciality, ',') INTO @oldSpecialities FROM exactMatches;

SELECT idSource INTO @idSource FROM exactMatches LIMIT 1;
SELECT entryId INTO @inContextEntryId FROM exactMatches WHERE context = ?;

-- Handle the entry
IF (@inContextEntryId > 0) THEN
    UPDATE TM_TABLE SET target=?, modifiedBy=?, dateModified=?, idTranslation=?, speciality=? WHERE id = @inContextEntryId;
    SET @entryId = @inContextEntryId;
ELSE
    INSERT INTO TM_TABLE (source, target, context, dateCreated, createdBy, dateModified, modifiedBy, speciality, idTranslation, extensionData) 
    VALUES (@source, @sourceHash, @target, @targetHash, @context, @dateCreated, @user, @dateModified, @user, @speciality, @idTranslation, @extensionData);
    
    SET @entryId = last_insert_rowid();

    IF ((SELECT COUNT(*) FROM exactMatches) > 0) THEN
        SET @newSource = 0;
        IF ((SELECT COUNT(*) FROM exactMatches) >= 5) THEN
            DELETE FROM TM_TABLE WHERE id IN (SELECT entryId FROM exactMatches ORDER BY entryId DESC LIMIT -1 OFFSET 4);
        END IF;
    ELSE
        SET @newSource = 1;
        SET @idSource = @entryId;
    END IF;

    UPDATE TM_TABLE SET idSource = @idSource WHERE id = @entryId;
END IF;

-- Calculating newSpecialities
SELECT GROUP_CONCAT(speciality, ',') INTO @newSpecialities FROM TM_TABLE WHERE idSource = @idSource;

-- This is an emulation of the returned result from the stored procedure.
SELECT @entryId AS id, @idSource AS idSource, @newSource AS isNew, @oldSpecialities AS oldSpecialities, @newSpecialities AS newSpecialities;
