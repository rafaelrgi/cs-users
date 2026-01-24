using Microsoft.EntityFrameworkCore;
using Users.Data;
using Users.src.Domain.Contracts;
using Users.src.Domain.Core;
using Users.src.Domain.Entities;

namespace Users.src.Infra.Repositories
{
  public class UserRepository : IUserRepository
  {
    readonly Db _db;

    public UserRepository(Db db)
    {
      _db = db;
    }

    public async Task<Pagination<User>> FindAll(int page, int perPage, string sort, string order)
    {
      var qry = _db.Users
                  .OrderBy(x => EF.Property<object>(x, sort)).ThenBy(x => x.Id)
                  .AsQueryable();

      //pagination
      int totalRecords = await qry.CountAsync();
      if (perPage > 0)
      {
        int skip = page * perPage - perPage;
        qry = qry
                .Skip(skip)
                .Take(perPage);
      }

      //Console.WriteLine(qry.ToQueryString());
      var rows = await qry.AsNoTracking().ToListAsync();

      var result = new Pagination<User>()
      {
        Data = rows,
        RecordCount = totalRecords,
        Page = page,
        PerPage = perPage,
        PageCount = totalRecords / perPage + (rows.Count > 0 ? 1 : 0),
      };

      return result;
    }

    public async Task<User?> Find(int id, bool ignoreDeleted = true)
    {
      var qry = _db.Users.AsQueryable();
      if (!ignoreDeleted)
        qry = qry.IgnoreQueryFilters();

      return await qry.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id);
    }

    public async Task<User?> FindByEmail(string email)
    {
      var row = await _db.Users.AsNoTracking().SingleOrDefaultAsync(x => x.Email == email);
      return row;
    }

    public async Task<User> Save(User user)
    {
      if (user.Id > 0)
        _db.Users.Update(user);
      else
        await _db.Users.AddAsync(user);

      await _db.SaveChangesAsync();
      return user;
    }

    public async Task<bool> Delete(User user)
    {
      user.DeletedAt = DateTime.Now;
      _db.Users.Update(user);
      return (await _db.SaveChangesAsync() > 0);
    }

    public async Task<bool> UnDelete(User user)
    {
      user.DeletedAt = null;
      _db.Users.Update(user);
      return (await _db.SaveChangesAsync() > 0);
    }

  }
}
