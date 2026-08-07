using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UserService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMissingFieldsToResidentDues : Migration
    {
		/// <inheritdoc />
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			// ✅ ONLY add the missing columns - NO CreateTable!
			migrationBuilder.AddColumn<Guid>(
				name: "EstateId",
				table: "ResidentDues",
				type: "char(36)",
				nullable: false,
				defaultValue: "00000000-0000-0000-0000-000000000000");

			migrationBuilder.AddColumn<Guid>(
				name: "UserId",
				table: "ResidentDues",
				type: "char(36)",
				nullable: false,
				defaultValue: "00000000-0000-0000-0000-000000000000");

			migrationBuilder.AddColumn<string>(
				name: "Email",
				table: "ResidentDues",
				type: "longtext",
				nullable: true);

			// ✅ Optional: Add indexes for performance
			migrationBuilder.CreateIndex(
				name: "IX_ResidentDues_EstateId",
				table: "ResidentDues",
				column: "EstateId");

			migrationBuilder.CreateIndex(
				name: "IX_ResidentDues_UserId",
				table: "ResidentDues",
				column: "UserId");
		}

		/// <inheritdoc />
		protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "EstateDue");

            migrationBuilder.DropTable(
                name: "EstateRegistration");

            migrationBuilder.DropTable(
                name: "ResidentDues");

            migrationBuilder.DropTable(
                name: "ResidentRegistration");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "AspNetUsers");
        }
    }
}
