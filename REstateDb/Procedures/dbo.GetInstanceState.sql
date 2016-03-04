SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[GetInstanceState]
	@MachineInstanceGuid uniqueidentifier
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    SELECT MachineDefinitionId, StateName FROM MachineInstances WHERE MachineInstanceId = @MachineInstanceGuid
END
GO
