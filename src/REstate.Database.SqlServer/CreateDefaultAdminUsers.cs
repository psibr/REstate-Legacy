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
                    ApiKey = Guid.Parse("98EC17D7-7F31-4A44-A911-6B4D10B3DC2E"),
                    PrincipalType = "Application",
                    UserOrApplicationName = "AdminUI"
                });
        }

        public override void Down()
        {
            Delete.FromTable("Principals")
                .Row(new {ApiKey = Guid.Parse("98EC17D7-7F31-4A44-A911-6B4D10B3DC2E")});
        }
    }
}