CREATE TABLE "[TM_TABLE]" (
    "id" INTEGER PRIMARY KEY,
    "ourceId" INTEGER,
    "source" TEXT,
    "sourceHash" INTEGER,
    "target" TEXT,
    "targetHash" INTEGER,
    "context" TEXT,
    "speciality" INTEGER,
    "metadata" TEXT
);

CREATE INDEX "ix_entries_source_hash" ON "[TM_TABLE]"("sourceHash");
CREATE INDEX "ix_entries_target_hash" ON "[TM_TABLE]"("targetHash");


