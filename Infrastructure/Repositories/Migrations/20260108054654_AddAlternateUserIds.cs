using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Split.Infrastructure.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class AddAlternateUserIds : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AlternateUserId",
                columns: table => new
                {
                    Type = table.Column<string>(type: "text", nullable: false),
                    Id = table.Column<string>(type: "text", nullable: false),
                    UserAggregateId = table.Column<string>(type: "text", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlternateUserId", x => new { x.Type, x.Id });
                    table.ForeignKey(
                        name: "FK_AlternateUserId_Users_UserAggregateId",
                        column: x => x.UserAggregateId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateIndex(
                name: "IX_AlternateUserId_UserAggregateId_Type",
                table: "AlternateUserId",
                columns: new[] { "UserAggregateId", "Type" },
                unique: true
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "AlternateUserId");
        }
    }
}
