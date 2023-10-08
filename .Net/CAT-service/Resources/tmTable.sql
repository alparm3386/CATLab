CREATE TABLE [dbo]."[TM_TABLE]"(
	[id] [int] IDENTITY(1,1) NOT NULL,
	[idSource] [int] NULL,
	[source] [nvarchar](4000) NULL,
	[sourceHash] int NULL,
	[idTarget] [int] NULL,
	[target] [nvarchar](4000) NULL,
	[targetHash] int NULL,
	[context] [varchar](16) NULL,
	[dateCreated] [datetime] NULL,
	[createdBy] [nvarchar](100) NULL,
	[dateModified] [datetime] NULL,
	[modifiedBy] [nvarchar](100) NULL,
	[speciality] int NULL,
	[idTranslation] int NULL
 CONSTRAINT [[PUBLIC_KEY_NAME]] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

CREATE INDEX ix_entries_source_hash
ON "[TM_TABLE]"(sourceHash);

CREATE INDEX ix_entries_target_hash
ON "[TM_TABLE]"(targetHash);

