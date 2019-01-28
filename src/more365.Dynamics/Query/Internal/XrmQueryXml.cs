using System;
using System.Linq;
using System.Text;

namespace more365.Dynamics.Query
{
    internal static class XrmQueryXml
    {
        public static string ToFetchUrl(this IXrmQuery xrmQuery, string entitySetName, int? maxRecordCount = null)
        {
            var xml = xrmQuery.ToFetchXml(maxRecordCount, true);
            return $"/{entitySetName}?fetchXml={Uri.EscapeDataString(xml)}";
        }

        public static string ToFetchXml(this IXrmQuery xrmQuery, int? maxRecordCount = null, bool distinctResults = false)
        {
            var xml = new StringBuilder();

            xml.Append("<fetch mapping=\"logical\"");

            if (distinctResults)
            {
                xml.Append(" distinct=\"true\"");
            }
            if (maxRecordCount.HasValue)
            {
                xml.Append($" count=\"{maxRecordCount}\"");
            }

            xml.Append(">");
            xml.Append($"<entity name=\"{xrmQuery.EntityLogicalName}\">");
            xml.Append(getQueryXml(xrmQuery));
            xml.Append("</entity>");
            xml.Append("</fetch>");

            return xml.ToString();
        }

        private static string getQueryXml(IXrmQuery query)
        {
            var xml = new StringBuilder();

            foreach (var attribute in query.SelectAttributes.Distinct())
            {
                xml.Append($"<attribute name=\"{attribute}\" />");
            }
            foreach (var orderBy in query.OrderByAttributes)
            {
                xml.Append($"<order attribute=\"{orderBy.AttributeName}\" descending=\"{orderBy.IsDescendingOrder.ToString().ToLower()}\" />");
            }
            if (query.Conditions.Count() > 0)
            {
                var filters = new StringBuilder();
                var hasOrCondition = query.Conditions.Any(c => c.OrConditions?.Length > 0);
                if (hasOrCondition)
                {
                    filters.Append("<filter type=\"and\">");
                }
                filters.Append("<filter type=\"and\">");
                foreach (var condition in query.Conditions)
                {
                    if (condition.OrConditions?.Length > 0)
                    {
                        filters.Append("</filter>");
                        filters.Append("<filter type=\"or\">");
                        foreach (var orCondition in condition.OrConditions)
                        {
                            filters.Append(getConditionXml(orCondition));
                        }
                        filters.Append("</filter>");
                        filters.Append("<filter type=\"and\">");
                    }
                    else
                    {
                        filters.Append(getConditionXml(condition));
                    }
                }
                filters.Append("</filter>");
                if (hasOrCondition)
                {
                    filters.Append("</filter>");
                }
                var skipNextFilter = false;
                var filterXmlLines = filters.ToString().Split('\n');
                for (var i = 0; i < filterXmlLines.Length; i++)
                {
                    if (i < filterXmlLines.Length - 1 && filterXmlLines[i].Contains("<filter") && filterXmlLines[i + 1].Contains("/filter>"))
                    {
                        skipNextFilter = true;
                    }
                    else if (!skipNextFilter)
                    {
                        xml.Append(filterXmlLines[i]);
                    }
                    else
                    {
                        skipNextFilter = false;
                    }
                }
            }
            if (query.Joins.Count() > 0)
            {
                foreach (var join in query.Joins)
                {
                    xml.Append($"<link-entity name=\"{join.JoinEntityName}\" alias=\"{join.JoinAlias}\" from=\"{join.JoinFromAttributeName}\" to=\"{join.JoinToAttributeName}\" link-type=\"{(join.IsOuterJoin ? "outer" : "inner")}\">");
                    xml.Append(getQueryXml(join.XrmQuery));
                    xml.Append("</link-entity>");
                }
            }

            return xml.ToString();
        }

        private static string getConditionXml(XrmQueryCondition condition)
        {
            var xml = new StringBuilder();
            if (condition.AttributeName.Contains('.'))
            {
                xml.Append($"<condition attribute=\"{condition.AttributeName.Split('.')[1]}\" entityname=\"{condition.AttributeName.Split('.')[0]}\" operator=\"{getOperator(condition.Operator)}\"");
            }
            else
            {
                xml.Append($"<condition attribute=\"{condition.AttributeName}\" operator=\"{getOperator(condition.Operator)}\"");
            }
            if (condition.Values?.Length > 0)
            {
                if (condition.Operator == XrmQueryOperator.In || condition.Operator == XrmQueryOperator.NotIn)
                {
                    xml.Append(">");
                    foreach (var value in condition.Values)
                    {
                        xml.Append($"<value>{escapeXml(value?.ToString())}</value>");
                    }
                    xml.Append("</condition>");
                }
                else
                {
                    xml.Append($" value=\"{escapeXml(condition.Values[0]?.ToString())}\" />");
                }
            }
            else
            {
                xml.Append($" />");
            }
            return xml.ToString();
        }

        private static string getOperator(XrmQueryOperator expressionOperator)
        {
            switch (expressionOperator)
            {
                case XrmQueryOperator.Contains: return "like";
                case XrmQueryOperator.StartsWith: return "begins-with";
                case XrmQueryOperator.Equals: return "eq";
                case XrmQueryOperator.NotEquals: return "neq";
                case XrmQueryOperator.GreaterThan: return "gt";
                case XrmQueryOperator.LessThan: return "lt";
                case XrmQueryOperator.In: return "in";
                case XrmQueryOperator.NotIn: return "not-in";
                case XrmQueryOperator.OnOrBefore: return "on-or-before";
                case XrmQueryOperator.OnOrAfter: return "on-or-after";
                case XrmQueryOperator.Null: return "null";
                case XrmQueryOperator.NotNull: return "not-null";
                case XrmQueryOperator.NotContains: return "not-like";
                case XrmQueryOperator.EndsWith: return "like";
                case XrmQueryOperator.GreaterThanOrEqual: return "ge";
                case XrmQueryOperator.LessThanOrEqual: return "le";
                case XrmQueryOperator.IsCurrentUser: return "eq-userid";
                case XrmQueryOperator.IsCurrentTeam: return "eq-userteams";
                case XrmQueryOperator.IsNotCurrentUser: return "ne-userid";
                case XrmQueryOperator.IsNotCurrentTeam: return "ne-userteams";
                default:
                    throw new Exception("ExpressionOperator error: unsupported operator (" + expressionOperator + ")");
            }
        }

        private static string escapeXml(string text)
        {
            return text
                       .Replace("&", "&amp;")
                       .Replace("\"", "&quot;")
                       .Replace("'", "&apos;")
                       .Replace("<", "&lt;")
                       .Replace(">", "&gt;");
        }
    }
}