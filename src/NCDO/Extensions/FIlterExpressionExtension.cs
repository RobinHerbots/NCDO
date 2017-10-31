using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
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
            BinaryExpression be;
            switch (expression.NodeType)
            {
                case ExpressionType.Call:
                    var mc = expression as MethodCallExpression;
                    switch (mc?.Method.Name)
                    {
                        case "Equals":
                            return $"{ToString(mc?.Object)} = {ToString(mc?.Arguments.FirstOrDefault())}";
                        case "StartsWith":
                            return $"{ToString(mc?.Object)} BEGINS {ToString(mc?.Arguments.FirstOrDefault())}";
                        case "Contains":
                            return $"{ToString(mc?.Object)} MATCHES '*{ToString(mc?.Arguments.FirstOrDefault()).Trim('\'')}*'";
                    }
                    throw new NotSupportedException($"ToABLFIlter {expression.NodeType} {mc?.Method.Name}");
                case ExpressionType.Constant:
                    return expression.ToString().Replace('"', '\'');
                case ExpressionType.Equal:
                    be = expression as BinaryExpression;
                    return $"{ToString(be?.Left)} = {ToString(be?.Right)}";
                case ExpressionType.GreaterThan:
                    be = expression as BinaryExpression;
                    return $"{ToString(be?.Left)} > {ToString(be?.Right)}";
                case ExpressionType.GreaterThanOrEqual:
                    be = expression as BinaryExpression;
                    return $"{ToString(be?.Left)} >= {ToString(be?.Right)}";
                case ExpressionType.MemberAccess:
                    return ((MemberExpression)expression).Member.Name;
                case ExpressionType.NotEqual:
                    be = expression as BinaryExpression;
                    return $"{ToString(be?.Left)} <> {ToString(be?.Right)}";
                case ExpressionType.LessThan:
                    be = expression as BinaryExpression;
                    return $"{ToString(be?.Left)} < {ToString(be?.Right)}";
                case ExpressionType.LessThanOrEqual:
                    be = expression as BinaryExpression;
                    return $"{ToString(be?.Left)} <= {ToString(be?.Right)}";
                case ExpressionType.OrElse:
                    be = expression as BinaryExpression;
                    return $"{ToString(be?.Left)} OR {ToString(be?.Right)}";
                case ExpressionType.AndAlso:
                    be = expression as BinaryExpression;
                    return $"{ToString(be?.Left)} AND {ToString(be?.Right)}";
                default:
                    throw new NotSupportedException($"ToABLFIlter {expression.NodeType}");
            }
        }
    }
}
