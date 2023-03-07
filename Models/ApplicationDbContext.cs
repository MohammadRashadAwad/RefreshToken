using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace RefreshToken.Models
{
    public class ApplicationDbContext:IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options):base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<ApplicationUser>().ToTable("Users", "jwt");
            builder.Entity<IdentityRole>().ToTable("Roles", "jwt");
            builder.Entity<IdentityUserRole<string>>().ToTable("UserRoles", "jwt");
            builder.Entity<IdentityRoleClaim<string>>().ToTable("RoleClaim", "jwt");
            builder.Entity<IdentityUserClaim<string>>().ToTable("UserClaim", "jwt");
            builder.Entity<IdentityUserLogin<string>>().ToTable("UserLogin", "jwt");
            builder.Entity<IdentityUserToken<string>>().ToTable("UserToken", "jwt");

        }
    }
}
