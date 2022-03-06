using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ExpensesTrackerAPI.Migrations
{
    public partial class AddWeblog : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Weblogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LogTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LogLevel = table.Column<int>(type: "integer", nullable: false),
                    LogMessage = table.Column<string>(type: "text", nullable: false),
                    LogInfo1 = table.Column<string>(type: "text", nullable: false),
                    LogInfo2 = table.Column<string>(type: "text", nullable: false),
                    StackTracke = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Weblogs", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Weblogs");
        }
    }
}
