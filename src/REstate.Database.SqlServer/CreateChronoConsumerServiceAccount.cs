using System;
using FluentMigrator;

namespace REstate.Database.SqlServer
{
    [Tags("Chrono")]
    [Tags("Full")]
    [Migration(2016031701)]
    public class CreateChronoConsumerServiceAccount
        : ForwardOnlyMigration
    {
        public override void Up()
        {
            var apiKey = Guid.NewGuid();

            Insert.IntoTable("Principals")
                .Row(new
                {
                    ApiKey = apiKey,
                    PrincipalType = "Application",
                    UserOrApplicationName = "ChronoConsumer"
                });

            Insert.IntoTable("PrincipalClaims")
                .Row(new { ApiKey = apiKey, ClaimName = "operator" });
        }
    }
}