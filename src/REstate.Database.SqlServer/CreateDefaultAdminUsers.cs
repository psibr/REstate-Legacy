using System;
using FluentMigrator;

namespace REstate.Database.SqlServer
{
    [Tags("Auth")]
    [Tags("Full")]
    [Migration(2016031703)]
    public class CreateDefaultAdminUsers
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
                    UserOrApplicationName = "AdminUI"
                });

            Insert.IntoTable("PrincipalClaims")
                .Row(new { ApiKey = apiKey, ClaimName = "machineBuilder"})
                .Row(new { ApiKey = apiKey, ClaimName = "developer" })
                .Row(new { ApiKey = apiKey, ClaimName = "operator" });
        }
    }
}