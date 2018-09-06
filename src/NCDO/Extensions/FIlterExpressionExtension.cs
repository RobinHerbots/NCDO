using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace NCDO.Extensions
{
    public static class FIlterExpressionExtension
    {
        public static string ToABLFIlter<R>(this Expression<Func<R, bool>> filterExpression)
        {
            return ToString(filterExpression?.Body);
        }

        private static string ToString(Expression expression, bool valueParameter = false)
        {
            BinaryExpression be;
            switch (expression?.NodeType)
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
                    var memberValue = ResolveMember(me)?.ToString();
                    return me.Type == typeof(System.Object) || valueParameter ? $"'{memberValue}'" : memberValue;
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
                    return $"({ToString(be?.Left)}) OR ({ToString(be?.Right)})";
                case ExpressionType.AndAlso:
                    be = expression as BinaryExpression;
                    return $"({ToString(be?.Left)}) AND ({ToString(be?.Right)})";
                case ExpressionType.Not:
                    var notExp = expression as UnaryExpression;
                    return $"NOT ({ToString(notExp?.Operand)})";
                case ExpressionType.Convert:
                    var convertExp = expression as UnaryExpression;
                    return ToString(convertExp?.Operand, true);
                default:
                    throw new NotSupportedException($"ToABLFIlter {expression?.NodeType}");
            }
        }

        private static object ResolveMember(MemberExpression exp)
        {
            object expValue = null;
            switch (exp?.Expression?.NodeType)
            {
                case ExpressionType.MemberAccess:
                    expValue = ResolveMember(exp.Expression as MemberExpression);
                    break;
                case ExpressionType.Constant:
                    expValue = (exp.Expression as ConstantExpression)?.Value;
                    break;
            }

            try
            {
                switch (exp?.Member)
                {
                    case FieldInfo info:
                        expValue = info.GetValue(expValue);
                        break;
                    case PropertyInfo propertyInfo:
                        expValue = propertyInfo.GetValue(expValue);
                        break;
                }
            }
            catch (Exception)
            {

            }

            return expValue;
        }
    }
}
