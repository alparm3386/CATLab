--CheckExactMatches
Select queryTable.id as id from [[TM_TABLE]] inner join @queryTable as queryTable on [[TM_TABLE]].sourceHash = queryTable.sourceHash and
	[[TM_TABLE]].context = queryTable.context and [[TM_TABLE]].source = queryTable.source
	 