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
        /// Return a collection of data - result of a join between two datbase entities
        /// </summary>
        /// <param name="filter1">The linq query condition (lambda expression) of the first joinable entity</param>
        /// <param name="filter2">The linq query condition (lambda expression) of the second joinable entity</param>
        /// <param name="joinField1">First entity's field to join on</param>
        /// <param name="joinField2">Second entity's field to join on</param>
        /// <param name="resultExpression">The expression to create the returnable result type</param>
        /// <returns>List of results</returns>
        public IQueryable<TResult> GetByJoin<TEntity1, TEntity2, TResult>(Expression<Func<TEntity1, bool>> filter1,
                                                                          Expression<Func<TEntity2, bool>> filter2,
                                                                          Expression<Func<TEntity1, int>> joinField1,
                                                                          Expression<Func<TEntity2, int>> joinField2,
                                                                          Expression<Func<TEntity1, TEntity2, TResult>> resultExpression) where TEntity1 : class
                                                                                                                                          where TEntity2 : class
                                                                                                                                          where TResult : class;
        /// <summary>
        /// Updates a database entry
        /// </summary>
        /// <param name="entity">The item to update</param>
        /// <returns></returns>
        public void Update<TEntity>(TEntity entity) where TEntity : class;

        /// <summary>
        /// Adds a new database entry
        /// </summary>
        /// <param name="entity">The item to add</param>
        /// <returns></returns>
        public void Add<TEntity>(TEntity entity) where TEntity : class;

        /// <summary>
        /// Deletes a database entry
        /// </summary>
        /// <param name="entity">The item to delete</param>
        /// <returns></returns>
        public void Delete<TEntity>(TEntity entity) where TEntity : class;
    }
}
