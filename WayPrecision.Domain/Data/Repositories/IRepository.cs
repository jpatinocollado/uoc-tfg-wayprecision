using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WayPrecision.Domain.Data.Repositories
{
    public interface IRepository<T> where T : class, new()
    {
        Task<List<T>> GetAllAsync();

        Task<T> GetByIdAsync(string guid);

        Task<int> InsertAsync(T entity);

        Task<int> UpdateAsync(T entity);

        Task<int> DeleteAsync(T entity);
    }
}