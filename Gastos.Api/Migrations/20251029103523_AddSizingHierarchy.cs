using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gastos.Api.Migrations;

/// <inheritdoc />
public partial class AddSizingHierarchy : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<int>(
            name: "parent_id",
            schema: "rc",
            table: "sizings",
            type: "integer",
            nullable: true);

        migrationBuilder.AddColumn<decimal>(
            name: "proportion",
            schema: "rc",
            table: "sizings",
            type: "numeric(18,6)",
            precision: 18,
            scale: 6,
            nullable: true);

        migrationBuilder.UpdateData(
            schema: "rc",
            table: "sizings",
            keyColumn: "id",
            keyValue: 1,
            columns: new[] { "parent_id", "proportion" },
            values: new object[] { 3, 1000m });

        migrationBuilder.UpdateData(
            schema: "rc",
            table: "sizings",
            keyColumn: "id",
            keyValue: 2,
            columns: new[] { "parent_id", "proportion" },
            values: new object[] { 3, 100m });

        migrationBuilder.UpdateData(
            schema: "rc",
            table: "sizings",
            keyColumn: "id",
            keyValue: 3,
            columns: new[] { "parent_id", "proportion" },
            values: new object[] { null, null });

        migrationBuilder.UpdateData(
            schema: "rc",
            table: "sizings",
            keyColumn: "id",
            keyValue: 4,
            columns: new[] { "parent_id", "proportion" },
            values: new object[] { 5, 1000m });

        migrationBuilder.UpdateData(
            schema: "rc",
            table: "sizings",
            keyColumn: "id",
            keyValue: 5,
            columns: new[] { "parent_id", "proportion" },
            values: new object[] { null, null });

        migrationBuilder.UpdateData(
            schema: "rc",
            table: "sizings",
            keyColumn: "id",
            keyValue: 6,
            columns: new[] { "parent_id", "proportion" },
            values: new object[] { null, null });

        migrationBuilder.CreateIndex(
            name: "ix_sizings_parent_id",
            schema: "rc",
            table: "sizings",
            column: "parent_id");

        migrationBuilder.AddForeignKey(
            name: "fk_sizings_sizings_parent_id",
            schema: "rc",
            table: "sizings",
            column: "parent_id",
            principalSchema: "rc",
            principalTable: "sizings",
            principalColumn: "id",
            onDelete: ReferentialAction.Restrict);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "fk_sizings_sizings_parent_id",
            schema: "rc",
            table: "sizings");

        migrationBuilder.DropIndex(
            name: "ix_sizings_parent_id",
            schema: "rc",
            table: "sizings");

        migrationBuilder.DropColumn(
            name: "parent_id",
            schema: "rc",
            table: "sizings");

        migrationBuilder.DropColumn(
            name: "proportion",
            schema: "rc",
            table: "sizings");
    }
}
