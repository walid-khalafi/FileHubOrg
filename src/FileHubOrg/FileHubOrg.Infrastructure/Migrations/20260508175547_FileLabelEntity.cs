using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FileHubOrg.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FileLabelEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FileMetaData_Label_LabelId",
                table: "FileMetaData");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Label",
                table: "Label");

            migrationBuilder.RenameTable(
                name: "Label",
                newName: "Labels");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Labels",
                table: "Labels",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_FileMetaData_Labels_LabelId",
                table: "FileMetaData",
                column: "LabelId",
                principalTable: "Labels",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FileMetaData_Labels_LabelId",
                table: "FileMetaData");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Labels",
                table: "Labels");

            migrationBuilder.RenameTable(
                name: "Labels",
                newName: "Label");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Label",
                table: "Label",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_FileMetaData_Label_LabelId",
                table: "FileMetaData",
                column: "LabelId",
                principalTable: "Label",
                principalColumn: "Id");
        }
    }
}
