using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Users.src.Domain.Entities;
using Users.Infra;
using Users.src.Infra.Repositories;

namespace UserTests
{
  public sealed class UserRepositoryTests : IDisposable
  {
    private UserRepository? _repository;
    private Db? _context;

    public UserRepositoryTests()
    {
      var options = new DbContextOptionsBuilder<Db>()
          .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
          .Options;

      _context = new Db(options);
      _context.Database.EnsureCreated();

      _context.Users.Add(new User { Id = 1, Name = "Admin", Email = "admin@test.com", Password = "123", IsAdmin = true });
      _context.Users.Add(new User { Id = 2, Name = "John Doe", Email = "jd@test.com", Password = "456", IsAdmin = false });
      _context.Users.Add(new User { Id = 3, Name = "Agent Smith", Email = "as@test.com", Password = "789", IsAdmin = false });
      _context.Users.Add(new User { Id = 4, Name = "Killed", Email = "kil@test.com", Password = "000", DeletedAt = DateTime.UtcNow });

      _context.SaveChanges();
      _context.ChangeTracker.Clear();

      _repository = new UserRepository(_context);
    }

    public void Dispose()
    {
      _context!.Dispose();
      _context = null;
      _repository = null;
    }

    [Fact]
    public async Task FindByEmail_ShouldReturnUser_WhenExists()
    {
      // Act
      var user = await _repository!.FindByEmail("as@test.com");

      // Assert
      Assert.NotNull(user);
      Assert.Equal("Agent Smith", user.Name);
    }

    [Fact]
    public async Task FindAll_ShouldReturnOnlyActiveUsers()
    {
      // Act
      var result = await _repository!.FindAll(1, 10, "Name", "asc");

      // Assert
      Assert.Equal(3, result.RecordCount);
      Assert.DoesNotContain(result!.Data!, u => u.Name == "Killed");
    }

    [Fact]
    public async Task Save_ShouldInsertNewUser()
    {
      // Arrange
      var newUser = new User { Name = "Brand New", Email = "bd@test.com", Password = "password" };

      // Act
      await _repository!.Save(newUser);
      var saved = await _repository.FindByEmail("bd@test.com");

      // Assert
      Assert.NotNull(saved);
      Assert.Equal("Brand New", saved.Name);
      Assert.True(saved.Id > 0);
    }

    [Fact]
    public async Task Find_ShouldBeAbleToFindActiveUsers()
    {
      // Act
      var active = await _repository!.Find(1);
      var inactive = await _repository!.Find(4);

      // Assert
      Assert.Null(inactive);
      Assert.NotNull(active);
    }

    [Fact]
    public async Task Find_ShouldBeAbleToFindInactiveUsers()
    {
      // Act
      var row = await _repository!.Find(4, false);

      // Assert
      Assert.NotNull(row);
    }

    [Fact]
    public async Task Undelete_ShouldUndeleteUsers()
    {
      // Act
      var row = await _repository!.Find(4, false);
      Assert.NotNull(row);
      await _repository!.UnDelete(row);
      var active = await _repository!.Find(4);

      // Assert
      Assert.NotNull(active);
    }

    [Fact]
    public async Task Delete_ShouldDeleteUsers()
    {
      // Act
      var active = await _repository!.Find(2);
      Assert.NotNull(active);
      await _repository!.Delete(active);
      var inactive = await _repository!.Find(2);

      // Assert
      Assert.Null(inactive);
    }

  }
}