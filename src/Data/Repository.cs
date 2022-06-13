using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Data;

public class Repository : IRepository
{
    private readonly SampleDbContext _context;

    public Repository(SampleDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public IQueryable<TEntity> Get<TEntity>()
        where TEntity : class
    {
        return _context.Set<TEntity>();
    }

    public EntityEntry<TEntity> Update<TEntity>(TEntity entity)
        where TEntity : class
    {
        return _context.Update(entity);
    }

    public Task SaveChangesAsync()
    {
        return _context.SaveChangesAsync();
    }

    public void Remove<TEntity>(TEntity entity)
        where TEntity : class
    {
        _context.Remove(entity);
    }

    public async Task Add<TEntity>(TEntity entity)
        where TEntity : class
    {
        await _context.AddAsync(entity);
    }
}