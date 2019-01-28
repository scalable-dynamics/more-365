using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace more365.Dynamics.Query
{
    internal class EntityQuery<TEntity> : IXrmQuery, IEntityQueryExpression<TEntity>
        where TEntity : class, new()
    {
        string IXrmQuery.EntityLogicalName => _entityLogicalName;

        IEnumerable<string> IXrmQuery.SelectAttributes => _selectAttributes;

        IEnumerable<XrmQueryOrderBy> IXrmQuery.OrderByAttributes => _orderByAttributes;

        IEnumerable<XrmQueryCondition> IXrmQuery.Conditions => _conditions;

        IEnumerable<XrmQueryJoin> IXrmQuery.Joins => _joins;

        private string _entityLogicalName;
        private List<string> _selectAttributes;
        private List<XrmQueryOrderBy> _orderByAttributes;
        private List<XrmQueryCondition> _conditions;
        private List<XrmQueryJoin> _joins;

        internal IXrmQuery RootQuery;

        public EntityQuery()
        {
            _entityLogicalName = typeof(TEntity).GetEntityLogicalName();
            _selectAttributes = new List<string>();
            _orderByAttributes = new List<XrmQueryOrderBy>();
            _conditions = new List<XrmQueryCondition>();
            _joins = new List<XrmQueryJoin>();
            RootQuery = this;

            _selectAttributes.Add(_entityLogicalName + "id");
        }

        private EntityQuery(IXrmQuery rootQuery)
            : this()
        {
            RootQuery = rootQuery;
        }

        public IEntityQueryExpression<TEntity> Select<TResult>(Expression<Func<TEntity, TResult>> selector)
        {
            _selectAttributes.AddRange(getEntityMembers<TEntity>(selector));
            return this;
        }

        public IEntityQueryExpression<TEntity> Where(Expression<Func<TEntity, bool>> predicate)
        {
            var condition = getQueryCondition(predicate);

            _conditions.Add(condition);
            return this;
        }

        public IEntityQueryExpression<TEntity> WhereAny(Action<EntityQueryExpressionWhere<TEntity>> or)
        {
            var conditions = new List<XrmQueryCondition>();
            or(any =>
            {
                var condition = getQueryCondition(any);
                conditions.Add(new XrmQueryCondition(condition.AttributeName, condition.Operator, condition.Values));
            });
            _conditions.Add(new XrmQueryCondition(conditions.ToArray()));
            return this;
        }

        public IEntityQueryExpression<TEntity> OrderBy<TProperty>(Expression<Func<TEntity, TProperty>> propertySelector, bool isDescendingOrder = false)
        {
            var property = getEntityMembers<TEntity>(propertySelector).Single();
            _orderByAttributes.Add(new XrmQueryOrderBy(property, isDescendingOrder));
            return this;
        }

        public IEntityQueryExpression<TJoin> Join<TJoin>(Expression<Func<TEntity, TJoin>> propertySelector, bool isOuterJoin = false) where TJoin : class, new()
        {
            var joinQuery = new EntityQuery<TJoin>(RootQuery);
            var property = getEntityProperties<TEntity>(propertySelector).Single();
            var toAttribute = typeof(TJoin).GetEntityLogicalName() + "id";
            var fromAttribute = property.Property;
            var joinProperties = typeof(TJoin).GetProperties().Select(p => p.GetPropertyName()).ToArray();
            if (!joinProperties.Contains(fromAttribute))
            {
                fromAttribute = toAttribute;
                toAttribute = property.Property;
            }
            _joins.Add(new XrmQueryJoin(typeof(TJoin).GetEntityLogicalName(), toAttribute, fromAttribute, isOuterJoin, property.Name, joinQuery));
            return joinQuery;
        }

        private XrmQueryCondition getQueryCondition(Expression<Func<TEntity, bool>> predicate)
        {
            var members = getEntityMembers<TEntity>(predicate);
            object[] val = null;
            XrmQueryOperator? expOperator = null;

            Expression expression = predicate.Body;
            var useNotOperator = false;
            if (expression is UnaryExpression notExpression && notExpression.NodeType == ExpressionType.Not)
            {
                expression = notExpression.Operand;
                useNotOperator = true;
            }

            switch (expression.NodeType)
            {
                case ExpressionType.Equal:
                    expOperator = XrmQueryOperator.Equals;
                    break;
                case ExpressionType.NotEqual:
                    expOperator = XrmQueryOperator.NotEquals;
                    break;
                case ExpressionType.GreaterThan:
                    expOperator = XrmQueryOperator.GreaterThan;
                    break;
                case ExpressionType.GreaterThanOrEqual:
                    expOperator = XrmQueryOperator.GreaterThanOrEqual;
                    break;
                case ExpressionType.LessThan:
                    expOperator = XrmQueryOperator.LessThan;
                    break;
                case ExpressionType.LessThanOrEqual:
                    expOperator = XrmQueryOperator.LessThanOrEqual;
                    break;
            }

            if (expression is MemberExpression propMember)
            {
                if (!isEntityMember(propMember))
                {
                    val = new[] { getExpressionValue(propMember) };
                }
                else if (propMember.Member is PropertyInfo propInfo)
                {
                    if (propInfo.PropertyType.IsAssignableFrom(typeof(bool)))
                    {
                        val = new object[] { !useNotOperator };
                    }
                    else
                    {
                        var propDefault = Expression.Default(propInfo.PropertyType);
                        val = new[] { getExpressionValue(propDefault) };
                    }
                }
                if (Nullable.GetUnderlyingType(propMember.Member.DeclaringType) != null && propMember.Member.Name == "HasValue")
                {
                    val = null;
                    if (useNotOperator)
                    {
                        expOperator = XrmQueryOperator.Null;
                    }
                    else
                    {
                        expOperator = XrmQueryOperator.NotNull;
                    }
                }
                else
                {
                    if (useNotOperator)
                    {
                        expOperator = XrmQueryOperator.NotEquals;
                    }
                    else
                    {
                        expOperator = XrmQueryOperator.Equals;
                    }
                }
            }
            else if (expression is BinaryExpression binaryExpression)
            {
                if (binaryExpression.Left is ConstantExpression leftConstant)
                {
                    val = new[] { leftConstant.Value };
                }
                else if (binaryExpression.Right is ConstantExpression rightConstant)
                {
                    val = new[] { rightConstant.Value };
                }
                else if (binaryExpression.Left is MemberExpression leftMember && !isEntityMember(leftMember))
                {
                    val = new[] { getExpressionValue(leftMember) };
                }
                else if (binaryExpression.Right is MemberExpression rightMember && !isEntityMember(rightMember))
                {
                    val = new[] { getExpressionValue(rightMember) };
                }
                else if (getEntityMembers<TEntity>(binaryExpression.Left).Count() == 0 &&
                         (binaryExpression.Left is UnaryExpression ||
                          binaryExpression.Left is MethodCallExpression))
                {
                    val = new[] { getExpressionValue(binaryExpression.Left) };
                }
                else if (getEntityMembers<TEntity>(binaryExpression.Right).Count() == 0 &&
                         (binaryExpression.Right is UnaryExpression ||
                          binaryExpression.Right is MethodCallExpression))
                {
                    val = new[] { getExpressionValue(binaryExpression.Right) };
                }
            }
            else if (expression is MethodCallExpression callExpression)
            {
                Expression arg = null;
                if (callExpression.Object != null)
                {
                    if (callExpression.Object is MemberExpression callMember && !isEntityMember(callMember))
                    {
                        val = new[] { getExpressionValue(callMember) };
                    }
                }
                if (callExpression.Method.Name == "IsNullOrEmpty" || callExpression.Method.Name == "IsNullOrWhiteSpace")
                {
                    if (useNotOperator)
                    {
                        expOperator = XrmQueryOperator.NotNull;
                    }
                    else
                    {
                        expOperator = XrmQueryOperator.Null;
                    }
                }
                else if (callExpression.Method.Name == "Contains")
                {
                    if (callExpression.Arguments.Count == 2)
                    {
                        if (callExpression.Arguments.First() is ConstantExpression arrConstant && arrConstant.Type.IsArray)
                        {
                            val = (object[])arrConstant.Value;
                        }
                        else if (callExpression.Arguments.First() is MemberExpression arrMember && arrMember.Type.IsArray)
                        {
                            var func = Expression.Lambda(arrMember).Compile();
                            val = (object[])func.DynamicInvoke();
                        }
                        if (useNotOperator)
                        {
                            expOperator = XrmQueryOperator.NotIn;
                        }
                        else
                        {
                            expOperator = XrmQueryOperator.In;
                        }
                    }
                    else
                    {
                        arg = callExpression.Arguments.Single();
                        if (useNotOperator)
                        {
                            expOperator = XrmQueryOperator.NotContains;
                        }
                        else
                        {
                            expOperator = XrmQueryOperator.Contains;
                        }
                    }
                }
                else if (callExpression.Method.Name == "StartsWith")
                {
                    if (!useNotOperator)
                    {
                        arg = callExpression.Arguments.Single();
                        expOperator = XrmQueryOperator.StartsWith;
                    }
                }
                else if (callExpression.Method.Name == "EndsWith")
                {
                    if (!useNotOperator)
                    {
                        arg = callExpression.Arguments.Single();
                        expOperator = XrmQueryOperator.EndsWith;
                    }
                }
                else if (callExpression.Method.Name == "Equals")
                {
                    arg = callExpression.Arguments.Single();
                    if (useNotOperator)
                    {
                        expOperator = XrmQueryOperator.NotEquals;
                    }
                    else
                    {
                        expOperator = XrmQueryOperator.Equals;
                    }
                }
                if (arg != null && val == null)
                {
                    if (arg is ConstantExpression argConstant)
                    {
                        val = new[] { argConstant.Value };
                    }
                    else if (arg is MemberExpression argMember && !isEntityMember(argMember))
                    {
                        val = new[] { getExpressionValue(argMember) };
                    }
                }
            }

            if (expOperator == XrmQueryOperator.Equals && (val == null || val.FirstOrDefault() == null))
            {
                expOperator = XrmQueryOperator.Null;
                val = null;
            }
            else if (expOperator == XrmQueryOperator.NotEquals && (val == null || val.FirstOrDefault() == null))
            {
                expOperator = XrmQueryOperator.NotNull;
                val = null;
            }

            if (members.Count() != 1)
            {
                throw new ArgumentNullException("Where expression could not be evaluated: " + predicate, "member");
            }

            if (expOperator == null)
            {
                throw new ArgumentNullException("Where expression could not be evaluated: " + predicate, "operator");
            }

            if ((val == null || val.FirstOrDefault() == null) && expOperator != XrmQueryOperator.Null && expOperator != XrmQueryOperator.NotNull)
            {
                throw new ArgumentException("Where expression could not be evaluated: " + predicate, "values");
            }

            if (val?.First()?.GetType().IsArray == true)
            {
                val = (object[])val.First();
            }

            return new XrmQueryCondition(members.Single(), expOperator.Value, val);
        }

        private bool isEntityMember(MemberExpression member)
        {
            return (typeof(TEntity).IsAssignableFrom(member.Member.DeclaringType) || typeof(TEntity).IsAssignableFrom(Nullable.GetUnderlyingType(member.Member.DeclaringType)) || getEntityMembers<TEntity>(member).Count() > 0);
        }

        private IEnumerable<string> getEntityMembers<T>(Expression expression)
        {
            var visitor = new MemberExpressionAccumulator<T>();
            visitor.Visit(expression);
            return visitor.Members.Select(m => m.GetPropertyName());
        }

        private IEnumerable<(string Name, string Property)> getEntityProperties<T>(Expression expression)
        {
            var visitor = new MemberExpressionAccumulator<T>();
            visitor.Visit(expression);
            return visitor.Members.Select(m => (m.Name, m.GetPropertyName()));
        }

        private object getExpressionValue(Expression expression)
        {
            if (getEntityMembers<TEntity>(expression).Count() > 0)
            {
                throw new ArgumentNullException("Expression cannot contain references to entity (" + typeof(TEntity).Name + "): " + expression, "expression");
            }
            var func = Expression.Lambda(expression).Compile();
            return func.DynamicInvoke();
        }

        private class MemberExpressionAccumulator<T> : ExpressionVisitor
        {
            public List<MemberInfo> Members { get; }

            public MemberExpressionAccumulator()
            {
                Members = new List<MemberInfo>();
            }

            protected override Expression VisitMember(MemberExpression node)
            {
                if (node != null)
                {
                    if (typeof(T).IsAssignableFrom(node.Member.DeclaringType))
                    {
                        Members.Add(node.Member);
                    }
                }
                return base.VisitMember(node);
            }
        }
    }
}
