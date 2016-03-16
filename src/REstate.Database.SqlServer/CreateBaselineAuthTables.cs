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
                .WithColumn("ClaimDescription").AsString(500).Nullable();

            Create.Table("Principals")
                .WithColumn("ApiKey").AsGuid().NotNullable().PrimaryKey()
                .WithColumn("PrincipalType").AsString(50).NotNullable()
                .WithColumn("UserOrApplicationName").AsString(255).NotNullable()
                .WithColumn("PasswordHash").AsString(1000).Nullable()
                .WithColumn("IsActive").AsBoolean().WithDefaultValue(true);

            Create.Table("PrincipalClaims")
                .WithColumn("ApiKey").AsGuid().NotNullable().PrimaryKey()
                .WithColumn("ClaimName").AsString(255).NotNullable().PrimaryKey()
                .WithColumn("IsActive").AsBoolean().WithDefaultValue(true);

            Create.ForeignKey("FK_PrincipalClaims_Principals")
                .FromTable("PrincipalClaims").ForeignColumns("ApiKey")
                .ToTable("Principals").PrimaryColumns("ApiKey");

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