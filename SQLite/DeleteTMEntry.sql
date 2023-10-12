declare @idSource int = -1
SELECT @idSource=idSource FROM [TM_TABLE] where id = @id
delete [TM_TABLE] where id = @Id
Select CASE WHEN count(*) > 0 THEN -1 ELSE @idSource END as luceneID FROM [TM_TABLE] where idSource = @idSource