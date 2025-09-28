using System.Linq.Expressions;

namespace MyApp.Repositories.Interfaces
{
    public interface IGenericRepository<T> where T : class
    {
        Task<IEnumerable<T>> GetAllAsync();
        Task<T?> GetByIdAsync(int id);
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);

        Task AddAsync(T entity);
        Task UpdateAsync(T entity);

        // Updated return type
        Task<bool> DeleteAsync(T entity);

        void Remove(T entity);

        Task SaveChangesAsync();
        IQueryable<T> Query();
    }
}
