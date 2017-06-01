// --------------------------------------------------------------------------------------------------------------------
// <copyright file="20170601045606_Init.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   The initial database migration
// </summary>
// --------------------------------------------------------------------------------------------------------------------

// ReSharper disable ArgumentsStyleStringLiteral
// ReSharper disable RedundantArgumentDefaultValue
namespace ClusterKit.NodeManager.ConfigurationSource.Migrations
{
    using System;

    using Microsoft.EntityFrameworkCore.Migrations;

    /// <summary>
    /// The initial database migration
    /// </summary>
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "CompatibleTemplate");

            migrationBuilder.DropTable(name: "MigrationLogRecords");

            migrationBuilder.DropTable(name: "RoleUsers");

            migrationBuilder.DropTable(name: "Migrations");

            migrationBuilder.DropTable(name: "Roles");

            migrationBuilder.DropTable(name: "Users");

            migrationBuilder.DropTable(name: "Releases");
        }

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Releases",
                columns: table => new
                                      {
                                          Id =
                                          table.Column<int>(type: "serial", nullable: false)
                                              .Annotation("Npgsql:ValueGenerationStrategy", 1),
                                          ConfigurationJson = table.Column<string>(nullable: true),
                                          Created = table.Column<DateTimeOffset>(nullable: false),
                                          Finished = table.Column<DateTimeOffset>(nullable: true),
                                          IsStable = table.Column<bool>(nullable: false),
                                          MajorVersion = table.Column<int>(nullable: false),
                                          MinorVersion = table.Column<int>(nullable: false),
                                          Name = table.Column<string>(nullable: true),
                                          Notes = table.Column<string>(nullable: true),
                                          Started = table.Column<DateTimeOffset>(nullable: true),
                                          State = table.Column<int>(nullable: false)
                                      },
                constraints: table => { table.PrimaryKey("PK_Releases", x => x.Id); });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                                      {
                                          Uid = table.Column<Guid>(nullable: false),
                                          AllowedScopeJson = table.Column<string>(nullable: true),
                                          DeniedScopeJson = table.Column<string>(nullable: true),
                                          Name = table.Column<string>(nullable: true)
                                      },
                constraints: table => { table.PrimaryKey("PK_Roles", x => x.Uid); });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                                      {
                                          Uid = table.Column<Guid>(nullable: false),
                                          ActiveTill = table.Column<DateTimeOffset>(nullable: true),
                                          BlockedTill = table.Column<DateTimeOffset>(nullable: true),
                                          IsBlocked = table.Column<bool>(nullable: false),
                                          IsDeleted = table.Column<bool>(nullable: false),
                                          Login = table.Column<string>(nullable: true),
                                          Password = table.Column<string>(nullable: true)
                                      },
                constraints: table => { table.PrimaryKey("PK_Users", x => x.Uid); });

            migrationBuilder.CreateTable(
                name: "CompatibleTemplate",
                columns: table => new
                                      {
                                          Id = table.Column<int>(type: "serial", nullable: false)
                                              .Annotation("Npgsql:ValueGenerationStrategy", 1),
                                          CompatibleReleaseId = table.Column<int>(nullable: false),
                                          ReleaseId = table.Column<int>(nullable: false),
                                          TemplateCode = table.Column<string>(nullable: true)
                                      },
                constraints: table =>
                    {
                        table.PrimaryKey("PK_CompatibleTemplate", x => x.Id);
                        table.ForeignKey(
                            name: "FK_CompatibleTemplate_Releases_CompatibleReleaseId",
                            column: x => x.CompatibleReleaseId,
                            principalTable: "Releases",
                            principalColumn: "Id",
                            onDelete: ReferentialAction.Cascade);
                        table.ForeignKey(
                            name: "FK_CompatibleTemplate_Releases_ReleaseId",
                            column: x => x.ReleaseId,
                            principalTable: "Releases",
                            principalColumn: "Id",
                            onDelete: ReferentialAction.Cascade);
                    });

            migrationBuilder.CreateTable(
                name: "Migrations",
                columns: table => new
                                      {
                                          Id = table.Column<int>(type: "serial", nullable: false)
                                              .Annotation("Npgsql:ValueGenerationStrategy", 1),
                                          Direction = table.Column<int>(nullable: true),
                                          Finished = table.Column<DateTimeOffset>(nullable: true),
                                          FromReleaseId = table.Column<int>(nullable: false),
                                          IsActive = table.Column<bool>(nullable: false),
                                          Started = table.Column<DateTimeOffset>(nullable: false),
                                          State = table.Column<int>(nullable: false),
                                          ToReleaseId = table.Column<int>(nullable: false)
                                      },
                constraints: table =>
                    {
                        table.PrimaryKey("PK_Migrations", x => x.Id);
                        table.ForeignKey(
                            name: "FK_Migrations_Releases_FromReleaseId",
                            column: x => x.FromReleaseId,
                            principalTable: "Releases",
                            principalColumn: "Id",
                            onDelete: ReferentialAction.Cascade);
                        table.ForeignKey(
                            name: "FK_Migrations_Releases_ToReleaseId",
                            column: x => x.ToReleaseId,
                            principalTable: "Releases",
                            principalColumn: "Id",
                            onDelete: ReferentialAction.Cascade);
                    });

            migrationBuilder.CreateTable(
                name: "RoleUsers",
                columns: table => new
                                      {
                                          UserUid = table.Column<Guid>(nullable: false),
                                          RoleUid = table.Column<Guid>(nullable: false)
                                      },
                constraints: table =>
                    {
                        table.PrimaryKey("PK_RoleUsers", x => new { x.UserUid, x.RoleUid });
                        table.ForeignKey(
                            name: "FK_RoleUsers_Roles_RoleUid",
                            column: x => x.RoleUid,
                            principalTable: "Roles",
                            principalColumn: "Uid",
                            onDelete: ReferentialAction.Cascade);
                        table.ForeignKey(
                            name: "FK_RoleUsers_Users_UserUid",
                            column: x => x.UserUid,
                            principalTable: "Users",
                            principalColumn: "Uid",
                            onDelete: ReferentialAction.Cascade);
                    });

            migrationBuilder.CreateTable(
                name: "MigrationLogRecords",
                columns: table => new
                                      {
                                          Id =
                                          table.Column<int>(type: "serial", nullable: false)
                                              .Annotation("Npgsql:ValueGenerationStrategy", 1),
                                          Discriminator = table.Column<string>(nullable: false),
                                          MigrationId = table.Column<int>(nullable: true),
                                          MigratorName = table.Column<string>(nullable: true),
                                          MigratorTemplateCode = table.Column<string>(nullable: true),
                                          MigratorTemplateName = table.Column<string>(nullable: true),
                                          MigratorTypeName = table.Column<string>(nullable: true),
                                          ReleaseId = table.Column<int>(nullable: false),
                                          ResourceCode = table.Column<string>(nullable: true),
                                          ResourceName = table.Column<string>(nullable: true),
                                          Created = table.Column<DateTimeOffset>(nullable: true),
                                          ErrorMessage = table.Column<string>(nullable: true),
                                          ErrorStackTrace = table.Column<string>(nullable: true),
                                          DestinationPoint = table.Column<string>(nullable: true),
                                          ErrorId = table.Column<int>(nullable: true),
                                          Finished = table.Column<DateTimeOffset>(nullable: true),
                                          SourcePoint = table.Column<string>(nullable: true),
                                          Started = table.Column<DateTimeOffset>(nullable: true)
                                      },
                constraints: table =>
                    {
                        table.PrimaryKey("PK_MigrationLogRecords", x => x.Id);
                        table.ForeignKey(
                            name: "FK_MigrationLogRecords_Migrations_MigrationId",
                            column: x => x.MigrationId,
                            principalTable: "Migrations",
                            principalColumn: "Id",
                            onDelete: ReferentialAction.Restrict);
                        table.ForeignKey(
                            name: "FK_MigrationLogRecords_Releases_ReleaseId",
                            column: x => x.ReleaseId,
                            principalTable: "Releases",
                            principalColumn: "Id",
                            onDelete: ReferentialAction.Cascade);
                        table.ForeignKey(
                            name: "FK_MigrationLogRecords_MigrationLogRecords_ErrorId",
                            column: x => x.ErrorId,
                            principalTable: "MigrationLogRecords",
                            principalColumn: "Id",
                            onDelete: ReferentialAction.Restrict);
                    });

            migrationBuilder.CreateIndex(
                name: "IX_CompatibleTemplate_CompatibleReleaseId",
                table: "CompatibleTemplate",
                column: "CompatibleReleaseId");

            migrationBuilder.CreateIndex(
                name: "IX_CompatibleTemplate_ReleaseId",
                table: "CompatibleTemplate",
                column: "ReleaseId");

            migrationBuilder.CreateIndex(
                name: "IX_Migrations_FromReleaseId",
                table: "Migrations",
                column: "FromReleaseId");

            migrationBuilder.CreateIndex(name: "IX_Migrations_ToReleaseId", table: "Migrations", column: "ToReleaseId");

            migrationBuilder.CreateIndex(
                name: "IX_MigrationLogRecords_MigrationId",
                table: "MigrationLogRecords",
                column: "MigrationId");

            migrationBuilder.CreateIndex(
                name: "IX_MigrationLogRecords_ReleaseId",
                table: "MigrationLogRecords",
                column: "ReleaseId");

            migrationBuilder.CreateIndex(
                name: "IX_MigrationLogRecords_ErrorId",
                table: "MigrationLogRecords",
                column: "ErrorId");

            migrationBuilder.CreateIndex(name: "IX_RoleUsers_RoleUid", table: "RoleUsers", column: "RoleUid");

            migrationBuilder.CreateIndex(name: "IX_Users_Login", table: "Users", column: "Login");
        }
    }
}