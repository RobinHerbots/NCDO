using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Reflection;
using System.Text;

namespace NCDO.Extensions
{
    public static class FIlterExpressionExtension
    {
        public static string ToABLFIlter<R>(this Expression<Func<R, bool>> filterExpression)
        {
            return ToString(filterExpression.Body);
        }

        private static string ToString(Expression expression, bool valueParameter = false)
        {
            BinaryExpression be;
            switch (expression.NodeType)
            {
                case ExpressionType.Call:
                    var mc = expression as MethodCallExpression;
                    switch (mc?.Method.Name)
                    {
                        case "Equals":
                            return mc?.Object != null ? $"{ToString(mc?.Object)} = {ToString(mc?.Arguments.FirstOrDefault(), true)}" : $"{ToString(mc?.Arguments[0])} = {ToString(mc?.Arguments[1], true)}";
                        case "StartsWith":
                            return $"{ToString(mc?.Object)} BEGINS {ToString(mc?.Arguments.FirstOrDefault(), true)}";
                        case "Contains":
                            return $"{ToString(mc?.Object)} MATCHES '*{ToString(mc?.Arguments.FirstOrDefault(), true).Trim('\'')}*'";
                    }
                    throw new NotSupportedException($"ToABLFIlter {expression.NodeType} {mc?.Method.Name}");
                case ExpressionType.Constant:
                    var constantValue = expression.ToString().Replace("\"", valueParameter ? "'" : string.Empty);
                    return constantValue == "null" ? "''" : constantValue;
                case ExpressionType.Equal:
                    be = expression as BinaryExpression;
                    return $"{ToString(be?.Left)} = {ToString(be?.Right, true)}";
                case ExpressionType.GreaterThan:
                    be = expression as BinaryExpression;
                    return $"{ToString(be?.Left)} > {ToString(be?.Right, true)}";
                case ExpressionType.GreaterThanOrEqual:
                    be = expression as BinaryExpression;
                    return $"{ToString(be?.Left)} >= {ToString(be?.Right, true)}";
                case ExpressionType.MemberAccess:
                    var me = expression as MemberExpression;
                    if (me?.Expression != null && me.Expression.NodeType == ExpressionType.Parameter)
                    {
                        return me.Member.Name.Replace("\"", valueParameter ? "'" : string.Empty);
                    }
                    switch (me?.Member)
                    {
                        case FieldInfo _:
                            var fieldValue = ((FieldInfo)me.Member).GetValue(ResolveMember(me)).ToString();
                            return valueParameter ? $"'{fieldValue}'" : fieldValue;
                        case PropertyInfo _:
                            var propValue = ((PropertyInfo)me.Member).GetValue(ResolveMember(me)).ToString();
                            return valueParameter ? $"'{propValue}'" : propValue;
                    }
                    throw new NotSupportedException($"ToABLFIlter {expression.NodeType}");
                case ExpressionType.NotEqual:
                    be = expression as BinaryExpression;
                    return $"{ToString(be?.Left)} <> {ToString(be?.Right, true)}";
                case ExpressionType.LessThan:
                    be = expression as BinaryExpression;
                    return $"{ToString(be?.Left)} < {ToString(be?.Right, true)}";
                case ExpressionType.LessThanOrEqual:
                    be = expression as BinaryExpression;
                    return $"{ToString(be?.Left)} <= {ToString(be?.Right, true)}";
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

        private static object ResolveMember(MemberExpression exp, bool memberResolve = false)
        {
            switch (exp?.Expression?.NodeType)
            {
                case ExpressionType.MemberAccess:
                    return ResolveMember(exp.Expression as MemberExpression, true);
                case ExpressionType.Constant:
                    if (memberResolve)
                    {
                        if (exp.Member is FieldInfo info)
                            return info.GetValue((exp.Expression as ConstantExpression)?.Value);
                        if (exp.Member is PropertyInfo propertyInfo)
                            return propertyInfo.GetValue((exp.Expression as ConstantExpression)?.Value);
                    }
                    return (exp.Expression as ConstantExpression)?.Value;
            }
            return exp;
        }
    }
}
