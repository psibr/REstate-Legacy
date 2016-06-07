using FluentMigrator;

namespace REstate.Database.SqlServer
{
    [Tags("Auth")]
    [Tags("Full")]
    [Migration(4)]
    public class CreateBaselineAuthTables
        : ForwardOnlyMigration
    {
        public override void Up()
        {
            //Create.Table("Claims")
            //    .WithColumn("ClaimName").AsAnsiString(255).NotNullable().PrimaryKey()
            //    .WithColumn("ClaimDescription").AsAnsiString(500).Nullable();

            Create.Table("Principals")
                .WithColumn("ApiKey").AsGuid().NotNullable().PrimaryKey()
                .WithColumn("PrincipalType").AsAnsiString(50).NotNullable()
                .WithColumn("UserOrApplicationName").AsAnsiString(255).NotNullable()
                .WithColumn("PasswordHash").AsAnsiString(1000).Nullable()
                .WithColumn("IsActive").AsBoolean().WithDefaultValue(true);

            Create.Table("PrincipalClaims")
                .WithColumn("ApiKey").AsGuid().NotNullable().PrimaryKey()
                .WithColumn("ClaimName").AsAnsiString(255).NotNullable().PrimaryKey()
                .WithColumn("IsActive").AsBoolean().WithDefaultValue(true);

            Create.ForeignKey("FK_PrincipalClaims_Principals")
                .FromTable("PrincipalClaims").ForeignColumns("ApiKey")
                .ToTable("Principals").PrimaryColumns("ApiKey");
        }
    }
}