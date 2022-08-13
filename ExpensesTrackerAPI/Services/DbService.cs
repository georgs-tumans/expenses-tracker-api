using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace ExpensesTrackerAPI.Services
{
    public class DbService : IDbService
    {
        private readonly DbContext _dbContext;
        public DbService(DbContext context)
        {
            _dbContext = context;
        }

        public IQueryable<TEntity> GetByCondition<TEntity>(Expression<Func<TEntity, bool>> expression) where TEntity : class
        {
            try
            {
                return _dbContext.Set<TEntity>().Where(expression).AsNoTracking();
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"[DbService.GetByCondition] Error querying the {typeof(TEntity)} entity - {ex.Message}", ex.ToString());
            }

        }

        public IQueryable<TEntity> GetAll<TEntity>() where TEntity : class
        {
            try
            {
                return _dbContext.Set<TEntity>().AsNoTracking();
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"[DbService.GetAll] Error querying the {typeof(TEntity)} entity - {ex.Message}", ex.ToString());
            }

        }

        public void Update<TEntity>(TEntity entity) where TEntity : class
        {
            try
            {
                _dbContext.Set<TEntity>().Update(entity);
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"[DbService.Update] Error updating the {typeof(TEntity)} entity - {ex.Message}", ex.ToString());
            }
        }

        public void Add<TEntity>(TEntity entity) where TEntity : class
        {
            try
            {
                _dbContext.Set<TEntity>().Add(entity);
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"[DbService.Add] Error adding a new {typeof(TEntity)} entity - {ex.Message}", ex.ToString());
            }
        }

        public void Delete<TEntity>(TEntity entity) where TEntity : class
        {
            try
            {
                _dbContext.Set<TEntity>().Remove(entity);
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"[DbService.Delete] Error deleteing a {typeof(TEntity)} entity - {ex.Message}", ex.ToString());
            }
        }
    }
}
