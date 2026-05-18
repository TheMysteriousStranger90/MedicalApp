namespace Medical.GrpcService.Repositories.Interfaces;

public interface IGenericRepository<T> where T : class
{
    public Task<T?> GetByIdAsync(string id);
    public Task<IEnumerable<T>> GetAllAsync();
    public Task<bool> AddAsync(T entity);
    public Task<bool> UpdateAsync(T entity);
    public Task<bool> DeleteAsync(string id);
    public Task<bool> Exists(string id);
}
