using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClientManagementSystem.Migrations
{
    public partial class UpdateIndexesAndClientCode : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop old PK on ClientContacts
            migrationBuilder.DropPrimaryKey(
                name: "PK_ClientContacts",
                table: "ClientContacts");

            migrationBuilder.DropIndex(
                name: "IX_ClientContacts_ClientId_ContactId",
                table: "ClientContacts");

            // Alter Clients.Email to nvarchar(255)
            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Clients",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            // Add ClientCode column
            migrationBuilder.AddColumn<string>(
                name: "ClientCode",
                table: "Clients",
                type: "nvarchar(6)",
                maxLength: 6,
                nullable: false,
                defaultValue: "");

            // ✅ Remove AlterColumn on ClientContactId (caused identity error)

            // Add composite PK on ClientContacts
            migrationBuilder.AddPrimaryKey(
                name: "PK_ClientContacts",
                table: "ClientContacts",
                columns: new[] { "ClientId", "ContactId" });

            // Add unique index on ClientCode
            migrationBuilder.CreateIndex(
                name: "IX_Clients_ClientCode",
                table: "Clients",
                column: "ClientCode",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop unique index on ClientCode
            migrationBuilder.DropIndex(
                name: "IX_Clients_ClientCode",
                table: "Clients");

            // Drop composite PK
            migrationBuilder.DropPrimaryKey(
                name: "PK_ClientContacts",
                table: "ClientContacts");

            // Drop ClientCode column
            migrationBuilder.DropColumn(
                name: "ClientCode",
                table: "Clients");

            // Revert Clients.Email back to nvarchar(max)
            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Clients",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255);

            // ✅ No need to alter ClientContactId identity here

            // Restore PK on ClientContactId
            migrationBuilder.AddPrimaryKey(
                name: "PK_ClientContacts",
                table: "ClientContacts",
                column: "ClientContactId");

            // Restore unique index on ClientId + ContactId
            migrationBuilder.CreateIndex(
                name: "IX_ClientContacts_ClientId_ContactId",
                table: "ClientContacts",
                columns: new[] { "ClientId", "ContactId" },
                unique: true);
        }
    }
}