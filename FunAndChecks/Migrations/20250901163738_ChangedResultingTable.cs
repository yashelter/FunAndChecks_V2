using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FunAndChecks.Migrations
{
    /// <inheritdoc />
    public partial class ChangedResultingTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Letter",
                table: "AspNetUsers",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Letter",
                table: "AspNetUsers");
        }
    }
}
