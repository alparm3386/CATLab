IF db_id('[dbName]') IS NOT NULL
BEGIN
ALTER DATABASE [[dbName]]
SET SINGLE_USER
--This rolls back all uncommitted transactions in the db.
WITH ROLLBACK IMMEDIATE
END

RESTORE DATABASE [[dbName]]
FROM DISK = '[backupPath]'
WITH MOVE '[dbName]' TO '[mdfPath]', 
MOVE '[logName]' TO '[logPath]'
--FILE = 1,
,REPLACE