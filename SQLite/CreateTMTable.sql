CREATE TABLE "[TM_TABLE]" (
    "id" INTEGER PRIMARY KEY,
    "idSource" INTEGER,
    "source" TEXT,
    "sourceHash" INTEGER,
    "idTarget" INTEGER,
    "target" TEXT,
    "targetHash" INTEGER,
    "metaData" TEXT
);

CREATE INDEX "ix_entries_source_hash" ON "[TM_TABLE]"("sourceHash");
CREATE INDEX "ix_entries_target_hash" ON "[TM_TABLE]"("targetHash");


