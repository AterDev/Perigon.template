using Entity;
using Perigon.PostgreSQL;
using System.Diagnostics.CodeAnalysis;

namespace EntityFramework.AppDbContext;

public partial class DefaultDbContext(string connectionString)
    : DbContext(builder => builder.UsePostgres(connectionString))
{
    public new DbSet<TEntity> Set<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TEntity>()
        where TEntity : class
    {
        return base.Set<TEntity>();
    }

    public DbSet<User> Users => Set<User>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("users");
                entity.Property(user => user.UserName).IsRequired();
                entity.HasIndex(user => user.UserName)
                    .HasDatabaseName("uq_users_user_name")
                    .IsUnique();
            });
        }
    }
