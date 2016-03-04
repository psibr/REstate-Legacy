SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE dbo.AddChronoTrigger
	@MachineInstanceId uniqueidentifier,
	@StateName varchar(255),
	@TriggerName varchar(255),
	@Payload varchar(1024),
	@FireAfter DateTime2(7)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	INSERT INTO ChronoTriggers ( ChronoTriggerId, MachineInstanceId, StateName, TriggerName, Payload, FireAfter)
	VALUES(newId(), @MachineInstanceId, @StateName, @TriggerName, @Payload, @FireAfter);
END
GO
