--ExactMatchQueryTable
IF TYPE_ID(N'ContextMatchQueryTable') IS NULL
CREATE TYPE dbo.ContextMatchQueryTable AS TABLE  
    ( 
		id int, 
		source nvarchar(4000),
		sourceHash int,
		context varchar(16)
	);
