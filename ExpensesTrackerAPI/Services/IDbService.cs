using System.Linq.Expressions;

namespace ExpensesTrackerAPI.Services
{
    /// <summary>
    /// Contains generic methods for working with data of any datbase entity
    /// </summary>
    public interface IDbService
    {
        /// <summary>
        /// Return a particular entity data from database filtered by a passed-in LINQ query condition
        /// </summary>
        /// <param name="expression">The linq query condition (lambda expression)</param>
        /// <returns>List of results</returns>
        public IQueryable<T> GetByCondition<T>(Expression<Func<T, bool>> expression) where T : class;

        /// <summary>
        /// Return all entries of a particular entity from database
        /// </summary>
        /// <returns>List of results</returns>
        public IQueryable<TEntity> GetAll<TEntity>() where TEntity : class;

        /// <summary>
        /// Updates a database entry
        /// </summary>
        /// <param name="entity">The item to update</param>
        /// <returns></returns>
        public void Update<TEntity>(TEntity entity) where TEntity : class;
    }
}
