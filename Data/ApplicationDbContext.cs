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

        // ✅ Many-to-Many for Recipe ↔ Ingredients
        modelBuilder.Entity<RecipeIngredient>()
            .HasKey(ri => new { ri.RecipeId, ri.IngredientId });

        modelBuilder.Entity<RecipeIngredient>()
            .HasOne(ri => ri.Recipe)
            .WithMany(r => r.RecipeIngredients)
            .HasForeignKey(ri => ri.RecipeId);

        modelBuilder.Entity<RecipeIngredient>()
            .HasOne(ri => ri.Ingredient)
            .WithMany(i => i.RecipeIngredients)
            .HasForeignKey(ri => ri.IngredientId);

        // Many-to-Many for Recipe ↔ Tags
        modelBuilder.Entity<RecipeTag>()
        .HasKey(rt => new { rt.RecipeId, rt.TagId });

        modelBuilder.Entity<RecipeTag>()
            .HasOne(rt => rt.Recipe)
            .WithMany(r => r.RecipeTags)
            .HasForeignKey(rt => rt.RecipeId);

        modelBuilder.Entity<RecipeTag>()
            .HasOne(rt => rt.Tag)
            .WithMany(t => t.RecipeTags)
            .HasForeignKey(rt => rt.TagId);

        //  Define relationships
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

        // Seed Tags
        modelBuilder.Entity<Tag>().HasData(
            new Tag { Id = 1, Name = "Vegetarian" },
            new Tag { Id = 2, Name = "Gluten-Free" },
            new Tag { Id = 3, Name = "Vegan" },
            new Tag { Id = 4, Name = "Kosher" },
            new Tag { Id = 5, Name = "Spicy" },
            new Tag { Id = 6, Name = "Halal" }
        );
    }
}

