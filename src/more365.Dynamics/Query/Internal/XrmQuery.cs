using System;
using System.Collections.Generic;

namespace more365.Dynamics.Query
{
    internal class XrmQuery : IXrmQuery, IXrmQueryExpression
    {
        string IXrmQuery.EntityLogicalName => _entityLogicalName;

        IEnumerable<string> IXrmQuery.SelectAttributes => _selectAttributes;

        IEnumerable<XrmQueryOrderBy> IXrmQuery.OrderByAttributes => _orderByAttributes;

        IEnumerable<XrmQueryCondition> IXrmQuery.Conditions => _conditions;

        IEnumerable<XrmQueryJoin> IXrmQuery.Joins => _joins;

        internal IXrmQuery RootQuery;

        private string _entityLogicalName;
        private List<string> _selectAttributes;
        private List<XrmQueryOrderBy> _orderByAttributes;
        private List<XrmQueryCondition> _conditions;
        private List<XrmQueryJoin> _joins;

        public XrmQuery(string entityLogicalName)
        {
            _entityLogicalName = entityLogicalName;
            _selectAttributes = new List<string>();
            _orderByAttributes = new List<XrmQueryOrderBy>();
            _conditions = new List<XrmQueryCondition>();
            _joins = new List<XrmQueryJoin>();
            RootQuery = this;

            _selectAttributes.Add(_entityLogicalName + "id");
        }

        internal XrmQuery(string entityLogicalName, IXrmQuery rootQuery)
            : this(entityLogicalName)
        {
            RootQuery = rootQuery;
        }

        public IXrmQueryExpression Join(string entityName, string attributeToName, string attributeFromName, bool isOuterJoin = false, string joinAlias = "")
        {
            if (string.IsNullOrEmpty(joinAlias))
            {
                joinAlias = entityName;
            }
            var joinQuery = new XrmQuery(entityName, RootQuery);
            var join = new XrmQueryJoin(entityName, attributeToName, attributeFromName, isOuterJoin, joinAlias, joinQuery);
            _joins.Add(join);
            return joinQuery;
        }

        public IXrmQueryExpression OrderBy(string attributeName, bool isDescendingOrder = false)
        {
            _orderByAttributes.Add(new XrmQueryOrderBy(attributeName, isDescendingOrder));
            return this;
        }

        public IXrmQueryExpression Select(params string[] attributeNames)
        {
            _selectAttributes.AddRange(attributeNames);
            return this;
        }

        public IXrmQueryExpression Where(string attributeName, XrmQueryOperator expressionOperator, params object[] values)
        {
            _conditions.Add(new XrmQueryCondition(attributeName, expressionOperator, values));
            return this;
        }

        public IXrmQueryExpression WhereAny(Action<XrmQueryExpressionWhere> or)
        {
            var conditions = new List<XrmQueryCondition>();
            or((string attributeName, XrmQueryOperator expressionOperator, object value) =>
            {
                conditions.Add(new XrmQueryCondition(attributeName, expressionOperator, new object[] { value }));
            });
            _conditions.Add(new XrmQueryCondition(conditions.ToArray()));
            return this;
        }
    }
}