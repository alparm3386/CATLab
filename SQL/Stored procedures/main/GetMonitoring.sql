-- ================================================
-- Template generated from Template Explorer using:
-- Create Procedure (New Menu).SQL
--
-- Use the Specify Values for Template Parameters 
-- command (Ctrl-Shift-M) to fill in the parameter 
-- values below.
--
-- This block of comments will not be included in
-- the definition of the procedure.
-- ================================================
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Alpar
-- Create date: 02/09/2023
-- Description:	GetMonitoring
-- =============================================
CREATE PROCEDURE GetMonitoring 
	-- Add the parameters for the stored procedure here
	@DateFrom as DateTime = '2000-01-01', 
	@DateTo as DateTime= '2040-01-01' 
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

Select * into #orders from Orders where DateCreated >= @DateFrom and DateCreated < @DateTo
--Orders
Select * from #orders

--Jobs
Select Jobs.Id as Id, Jobs.DateProcessed --job
,SourceLanguage, TargetLanguage, Speciality, Speed, Service, Words, Fee --quote
--,Documents. --document
into #jobs from #orders
inner join Jobs on Jobs.OrderId = #orders.Id
inner join Quotes on Quotes.Id = Jobs.QuoteId
--inner join Documents on Documents.id = Jobs.SourceDocumentId
Select * from #jobs

--workflow steps
Select WorkflowSteps.* into #workflowsteps from WorkflowSteps
inner join #jobs on #jobs.Id = WorkflowSteps.JobId
Select * from #workflowsteps


DROP TABLE #orders;
DROP TABLE #jobs;
DROP TABLE #workflowsteps;


END
GO
