using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace WayPrecision.Domain.Services
{
    public interface IService<T> where T : class
    {
        Task<List<T>> GetAllAsync();

        Task<T?> GetByIdAsync(string guid);

        Task<T?> CreateAsync(T? entity);

        Task<T?> UpdateAsync(T? entity);

        Task<bool> DeleteAsync(string guid);
    }
}