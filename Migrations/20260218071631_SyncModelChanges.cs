using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClientManagementSystem.Migrations
{
    /// <inheritdoc />
    public partial class SyncModelChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_ClientContacts",
                table: "ClientContacts");

            migrationBuilder.AlterColumn<int>(
                name: "ClientContactId",
                table: "ClientContacts",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ClientContacts",
                table: "ClientContacts",
                column: "ClientContactId");

            migrationBuilder.CreateIndex(
                name: "IX_ClientContacts_ClientId_ContactId",
                table: "ClientContacts",
                columns: new[] { "ClientId", "ContactId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_ClientContacts",
                table: "ClientContacts");

            migrationBuilder.DropIndex(
                name: "IX_ClientContacts_ClientId_ContactId",
                table: "ClientContacts");

            migrationBuilder.AlterColumn<int>(
                name: "ClientContactId",
                table: "ClientContacts",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .OldAnnotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ClientContacts",
                table: "ClientContacts",
                columns: new[] { "ClientId", "ContactId" });
        }
    }
}
