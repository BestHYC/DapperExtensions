using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Dapper.Framework
{
    /// <summary>
    /// 查询参数化解析
    /// </summary>
    public sealed class QueryParameterReduce
    {
        public QuerySQLHelper QueryHelper { get; }
        public DynamicParameters Parameters { get; }
        public QueryParameterReduce()
        {
            QueryHelper = new QuerySQLHelper();
            Parameters = new DynamicParameters();
        }
        public List<ColumnRelevanceMapper> GetQuery()
        {
            return QueryHelper.Query;
        }
        public List<TableRelevanceMapper> GetTable()
        {
            return QueryHelper.Table;
        }
        public ColumnRelevanceMapper GetTop()
        {
            return QueryHelper.Top;
        }
        public List<ColumnRelevanceMapper> GetWhere()
        {
            return QueryHelper.Where;
        }
        public List<String> GetOrderBy()
        {
            return QueryHelper.Orderby;
        }
        public QueryParameterReduce AddTop(Int32 num)
        {
            String name = num.RandromName();
            QueryHelper.AddTop(name);
            Parameters.Add(name, num);
            return this;
        }
        public QueryParameterReduce AddQuery(Expression expression)
        {
            ConvertExpression(expression, QueryHelper.Query);
            return this;
        }
        public QueryParameterReduce AddQuery(Type type, String columnName)
        {
            ColumnRelevanceMapper column = new ColumnRelevanceMapper
            {
                ColumnName = columnName,
                TableName = type
            };
            AddQuery(column);
            return this;
        }
        public QueryParameterReduce AddQuery(ColumnRelevanceMapper column)
        {
            QueryHelper.AddQuery(column);
            return this;
        }
        public QueryParameterReduce AddQueryList(List<ColumnRelevanceMapper> columns)
        {
            QueryHelper.AddQueryList(columns);
            return this;
        }
        /// <summary>
        /// 添加表及表关系
        /// </summary>
        /// <param name="t"></param>
        /// <param name="tableOperator"></param>
        /// <param name="expression"></param>
        /// <returns></returns>
        public QueryParameterReduce AddTable(Type t, TableOperatorEnum tableOperator = TableOperatorEnum.None, Expression expression = null)
        {
            TableRelevanceMapper table = new TableRelevanceMapper(t, tableOperator);
            if (expression != null)
            {
                ConvertExpression(expression, table.ColumnOperator);
            }
            QueryHelper.AddTable(table);
            return this;
        }
        public QueryParameterReduce AddWhere(Expression expression)
        {
            SqlOperatorEnum operatorEnum = SqlOperatorEnum.None;
            if (QueryHelper.WhereCount() != 0)
            {
                operatorEnum = SqlOperatorEnum.And;
            }
            ConvertExpression(expression, QueryHelper.Where, operatorEnum);
            return this;
        }
        private void ConvertExpression(Expression expression, List<ColumnRelevanceMapper> columns, SqlOperatorEnum operatorEnum = SqlOperatorEnum.None)
        {
            ParameterReduce parameterReduce = new ParameterReduce(Parameters, columns);
            if (operatorEnum != SqlOperatorEnum.None)
            {
                parameterReduce.AddOperator(operatorEnum);
            }
            expression.ConvertExpression(parameterReduce);
        }
    }
}
