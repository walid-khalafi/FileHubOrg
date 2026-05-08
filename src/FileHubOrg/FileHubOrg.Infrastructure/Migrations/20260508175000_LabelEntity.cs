using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FileHubOrg.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class LabelEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "LabelId",
                table: "FileMetaData",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Label",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedByIP = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedByIP = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "varbinary(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Label", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FileMetaData_LabelId",
                table: "FileMetaData",
                column: "LabelId");

            migrationBuilder.AddForeignKey(
                name: "FK_FileMetaData_Label_LabelId",
                table: "FileMetaData",
                column: "LabelId",
                principalTable: "Label",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FileMetaData_Label_LabelId",
                table: "FileMetaData");

            migrationBuilder.DropTable(
                name: "Label");

            migrationBuilder.DropIndex(
                name: "IX_FileMetaData_LabelId",
                table: "FileMetaData");

            migrationBuilder.DropColumn(
                name: "LabelId",
                table: "FileMetaData");
        }
    }
}
