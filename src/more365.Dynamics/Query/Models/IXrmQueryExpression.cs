using System;

namespace more365.Dynamics.Query
{
    public delegate void XrmQueryExpressionWhere(string attributeName, XrmQueryOperator expressionOperator, object value);

    public interface IXrmQueryExpression
    {
        IXrmQueryExpression Select(params string[] attributeNames);

        IXrmQueryExpression Where(string attributeName, XrmQueryOperator expressionOperator, params object[] values);

        IXrmQueryExpression WhereAny(Action<XrmQueryExpressionWhere> or);

        IXrmQueryExpression OrderBy(string attributeName, bool isDescendingOrder = false);

        IXrmQueryExpression Join(string entityName, string attributeToName, string attributeFromName = "", bool isOuterJoin = false, string joinAlias = "");
    }

    public enum XrmQueryOperator
    {
        Contains,
        NotContains,
        StartsWith,
        EndsWith,
        Equals,
        NotEquals,
        GreaterThan,
        GreaterThanOrEqual,
        LessThan,
        LessThanOrEqual,
        In,
        NotIn,
        OnOrBefore,
        OnOrAfter,
        Null,
        NotNull,
        IsCurrentUser,
        IsCurrentTeam,
        IsNotCurrentUser,
        IsNotCurrentTeam
    }
}