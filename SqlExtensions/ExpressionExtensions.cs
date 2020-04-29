using Dapper;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Dapper.Framework
{
    /// <summary>
    /// 表达式树解析
    /// </summary>
    public static class ExpressionExtensions
    {
        public static void ConvertExpression(this Expression expression,
            in ParameterReduce reduce)
        {
            switch (expression.NodeType)
            {
                case ExpressionType.Lambda:
                    ConvertExpression(((LambdaExpression)expression).Body, in reduce);
                    break;
                case ExpressionType.AndAlso:
                    ConvertExpression(((BinaryExpression)expression).Left, in reduce);
                    reduce.AddOperator(SqlOperatorEnum.And);
                    ConvertExpression(((BinaryExpression)expression).Right, in reduce);
                    break;
                case ExpressionType.OrElse:
                    ConvertExpression(((BinaryExpression)expression).Left, in reduce);
                    reduce.AddOperator(SqlOperatorEnum.Or);
                    ConvertExpression(((BinaryExpression)expression).Right, in reduce);
                    break;
                case ExpressionType.Equal:
                    ConvertExpression(((BinaryExpression)expression).Left, in reduce);
                    reduce.AddOperator(SqlOperatorEnum.Equal);
                    ConvertExpression(((BinaryExpression)expression).Right, in reduce);
                    break;
                case ExpressionType.NotEqual:
                    ConvertExpression(((BinaryExpression)expression).Left, in reduce);
                    reduce.AddOperator(SqlOperatorEnum.NotEqual);
                    ConvertExpression(((BinaryExpression)expression).Right, in reduce);
                    break;
                case ExpressionType.LessThanOrEqual:
                    ConvertExpression(((BinaryExpression)expression).Left, in reduce);
                    reduce.AddOperator(SqlOperatorEnum.LessThanOrEqual);
                    ConvertExpression(((BinaryExpression)expression).Right, in reduce);
                    break;
                case ExpressionType.LessThan:
                    ConvertExpression(((BinaryExpression)expression).Left, in reduce);
                    reduce.AddOperator(SqlOperatorEnum.LessThan);
                    ConvertExpression(((BinaryExpression)expression).Right, in reduce);
                    break;
                case ExpressionType.GreaterThan:
                    ConvertExpression(((BinaryExpression)expression).Left, in reduce);
                    reduce.AddOperator(SqlOperatorEnum.GreaterThan);
                    ConvertExpression(((BinaryExpression)expression).Right, in reduce);
                    break;
                case ExpressionType.GreaterThanOrEqual:
                    ConvertExpression(((BinaryExpression)expression).Left, in reduce);
                    reduce.AddOperator(SqlOperatorEnum.GreaterThanOrEqual);
                    ConvertExpression(((BinaryExpression)expression).Right, in reduce);
                    break;
                case ExpressionType.Constant:
                    ConstantExpression((ConstantExpression)expression, in reduce);
                    break;
                case ExpressionType.MemberAccess:
                    MemberExpression((MemberExpression)expression, in reduce);
                    break;
                case ExpressionType.New:
                    NewExpression((NewExpression)expression, in reduce);
                    break;
                case ExpressionType.Convert:
                    ConvertExpression(((UnaryExpression)expression).Operand, in reduce);
                    break;
                default:
                    throw new ArgumentException("请传递正确sql参数");
            }
        }
        private static void ConstantExpression(ConstantExpression constant, in ParameterReduce reduce)
        {
            String name = constant.RandromName();
            reduce.Add(name, constant.Value);
        }
        private static void NewExpression(NewExpression expression, in ParameterReduce reduce)
        {
            foreach (var item in expression.Arguments)
            {
                MemberExpression memberExp = (MemberExpression)item;
                var member = memberExp.Member as MemberInfo;
                Type type = member.ReflectedType;
                ColumnRelevanceMapper column = new ColumnRelevanceMapper();
                column.TableName = type;
                column.ColumnName = member.Name;
                reduce.Add(column);
            }
        }
        private static void MemberExpression(MemberExpression expression,
            in ParameterReduce reduce)
        {
            var member = expression.Member as MemberInfo;
            Type type = member.ReflectedType;
            //如 int32 i =0;
            //a.id=i 那么会解析出 i result = 1
            if (
                expression.Expression.NodeType == ExpressionType.MemberAccess ||
                expression.Expression.NodeType == ExpressionType.Constant
                )
            {
                var result1 = Expression.Lambda(expression).Compile().DynamicInvoke();
                reduce.Add(member.Name, result1);
                return;
            }
            switch (member.MemberType)
            {
                case MemberTypes.Property:
                    ColumnRelevanceMapper column = new ColumnRelevanceMapper();
                    column.ColumnName = member.Name;
                    column.TableName = type;
                    reduce.Add(column);
                    break;
                case MemberTypes.Field:
                    throw new ArgumentException("请传递正确sql参数,注:只允许属性注入,不允许字段注入");
            }
        }
    }
}
