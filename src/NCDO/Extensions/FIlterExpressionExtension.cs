using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace NCDO.Extensions
{
    public static class FIlterExpressionExtension
    {
        public static string ToABLFIlter<R>(this Expression<Func<R, bool>> filterExpression)
        {
            return ToString(filterExpression.Body);
        }

        private static string ToString(Expression expression)
        {
            switch (expression.NodeType)
            {
                case ExpressionType.Constant:
                    return expression.ToString().Replace('"', '\'');
                case ExpressionType.Equal:
                    var be = expression as BinaryExpression;
                    return $"{ToString(be?.Left)} = {ToString(be?.Right)}";
                case ExpressionType.MemberAccess:
                    return ((MemberExpression)expression).Member.Name;
                default:
                    throw new NotSupportedException($"ToABLFIlter {expression.NodeType}");
            }
        }
    }
}
