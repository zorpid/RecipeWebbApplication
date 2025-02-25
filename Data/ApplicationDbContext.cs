using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RecipeWebbApplication.Models;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Recipe> Recipes { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Ingredient> Ingredients { get; set; }
    public DbSet<RecipeIngredient> RecipeIngredients { get; set; }
    public DbSet<RecipeTag> RecipeTags { get; set; }
    public DbSet<Tag> Tags { get; set; }
    public DbSet<RecipeReview> RecipeReviews { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Many-to-Many for Recipe ↔ Ingredients
        modelBuilder.Entity<RecipeIngredient>()
            .HasKey(ri => new { ri.RecipeId, ri.IngredientId });

        // Many-to-Many for Recipe ↔ Tags
        modelBuilder.Entity<RecipeTag>()
            .HasKey(rt => new { rt.RecipeId, rt.TagId });

        // Define relationships
        modelBuilder.Entity<Recipe>()
            .HasOne(r => r.CreatedByUser)
            .WithMany(u => u.Recipes)
            .HasForeignKey(r => r.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<RecipeReview>()
            .HasOne(rr => rr.User)
            .WithMany(u => u.Reviews)
            .HasForeignKey(rr => rr.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // SEED DATA for Categories Table 
        modelBuilder.Entity<Category>().HasData(
            new Category { Id = 1, Name = "Breakfast" },
            new Category { Id = 2, Name = "Lunch" },
            new Category { Id = 3, Name = "Dinner" },
            new Category { Id = 4, Name = "Dessert" },
            new Category { Id = 5, Name = "Snack" }
        );
    }
}

