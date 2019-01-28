using System;
using System.Linq.Expressions;

namespace more365.Dynamics.Query
{
    public delegate void EntityQueryExpressionWhere<TEntity>(Expression<Func<TEntity, bool>> predicate);

    public interface IEntityQueryExpression<TEntity> where TEntity : class, new()
    {
        IEntityQueryExpression<TEntity> Select<TResult>(Expression<Func<TEntity, TResult>> selector);

        IEntityQueryExpression<TEntity> Where(Expression<Func<TEntity, bool>> predicate);

        IEntityQueryExpression<TEntity> WhereAny(Action<EntityQueryExpressionWhere<TEntity>> or);

        IEntityQueryExpression<TEntity> OrderBy<TProperty>(Expression<Func<TEntity, TProperty>> propertySelector, bool isDescendingOrder = false);

        IEntityQueryExpression<TJoin> Join<TJoin>(Expression<Func<TEntity, TJoin>> propertySelector, bool isOuterJoin = false) where TJoin : class, new();
    }
}