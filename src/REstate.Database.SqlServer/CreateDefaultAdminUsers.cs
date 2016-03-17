using System;
using FluentMigrator;

namespace REstate.Database.SqlServer
{
    [Tags("Auth")]
    [Tags("Full")]
    [Migration(2016031601)]
    public class CreateDefaultAdminUsers
        : Migration
    {
        public override void Up()
        {
            Insert.IntoTable("Principals")
                .Row(new
                {
                    ApiKey = Guid.NewGuid(),
                    PrincipalType = "Application",
                    UserOrApplicationName = "AdminUI"
                });
        }

        public override void Down()
        {
            Delete.FromTable("Principals")
                .Row(new { PrincipalType = "Application", UserOrApplicationName = "AdminUI" });
        }
    }
}