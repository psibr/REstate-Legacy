SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- =============================================
CREATE PROCEDURE dbo.DefineStates
	-- Add the parameters for the stored procedure here
	@States StateTable READONLY
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	INSERT INTO States(MachineDefinitionId, StateName, ParentStateName, StateDescription)
	SELECT MachineDefinitionId, StateName, ParentStateName, StateDescription FROM @States

	SELECT States.* FROM States
	INNER JOIN @States s on s.MachineDefinitionId = States.MachineDefinitionId AND s.StateName = States.StateName
END
GO
