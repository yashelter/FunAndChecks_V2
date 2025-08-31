using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FunAndChecks.Migrations
{
    /// <inheritdoc />
    public partial class MinorChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "QueueEventGroups");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "QueueEventGroups",
                columns: table => new
                {
                    QueueEventId = table.Column<int>(type: "integer", nullable: false),
                    GroupId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QueueEventGroups", x => new { x.QueueEventId, x.GroupId });
                    table.ForeignKey(
                        name: "FK_QueueEventGroups_Groups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "Groups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_QueueEventGroups_QueueEvents_QueueEventId",
                        column: x => x.QueueEventId,
                        principalTable: "QueueEvents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_QueueEventGroups_GroupId",
                table: "QueueEventGroups",
                column: "GroupId");
        }
    }
}
