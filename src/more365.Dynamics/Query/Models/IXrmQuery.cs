using System;
using System.Collections.Generic;

namespace more365.Dynamics.Query
{
    internal interface IXrmQuery
    {
        string EntityLogicalName { get; }

        IEnumerable<string> SelectAttributes { get; }

        IEnumerable<XrmQueryOrderBy> OrderByAttributes { get; }

        IEnumerable<XrmQueryCondition> Conditions { get; }

        IEnumerable<XrmQueryJoin> Joins { get; }
    }

    internal struct XrmQueryOrderBy
    {
        public string AttributeName;
        public bool IsDescendingOrder;

        public XrmQueryOrderBy(string attributeName, bool isDescendingOrder)
        {
            AttributeName = attributeName;
            IsDescendingOrder = isDescendingOrder;
        }
    }

    internal struct XrmQueryCondition
    {
        public string AttributeName;
        public XrmQueryOperator Operator;
        public object[] Values;
        public XrmQueryCondition[] OrConditions;

        public XrmQueryCondition(string attributeName, XrmQueryOperator expressionOperator, object[] values)
        {
            AttributeName = attributeName;
            Operator = expressionOperator;
            Values = values;
            OrConditions = null;
        }

        public XrmQueryCondition(XrmQueryCondition[] orConditions)
        {
            AttributeName = null;
            Operator = XrmQueryOperator.In;
            Values = null;
            OrConditions = orConditions;
        }
    }

    internal struct XrmQueryJoin
    {
        public string JoinAlias;
        public string JoinEntityName;
        public string JoinToAttributeName;
        public string JoinFromAttributeName;
        public bool IsOuterJoin;
        internal IXrmQuery XrmQuery;

        internal XrmQueryJoin(string joinEntityName, string joinAttributeToName, string joinAttributeFromName, bool isOuterJoin, string joinAlias, IXrmQuery xrmQuery)
        {
            JoinAlias = joinAlias;
            JoinEntityName = joinEntityName;
            JoinToAttributeName = joinAttributeToName;
            JoinFromAttributeName = joinAttributeFromName;
            IsOuterJoin = isOuterJoin;
            XrmQuery = xrmQuery;
        }
    }
}