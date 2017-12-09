using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace Zdy.Blog.Migrations
{
    public partial class ZdyMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Category",
                columns: table => new
                {
                    ID = table.Column<Guid>(nullable: false),
                    SourceID = table.Column<Guid>(nullable: false),
                    Text = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Category", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Comment",
                columns: table => new
                {
                    ID = table.Column<Guid>(nullable: false),
                    Author = table.Column<string>(nullable: true),
                    Content = table.Column<string>(nullable: true),
                    Email = table.Column<string>(nullable: true),
                    Ip = table.Column<string>(nullable: true),
                    IsAdmin = table.Column<bool>(nullable: false),
                    IsApproved = table.Column<bool>(nullable: false),
                    PubDate = table.Column<DateTime>(nullable: false),
                    SourceID = table.Column<Guid>(nullable: false),
                    UserAgent = table.Column<string>(nullable: true),
                    Website = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Comment", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Gallery",
                columns: table => new
                {
                    ID = table.Column<Guid>(nullable: false),
                    Categories = table.Column<string>(nullable: true),
                    Excerpt = table.Column<string>(nullable: true),
                    IsPublished = table.Column<bool>(nullable: false),
                    PubDate = table.Column<DateTime>(nullable: false),
                    Slug = table.Column<string>(nullable: true),
                    Title = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Gallery", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Photo",
                columns: table => new
                {
                    ID = table.Column<Guid>(nullable: false),
                    Excerpt = table.Column<string>(nullable: true),
                    FileName = table.Column<string>(nullable: true),
                    PubDate = table.Column<DateTime>(nullable: false),
                    Size = table.Column<long>(nullable: false),
                    SourceID = table.Column<Guid>(nullable: false),
                    Title = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Photo", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Post",
                columns: table => new
                {
                    ID = table.Column<Guid>(nullable: false),
                    Author = table.Column<string>(nullable: true),
                    Categories = table.Column<string>(nullable: true),
                    Content = table.Column<string>(nullable: true),
                    Excerpt = table.Column<string>(nullable: true),
                    IsPublished = table.Column<bool>(nullable: false),
                    LastModified = table.Column<DateTime>(nullable: false),
                    PubDate = table.Column<DateTime>(nullable: false),
                    Slug = table.Column<string>(nullable: true),
                    Title = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Post", x => x.ID);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Category");

            migrationBuilder.DropTable(
                name: "Comment");

            migrationBuilder.DropTable(
                name: "Gallery");

            migrationBuilder.DropTable(
                name: "Photo");

            migrationBuilder.DropTable(
                name: "Post");
        }
    }
}
