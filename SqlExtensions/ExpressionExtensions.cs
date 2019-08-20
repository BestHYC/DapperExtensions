using Dapper;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Dapper.Framework.SqlExtensions
{
    /// <summary>
    /// 表达式树参数解析
    /// </summary>
    public sealed class ParameterReduce
    {
        public ParameterReduce():this(new DynamicParameters())
        {

        }
        public ParameterReduce(DynamicParameters parameters):this(parameters, new List<ColumnRelevanceMapper>())
        {
        }
        public ParameterReduce(DynamicParameters parameters, List<ColumnRelevanceMapper> columns)
        {
            Parameters = parameters;
            ReduceSql = columns;
        }
        /// <summary>
        /// 参数获取 @key对应的key值保存,有2种,一种是new {key = 1}模式, 一种是{"key", "1"};
        /// </summary>
        public DynamicParameters Parameters { get; set; }
        /// <summary>
        /// 限制性条件集合
        /// </summary>
        public List<ColumnRelevanceMapper> ReduceSql { get; }
        private SqlOperatorEnum _sqlOperatorEnum = SqlOperatorEnum.None;
        /// <summary>
        /// 添加字段映射
        /// </summary>
        /// <param name="column"></param>
        public void Add(ColumnRelevanceMapper column)
        {
            AddColumn(column);
        }
        /// <summary>
        /// 添加操作符号
        /// </summary>
        /// <param name="SqlOperatorEnum"></param>
        public void AddOperator(SqlOperatorEnum SqlOperatorEnum)
        {
            this._sqlOperatorEnum = SqlOperatorEnum;
        }
        /// <summary>
        /// 添加属性名
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public void Add(String name, Object value)
        {
            ColumnRelevanceMapper column = new ColumnRelevanceMapper();
            if(value != null)
            {
                column.ColumnName = $"{name}";
            }
            Parameters.Add(name, value);
            //添加@name的模式
            AddColumn(column);
        }
        /// <summary>
        /// 每次的行操作都会判断前面的执行方式
        /// </summary>
        /// <param name="column"></param>
        private void AddColumn(ColumnRelevanceMapper column)
        {
            if (_sqlOperatorEnum != SqlOperatorEnum.None)
            {
                column.SqlOperatorEnum = _sqlOperatorEnum;
                //初始化sql操作
                _sqlOperatorEnum = SqlOperatorEnum.None;
            }
            ReduceSql.Add(column);
        }
    }
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
                    reduce.AddOperator(SqlOperatorEnum.LessThan);
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
            for (Int32 i =0; i < expression.Arguments.Count; i++)
            {
                MemberExpression memberExp = (MemberExpression)expression.Arguments[i];
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
