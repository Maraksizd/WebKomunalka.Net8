using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebKomunalka.Net8.Data.Migrations
{
    /// <inheritdoc />
    public partial class addAlarmsTablets1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AllAlarmModels",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<int>(type: "INTEGER", nullable: false),
                    AlarmName = table.Column<string>(type: "TEXT", nullable: false),
                    RealAlarmName = table.Column<string>(type: "TEXT", nullable: false),
                    GroupId = table.Column<int>(type: "INTEGER", nullable: false),
                    Duration = table.Column<DateTime>(type: "TEXT", nullable: false),
                    AlarmSize = table.Column<double>(type: "REAL", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AllAlarmModels", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CityModels",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CityId = table.Column<int>(type: "INTEGER", nullable: false),
                    CityName = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CityModels", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LogsModels",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<int>(type: "INTEGER", nullable: false),
                    AlarmId = table.Column<int>(type: "INTEGER", nullable: false),
                    SityId = table.Column<int>(type: "INTEGER", nullable: false),
                    WSId = table.Column<int>(type: "INTEGER", nullable: false),
                    Date = table.Column<DateTime>(type: "TEXT", nullable: false),
                    StartTime = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LogsModels", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WorkStantionModels",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CityId = table.Column<int>(type: "INTEGER", nullable: false),
                    WSId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkStantionModels", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AllAlarmModels");

            migrationBuilder.DropTable(
                name: "CityModels");

            migrationBuilder.DropTable(
                name: "LogsModels");

            migrationBuilder.DropTable(
                name: "WorkStantionModels");
        }
    }
}
