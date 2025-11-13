namespace WayPrecision.Domain.Services
{
    public interface IService<T>
    {
        Task<List<T>> GetAllAsync();

        Task<T?> GetByIdAsync(string guid);

        Task<T?> CreateAsync(T entity);

        Task<T?> UpdateAsync(T entity);

        Task<bool> DeleteAsync(string guid);
    }
}