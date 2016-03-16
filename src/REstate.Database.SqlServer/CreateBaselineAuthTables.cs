using FluentMigrator;

namespace REstate.Database.SqlServer
{
    [Tags("Auth")]
    [Tags("Full")]
    [Migration(4)]
    public class CreateBaselineAuthTables
        : Migration
    {
        public override void Up()
        {
            Create.Table("Claims")
                .WithColumn("ClaimName").AsString(255).NotNullable().PrimaryKey()
                .WithColumn("ClaimDescription").AsString(500);

            Create.Table("Principals")
                .WithColumn("PrincipalId").AsInt32().NotNullable().PrimaryKey()
                .WithColumn("ApiKey").AsGuid().NotNullable()
                .WithColumn("PrincipalType").AsString(50).NotNullable()
                .WithColumn("UserOrApplicationName").AsString(255).NotNullable()
                .WithColumn("PasswordHash").AsString(1000)
                .WithColumn("IsActive").AsBoolean().WithDefaultValue(true);

            Create.Table("PrincipalClaims")
                .WithColumn("PrincipalId").AsInt32().NotNullable().PrimaryKey()
                .WithColumn("ClaimName").AsString(255).NotNullable().PrimaryKey()
                .WithColumn("IsActive").AsBoolean().WithDefaultValue(true);

            Create.ForeignKey("FK_PrincipalClaims_Principals")
                .FromTable("PrincipalClaims").ForeignColumns("PrincipalId")
                .ToTable("Principals").PrimaryColumns("PrincipalId");

            Create.ForeignKey("FK_PrincipalClaims_Claims")
                .FromTable("PrincipalClaims").ForeignColumns("ClaimName")
                .ToTable("Claims").PrimaryColumns("ClaimName");
        }

        public override void Down()
        {
            Delete.ForeignKey("FK_PrincipalClaims_Claims")
                .OnTable("PrincipalClaims");

            Delete.ForeignKey("FK_PrincipalClaims_Principals")
                .OnTable("PrincipalClaims");

            Delete.Table("PrincipalClaims");
            Delete.Table("Principals");
            Delete.Table("Claims");
        }
    }
}