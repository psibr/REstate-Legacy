SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Loads all configuration info needed for
-- building a state machine.
-- =============================================
CREATE PROCEDURE [dbo].[LoadMachineDefinition]
	@MachineDefinitionId int
AS
BEGIN
	SET NOCOUNT ON;

	SELECT * 
	FROM MachineDefinitions 
	WHERE MachineDefinitionId = @MachineDefinitionId; 

	SELECT s.*
	FROM States s
	WHERE s.MachineDefinitionId = @MachineDefinitionId;

	SELECT * 
	FROM [Triggers] 
	WHERE MachineDefinitionId = @MachineDefinitionId;

	SELECT Transitions.* 
	FROM Transitions 
	INNER JOIN States ON States.MachineDefinitionId = Transitions.MachineDefinitionId 
		AND States.StateName = Transitions.StateName 
	WHERE States.MachineDefinitionId = @MachineDefinitionId;

	SELECT IgnoreRules.* 
	FROM IgnoreRules 
	INNER JOIN States ON States.MachineDefinitionId = IgnoreRules.MachineDefinitionId 
		AND States.StateName = IgnoreRules.StateName 
	WHERE States.MachineDefinitionId= @MachineDefinitionId;

	SELECT DISTINCT g.*
	FROM Guards g 
	INNER JOIN Transitions tra ON tra.GuardName = g.GuardName AND g.MachineDefinitionId = tra.MachineDefinitionId
	INNER JOIN States s ON s.MachineDefinitionId = tra.MachineDefinitionId AND s.StateName = tra.StateName 
	WHERE s.MachineDefinitionId = @MachineDefinitionId; 

	SELECT s.MachineDefinitionId, s.StateName, sa.PurposeName, sa.TriggerName, sa.StateActionDescription, sa.CodeElementId
	FROM States s
	INNER JOIN StateActions sa ON sa.MachineDefinitionId = s.MachineDefinitionId AND sa.StateName = s.StateName
	WHERE s.MachineDefinitionId = @MachineDefinitionId;

	SELECT DISTINCT ce.CodeElementId,ce.CodeTypeId,  ce.CodeElementName, ce.SemanticVersion, ce.CodeElementDescription, ce.CodeBody,
		sqlDb.*, prov.ProviderDescription, prov.ProviderValue
	FROM CodeElements ce
	LEFT JOIN StateActions sa ON sa.CodeElementId = ce.CodeElementId
	LEFT JOIN Guards g ON ce.CodeElementId = g.CodeElementId
	LEFT JOIN Transitions tra ON g.GuardName = tra.GuardName
	INNER JOIN States s ON (sa.MachineDefinitionId = s.MachineDefinitionId AND sa.StateName = s.StateName) 
		OR (tra.MachineDefinitionId = s.MachineDefinitionId AND tra.StateName = s.StateName)
	LEFT JOIN CodeTypeUsages ctu ON ctu.CodeTypeId = ce.CodeTypeId
	LEFT JOIN SqlDatabaseDefinitions sqlDb ON sqlDb.SqlDatabaseDefinitionId = ce.SqlDatabaseDefinitionId
	LEFT JOIN SqlDatabaseProviders prov ON prov.ProviderName = sqlDb.ProviderName
	WHERE s.MachineDefinitionId = @MachineDefinitionId;

END
GO
