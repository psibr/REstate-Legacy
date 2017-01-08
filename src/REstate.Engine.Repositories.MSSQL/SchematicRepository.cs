using REstate.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static Susanoo.SusanooCommander;

namespace REstate.Engine.Repositories.MSSQL
{
    public class SchematicRepository
        : ContextualRepository, ISchematicRepository
    {
        private readonly StringSerializer _StringSerializer;

        public SchematicRepository(EngineRepositoryContext root, StringSerializer stringSerializer)
            : base(root)
        {
            _StringSerializer = stringSerializer;
        }

        public async Task<IEnumerable<Schematic>> ListSchematicsAsync(CancellationToken cancellationToken)
        {
            return (await DefineCommand(
                    "SELECT SchematicName, InitialState, CreatedDateTime " +
                    "FROM Schematics;")
                .WithResultsAs<SchematicRecord>()
                .Compile()
                .ExecuteAsync(DatabaseManagerPool.DatabaseManager, cancellationToken).ConfigureAwait(false))
                .Select(record => new Schematic { SchematicName = record.SchematicName, InitialState = record.InitialState });
        }

        public async Task<Schematic> RetrieveSchematicAsync(string schematicName, CancellationToken cancellationToken)
        {
            var schematicRecord = (await DefineCommand(
                    "SELECT * FROM Schematics WHERE SchematicName = @SchematicName;")
                .WithResultsAs<SchematicRecord>()
                .Compile()
                .ExecuteAsync(DatabaseManagerPool.DatabaseManager, new { SchematicName = schematicName }, cancellationToken)
                    .ConfigureAwait(false))
                .Single();

            var schematic = _StringSerializer.Deserialize<Schematic>(schematicRecord.Schematic);

            return schematic;
        }

        public async Task<Schematic> RetrieveSchematicForMachineAsync(string machineId, CancellationToken cancellationToken)
        {
            var schematicRecord = (await DefineCommand(
                    "SELECT TOP 1 Schematics.* " +
                    "FROM Schematics " +
                    "INNER JOIN Machines ON Machines.SchematicName = Schematics.SchematicName " +
                    "WHERE MachineId = @MachineId;")
                .WithResultsAs<SchematicRecord>()
                .Compile()
                .ExecuteAsync(DatabaseManagerPool.DatabaseManager, new { MachineId = machineId }, cancellationToken)
                    .ConfigureAwait(false))
                .Single();

            var schematic = _StringSerializer.Deserialize<Schematic>(schematicRecord.Schematic);

            return schematic;
        }

        public async Task<Schematic> CreateSchematicAsync(Schematic schematic, string forkedFrom, CancellationToken cancellationToken)
        {
            var definition = _StringSerializer.Serialize(schematic);

            var results = await DefineCommand(
                    "INSERT INTO Schematics (SchematicName, ForkedFrom, InitialState, Schematic) " +
                    "VALUES (@SchematicName, @ForkedFrom, @InitialState, @Schematic);" +
                    "\r\n\r\n" +
                    "SELECT * FROM Schematics WHERE SchematicName = @SchematicName")
                .SendNullValues()
                .WithResultsAs<SchematicRecord>()
                .Compile()
                .ExecuteAsync(DatabaseManagerPool.DatabaseManager, new
                {
                    schematic.SchematicName,
                    ForkedFrom = forkedFrom,
                    schematic.InitialState,
                    Schematic = definition
                }, cancellationToken).ConfigureAwait(false);

            var schematicRecord = results.Single();

            var schematicResult = _StringSerializer.Deserialize<Schematic>(schematicRecord.Schematic);

            return schematicResult;
        }

        public Task<Schematic> CreateSchematicAsync(Schematic schematic, CancellationToken cancellationToken)
        {
            return CreateSchematicAsync(schematic, null, cancellationToken);
        }
    }
}
