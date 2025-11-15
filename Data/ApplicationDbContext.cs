using BasicBlog_Migrated.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore; // <-- THIS WAS THE MAIN MISSING LINE

namespace BasicBlog_Migrated.Data
{
    // Note: It inherits from IdentityDbContext<ApplicationUser>
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        // Your custom DbSets
        public DbSet<Blog> Blogs { get; set; }
        public DbSet<Comment> Comments { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // This tells EF Core to use your *existing* table names
            // from the .NET 4.8 database instead of creating new ones.

            builder.Entity<ApplicationUser>(e => e.ToTable("AspNetUsers"));
            builder.Entity<IdentityRole>(e => e.ToTable("AspNetRoles"));
            builder.Entity<IdentityUserRole<string>>(e => e.ToTable("AspNetUserRoles"));
            builder.Entity<IdentityUserClaim<string>>(e => e.ToTable("AspNetUserClaims"));
            builder.Entity<IdentityUserLogin<string>>(e => e.ToTable("AspNetUserLogins"));
            builder.Entity<IdentityRoleClaim<string>>(e => e.ToTable("AspNetRoleClaims"));
            builder.Entity<IdentityUserToken<string>>(e => e.ToTable("AspNetUserTokens"));

            // --- Configure Relationships ---
            // Configure Blog -> ApplicationUser relationship
            builder.Entity<Blog>()
                .HasOne(b => b.BlogOwner)
                .WithMany()
                .HasForeignKey(b => b.BlogOwnerId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure Blog -> Comments relationship
            builder.Entity<Blog>()
                .HasMany(b => b.Comments)
                .WithOne(c => c.Blog)
                .HasForeignKey(c => c.BlogId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}