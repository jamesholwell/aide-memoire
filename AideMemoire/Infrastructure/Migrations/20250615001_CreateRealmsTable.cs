using FluentMigrator;

namespace AideMemoire.Infrastructure.Migrations;

[Migration(20250615001)]
public class CreateRealmsTable : Migration {
    public override void Up() {
        Create.Table("Realms")
            .WithColumn("Id").AsInt64().PrimaryKey()
            .WithColumn("Key").AsString(255).NotNullable()
            .WithColumn("CreatedAt").AsDateTime().NotNullable()
            .WithColumn("LastUpdatedAt").AsDateTime().NotNullable()
            .WithColumn("Name").AsString(255).NotNullable()
            .WithColumn("Description").AsString().Nullable();

        Create.Index("IX_Realms_Name")
            .OnTable("Realms")
            .OnColumn("Name")
            .Unique();
    }

    public override void Down() {
        Delete.Table("Realms");
    }
}
