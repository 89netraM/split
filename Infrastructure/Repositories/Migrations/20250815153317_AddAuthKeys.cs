using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Split.Infrastructure.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class AddAuthKeys : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AuthKeyEntity",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Key = table.Column<byte[]>(type: "bytea", nullable: false),
                    SignCount = table.Column<long>(type: "bigint", nullable: false),
                    UserAggregateId = table.Column<string>(type: "text", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuthKeyEntity", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AuthKeyEntity_Users_UserAggregateId",
                        column: x => x.UserAggregateId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateIndex(
                name: "IX_AuthKeyEntity_UserAggregateId",
                table: "AuthKeyEntity",
                column: "UserAggregateId"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "AuthKeyEntity");
        }
    }
}
