using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace Zdy.Blog.Migrations
{
    public partial class ZdyMigration_01 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CheckNumber",
                table: "Post",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Sort",
                table: "Photo",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CheckNumber",
                table: "Gallery",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CheckNumber",
                table: "Post");

            migrationBuilder.DropColumn(
                name: "Sort",
                table: "Photo");

            migrationBuilder.DropColumn(
                name: "CheckNumber",
                table: "Gallery");
        }
    }
}
