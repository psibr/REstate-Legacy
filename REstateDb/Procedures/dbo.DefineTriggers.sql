SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- =============================================
CREATE PROCEDURE [dbo].[DefineTriggers]
	-- Add the parameters for the stored procedure here
	@Triggers TriggerTable READONLY
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	INSERT INTO [Triggers](MachineDefinitionId, TriggerName, TriggerDescription, IsActive)
	SELECT MachineDefinitionId, TriggerName, TriggerDescription, IsActive FROM @Triggers

	SELECT [Triggers].* FROM [Triggers]
	INNER JOIN @Triggers t on t.MachineDefinitionId = [Triggers].MachineDefinitionId AND t.TriggerName = [Triggers].TriggerName
END
GO
