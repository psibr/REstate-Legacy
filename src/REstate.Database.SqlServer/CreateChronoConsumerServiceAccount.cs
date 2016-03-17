using System;
using FluentMigrator;

namespace REstate.Database.SqlServer
{
    [Tags("Chrono")]
    [Tags("Full")]
    [Migration(2016031701)]
    public class CreateChronoConsumerServiceAccount
        : Migration
    {
        public override void Up()
        {
            Insert.IntoTable("Principals")
                .Row(new
                {
                    ApiKey = Guid.NewGuid(),
                    PrincipalType = "Application",
                    UserOrApplicationName = "ChronoConsumer"
                });
        }

        public override void Down()
        {
            Delete.FromTable("Principals")
                .Row(new { PrincipalType = "Application", UserOrApplicationName = "ChronoConsumer" });
        }
    }
}