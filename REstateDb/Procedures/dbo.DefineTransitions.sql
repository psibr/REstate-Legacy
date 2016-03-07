SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- =============================================
CREATE PROCEDURE [dbo].[DefineTransitions]
	-- Add the parameters for the stored procedure here
	@Transitions TransitionTable READONLY
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	INSERT INTO Transitions(MachineDefinitionId, StateName, TriggerName, ResultantStateName, GuardName, IsActive)
	SELECT MachineDefinitionId, StateName, TriggerName, ResultantStateName, GuardName, IsActive FROM @Transitions

	SELECT Transitions.* FROM Transitions
	INNER JOIN @Transitions t on t.MachineDefinitionId = Transitions.MachineDefinitionId 
		AND t.StateName = Transitions.StateName
		AND t.TriggerName = Transitions.TriggerName
END
GO
