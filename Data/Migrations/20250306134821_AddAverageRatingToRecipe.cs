using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RecipeWebbApplication.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAverageRatingToRecipe : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "AverageRating",
                table: "Recipes",
                type: "float",
                nullable: false,
                defaultValue: 0.0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AverageRating",
                table: "Recipes");
        }
    }
}
