using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InitiativeTracker.Migrations
{
    /// <inheritdoc />
    public partial class AddSkipDeadToEncounter : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "SkipDead",
                table: "Encounters",
                type: "INTEGER",
                nullable: false,
                defaultValue: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SkipDead",
                table: "Encounters");
        }
    }
}
