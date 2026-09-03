using ECart.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECart.Infrastructure.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("Users");

            builder.HasKey(u => u.Id);

            builder.Property(u => u.FirstName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(u => u.LastName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(u => u.Email)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(u => u.PasswordHash)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(u => u.Phone)
                .HasMaxLength(20);

            // Unique email — no two users can register with the same email
            builder.HasIndex(u => u.Email)
                .IsUnique();

            // Relationship: User → Role (Many-to-One)
            // "A user HAS ONE role, a role HAS MANY users"
            builder.HasOne(u => u.Role)
                .WithMany(r => r.Users)
                .HasForeignKey(u => u.RoleId)
                .OnDelete(DeleteBehavior.Restrict);
            // Restrict = if you try to delete a Role that has Users,
            // the database will throw an error. This prevents orphaned users.

            // Relationship: User → Addresses (One-to-Many)
            builder.HasMany(u => u.Addresses)
                .WithOne(a => a.User)
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            // Cascade = if you delete a User, all their Addresses are deleted too.

            // Relationship: User → Cart (One-to-One)
            builder.HasOne(u => u.Cart)
                .WithOne(c => c.User)
                .HasForeignKey<Cart>(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Relationship: User → Wishlist (One-to-One)
            builder.HasOne(u => u.Wishlist)
                .WithOne(w => w.User)
                .HasForeignKey<Wishlist>(w => w.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Relationship: User → Orders (One-to-Many)
            builder.HasMany(u => u.Orders)
                .WithOne(o => o.User)
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.Restrict);
            // Restrict = don't delete a user if they have orders.
            // We need order history for business records!

            // Relationship: User → Reviews (One-to-Many)
            builder.HasMany(u => u.Reviews)
                .WithOne(r => r.User)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Seed default admin user
            // Password is "Admin@123" hashed with BCrypt
            builder.HasData(
                new User
                {
                    Id = 1,
                    FirstName = "System",
                    LastName = "Admin",
                    Email = "admin@ecart.com",
                    PasswordHash = "$2a$11$n7VQX6YJYj5GJHGFHJFGOeNjJFGOeNjJFGOeNjJFGOeNjJFGOeNj",
                    Phone = null,
                    RoleId = 2,
                    IsActive = true,
                    CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                }
            );
        }
    }
}