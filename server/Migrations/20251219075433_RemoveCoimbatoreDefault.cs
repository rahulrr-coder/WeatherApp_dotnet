using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WeatherApp.Migrations
{
    /// <inheritdoc />
    public partial class RemoveCoimbatoreDefault : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 🟢 FORCE DROP the default value in PostgreSQL
            migrationBuilder.Sql("ALTER TABLE \"Users\" ALTER COLUMN \"SubscriptionCity\" DROP DEFAULT;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Optional: Restore it if you ever rollback (unlikely needed)
            migrationBuilder.Sql("ALTER TABLE \"Users\" ALTER COLUMN \"SubscriptionCity\" SET DEFAULT 'Coimbatore';");
        }
    }
}