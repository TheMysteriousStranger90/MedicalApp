using Medical.GrpcService.Context;
using Medical.GrpcService.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Medical.GrpcService.Repositories;

public class GenericRepository<T> : IGenericRepository<T> where T : class
{
    protected readonly ApplicationDbContext _context;

    public GenericRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<T?> GetByIdAsync(string id)
    {
        return await _context.Set<T>().FindAsync(id);
    }

    public async Task<IEnumerable<T>> GetAllAsync()
    {
        return await _context.Set<T>().ToListAsync();
    }

    public async Task<bool> AddAsync(T entity)
    {
        await _context.Set<T>().AddAsync(entity);
        return true;
    }

    public async Task<bool> UpdateAsync(T entity)
    {
        _context.Set<T>().Update(entity);
        return true;
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var entity = await GetByIdAsync(id);
        if (entity == null) return false;
        _context.Set<T>().Remove(entity);
        return true;
    }

    public async Task<bool> Exists(string id)
    {
        var entity = await GetByIdAsync(id);
        return entity != null;
    }
}