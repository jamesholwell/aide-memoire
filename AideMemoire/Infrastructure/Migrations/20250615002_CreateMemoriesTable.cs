using FluentMigrator;

namespace AideMemoire.Infrastructure.Migrations;

[Migration(20250615002)]
public class CreateMemoriesTable : Migration {
    public override void Up() {
        Create.Table("Memories")
            .WithColumn("Id").AsInt64().PrimaryKey()
            .WithColumn("RealmId").AsInt64().NotNullable()
            .WithColumn("Key").AsString(255).NotNullable()
            .WithColumn("CreatedAt").AsDateTime().NotNullable()
            .WithColumn("LastUpdatedAt").AsDateTime().NotNullable()
            .WithColumn("Title").AsString(255).NotNullable()
            .WithColumn("Content").AsString().Nullable()
            .WithColumn("Uri").AsString().Nullable()
            .WithColumn("EnclosureUri").AsString().Nullable()
            .WithColumn("ImageUri").AsString().Nullable();

        Create.Index("IX_Memories_RealmId")
            .OnTable("Memories")
            .OnColumn("RealmId");

        Create.Index("IX_Memories_Title")
            .OnTable("Memories")
            .OnColumn("Title");
    }

    public override void Down() {
        Delete.Table("Memories");
    }
}
