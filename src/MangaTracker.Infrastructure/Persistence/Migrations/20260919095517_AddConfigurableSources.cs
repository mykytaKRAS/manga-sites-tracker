using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MangaTracker.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddConfigurableSources : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Sources",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Kind = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ChapterLinkSelector = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ChapterNumberPattern = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    NumberSource = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsEnabled = table.Column<bool>(type: "bit", nullable: false),
                    DisabledReason = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sources", x => x.Id);
                });

            migrationBuilder.Sql(@"
                SET IDENTITY_INSERT dbo.Sources ON;

                INSERT INTO dbo.Sources
                    (Id, Name, Kind, ChapterLinkSelector, ChapterNumberPattern, NumberSource, IsEnabled, DisabledReason, CreatedAt)
                VALUES
                    (1, 'MangaLib',           'MangaLibApi', NULL,                                   NULL, 'LinkTextThenHref', 1, NULL, SYSUTCDATETIME()),
                    (2, 'MangaShi',           'Html',        'div#chapters-list a',                  NULL, 'LinkTextThenHref', 1, NULL, SYSUTCDATETIME()),
                    (3, 'BluePeriodChapters', 'Html',        'div#Chapters_List ul li a',            NULL, 'LinkTextThenHref', 1, NULL, SYSUTCDATETIME()),
                    (4, 'RagnarokManga',      'Html',        'div.su-expand-content.su-u-trim li a', NULL, 'LinkTextThenHref', 1, NULL, SYSUTCDATETIME()),
                    (5, 'ReadHxh',            'Html',        'tr td a',                              NULL, 'LinkTextThenHref', 1, NULL, SYSUTCDATETIME());

                SET IDENTITY_INSERT dbo.Sources OFF;
            ");

            migrationBuilder.AddColumn<int>(
            name: "SourceId",
            table: "Mangas",
            type: "int",
            nullable: false,
            defaultValue: 0);

            migrationBuilder.Sql(@"
                UPDATE m
                SET m.SourceId = s.Id
                FROM dbo.Mangas m
                INNER JOIN dbo.Sources s ON s.Name = m.Source;
            ");

            migrationBuilder.DropColumn(
                name: "Source",
                table: "Mangas"
            );

            migrationBuilder.CreateIndex(
                name: "IX_Sources_Name",
                table: "Sources",
                column: "Name",
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "IX_Mangas_SourceId",
                table: "Mangas",
                column: "SourceId"
            );

            migrationBuilder.AddForeignKey(
                name: "FK_Mangas_Sources_SourceId",
                table: "Mangas",
                column: "SourceId",
                principalTable: "Sources",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Mangas_Sources_SourceId",
                table: "Mangas");

            migrationBuilder.DropTable(
                name: "Sources");

            migrationBuilder.DropIndex(
                name: "IX_Mangas_SourceId",
                table: "Mangas");

            migrationBuilder.DropColumn(
                name: "SourceId",
                table: "Mangas");

            migrationBuilder.AddColumn<string>(
                name: "Source",
                table: "Mangas",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");
        }
    }
}
