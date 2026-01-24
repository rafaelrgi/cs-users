using Microsoft.EntityFrameworkCore;
using Users.src.Domain.Entities;

namespace Users.Data
{
  public class Db : DbContext
  {
    private const string CONNECTION_NAME = "DefaultConnection";
    private const string DOCKER_CONNECTION_NAME = "DockerConnection";

    public static string ConnectionName { get => (Environment.GetEnvironmentVariable("IS_DOCKER") == "true") ? DOCKER_CONNECTION_NAME : CONNECTION_NAME; }
    public Db(DbContextOptions<Db> options) : base(options) { }

    public DbSet<User> Users { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
      //bring the also the "blocked" (deleted) users, so they can be unblocked
      //modelBuilder.Entity<User>().HasQueryFilter(p => p.DeletedAt == null);
    }

    public override int SaveChanges()
    {
      AddTimestamps();
      return base.SaveChanges();
    }

    public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default(CancellationToken))
    {
      AddTimestamps();
      return await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default(CancellationToken))
    {
      AddTimestamps();
      return await base.SaveChangesAsync(cancellationToken);
    }

    private void AddTimestamps()
    {
      var entities = ChangeTracker.Entries()
          .Where(x => x.Entity is BaseEntity && (x.State == EntityState.Added || x.State == EntityState.Modified));

      foreach (var entity in entities)
      {
        var now = DateTime.Now; //UtcNow ???
        if (entity.State == EntityState.Added)
          ((BaseEntity)entity.Entity).CreatedAt = now;

        ((BaseEntity)entity.Entity).UpdatedAt = now;
      }
    }
  }
}
