-- Handle the entry

DELETE FROM [TM_TABLE] WHERE sourceHash = :sourceHash and source = :source;
INSERT INTO [TM_TABLE] (source, sourceHash, target, targetHash, context, speciality, metadata) 
VALUES (:source, :sourceHash, :target, :targetHash, :context, :speciality, :metadata); 
update [TM_TABLE] set idSource=last_insert_rowid() where id = last_insert_rowid();
--SELECT GROUP_CONCAT(speciality, ',') as newSpecialities FROM TM_TABLE WHERE idSource = last_insert_rowid();
Select last_insert_rowid() AS id, last_insert_rowid() AS sourceId, true AS isNew, '' AS oldSpecialities, '' AS newSpecialities;