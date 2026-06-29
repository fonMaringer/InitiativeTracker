using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InitiativeTracker.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Encounters",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CurrentRound = table.Column<int>(type: "INTEGER", nullable: false),
                    CurrentActiveParticipantId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Encounters", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MagicItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Type = table.Column<string>(type: "TEXT", nullable: true),
                    Rarity = table.Column<int>(type: "INTEGER", nullable: false),
                    RequiresAttunement = table.Column<bool>(type: "INTEGER", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    PrintedCount = table.Column<int>(type: "INTEGER", nullable: false),
                    Link = table.Column<string>(type: "TEXT", nullable: true),
                    Source = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MagicItems", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Miniatures",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    ImageData = table.Column<byte[]>(type: "BLOB", nullable: true),
                    CroppedImageData = table.Column<byte[]>(type: "BLOB", nullable: true),
                    Size = table.Column<int>(type: "INTEGER", nullable: false),
                    PrintedCount = table.Column<int>(type: "INTEGER", nullable: false),
                    Link = table.Column<string>(type: "TEXT", nullable: true),
                    CropX = table.Column<double>(type: "REAL", nullable: false),
                    CropY = table.Column<double>(type: "REAL", nullable: false),
                    CropWidth = table.Column<double>(type: "REAL", nullable: false),
                    CropHeight = table.Column<double>(type: "REAL", nullable: false),
                    NaturalWidth = table.Column<double>(type: "REAL", nullable: false),
                    NaturalHeight = table.Column<double>(type: "REAL", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Miniatures", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ParticipantCatalog",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Hits = table.Column<int>(type: "INTEGER", nullable: false),
                    ArmorClass = table.Column<int>(type: "INTEGER", nullable: false),
                    Dexterity = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParticipantCatalog", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Spells",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Type = table.Column<string>(type: "TEXT", nullable: false),
                    VerbalComponent = table.Column<bool>(type: "INTEGER", nullable: false),
                    SomaticComponent = table.Column<bool>(type: "INTEGER", nullable: false),
                    MaterialComponent = table.Column<string>(type: "TEXT", nullable: true),
                    Range = table.Column<string>(type: "TEXT", nullable: false),
                    Duration = table.Column<string>(type: "TEXT", nullable: false),
                    Time = table.Column<string>(type: "TEXT", nullable: false),
                    Level = table.Column<int>(type: "INTEGER", nullable: false),
                    Classes = table.Column<string>(type: "TEXT", nullable: false),
                    Subclasses = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    Upper = table.Column<string>(type: "TEXT", nullable: true),
                    PrintedCount = table.Column<int>(type: "INTEGER", nullable: false),
                    Link = table.Column<string>(type: "TEXT", nullable: true),
                    Concentration = table.Column<bool>(type: "INTEGER", nullable: false),
                    Source = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Spells", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Stands",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ImageData = table.Column<byte[]>(type: "BLOB", nullable: true),
                    InverseTextColor = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Stands", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EncounterParticipants",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    EncounterId = table.Column<int>(type: "INTEGER", nullable: false),
                    Order = table.Column<int>(type: "INTEGER", nullable: false),
                    Initiative = table.Column<int>(type: "INTEGER", nullable: false),
                    Dexterity = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    HitsAverage = table.Column<int>(type: "INTEGER", nullable: false),
                    HitsFormula = table.Column<string>(type: "TEXT", nullable: true),
                    HitsBonus = table.Column<int>(type: "INTEGER", nullable: true),
                    HitsDefault = table.Column<int>(type: "INTEGER", nullable: false),
                    HitsCurrent = table.Column<int>(type: "INTEGER", nullable: false),
                    ArmorClass = table.Column<int>(type: "INTEGER", nullable: false),
                    ArmorClassCurrent = table.Column<int>(type: "INTEGER", nullable: false),
                    Link = table.Column<string>(type: "TEXT", nullable: true),
                    Source = table.Column<int>(type: "INTEGER", nullable: false),
                    ChangeHpValue = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EncounterParticipants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EncounterParticipants_Encounters_EncounterId",
                        column: x => x.EncounterId,
                        principalTable: "Encounters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EncounterParticipants_EncounterId",
                table: "EncounterParticipants",
                column: "EncounterId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EncounterParticipants");

            migrationBuilder.DropTable(
                name: "MagicItems");

            migrationBuilder.DropTable(
                name: "Miniatures");

            migrationBuilder.DropTable(
                name: "ParticipantCatalog");

            migrationBuilder.DropTable(
                name: "Spells");

            migrationBuilder.DropTable(
                name: "Stands");

            migrationBuilder.DropTable(
                name: "Encounters");
        }
    }
}
