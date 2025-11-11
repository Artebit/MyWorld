using Microsoft.EntityFrameworkCore;
using MyWorld.Domain.Interfaces.Repositories;
using MyWorld.Infrastructure.Data;

namespace MyWorld.Infrastructure.Repositories;

public class EfRepository<T> : IRepository<T> where T : class
{
    protected readonly AppDbContext _ctx;
    protected readonly DbSet<T> _set;

    public EfRepository(AppDbContext ctx)
    {
        _ctx = ctx;
        _set = ctx.Set<T>();
    }

    public void Add(T entity) => _set.Add(entity);

    public IEnumerable<T> GetAll() => _set.AsNoTracking().ToList();

    public T? GetById(Guid id) => _set.Find(id);

    public void Update(T entity) => _set.Update(entity);

    public void Remove(T entity) => _set.Remove(entity);
}
