using System;
using FluentMigrator;

namespace REstate.Database.SqlServer
{
    [Tags("Auth")]
    [Tags("Full")]
    [Migration(2016031703)]
    public class CreateDefaultAdminUsers
        : Migration
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

        public override void Down()
        {
            //Delete.FromTable("Principals")
            //    .Row(new { PrincipalType = "Application", UserOrApplicationName = "AdminUI" });
        }
    }

    [Tags("Auth")]
    [Tags("Full")]
    [Migration(2016031702)]
    public class CreateClaims
        : Migration
    {
        public override void Up()
        {
            Insert.IntoTable("Claims")
                .Row(new
                {
                    ClaimName = "machineBuilder",
                    ClaimDescription = "Allows the user or application to configure new machines and copy existing ones."
                }).Row(new
                {
                    ClaimName = "developer",
                    ClaimDescription = "Allows the user or application to configure code for actions and guards."
                }).Row(new
                {
                    ClaimName = "operator",
                    ClaimDescription = "Allows the user or application to fire trigger and tranisition state on machine instances and create new instances."
                });
        }

        public override void Down()
        {
            Delete.FromTable("Claims")
                .Row(new { ClaimsName = "machineBuilder" })
                .Row(new { ClaimsName = "developer" })
                .Row(new { ClaimsName = "operator" });
        }
    }
}