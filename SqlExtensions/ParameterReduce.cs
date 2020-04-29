using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Dapper.Framework
{
    /// <summary>
    /// 表达式树参数解析
    /// </summary>
    public class ParameterReduce
    {
        /// <summary>
        /// 参数获取,完全依照dapper做参数封装
        /// 
        /// dapper中@key对应的key值保存,有2种,一种是new {key = 1}模式, 一种是{"key", "1"};
        /// </summary>
        public DynamicParameters Parameters { get; protected set; }
        /// <summary>
        /// 限制性sql集合
        /// 如 a.id=1那么会生成 a.id=@id 
        /// Parameters中会产生一个id=1 或者是 {"id",1}的对象
        /// </summary>
        public List<ColumnRelevanceMapper> ReduceSql { get; protected set; }
        /// <summary>
        /// 解析固定操作
        /// </summary>
        private SqlOperatorEnum _sqlOperatorEnum = SqlOperatorEnum.None;
        public ParameterReduce() : this(new DynamicParameters())
        {

        }
        public ParameterReduce(DynamicParameters parameters) : this(parameters, new List<ColumnRelevanceMapper>())
        {

        }
        public ParameterReduce(DynamicParameters parameters, List<ColumnRelevanceMapper> columns)
        {
            Parameters = parameters;
            ReduceSql = columns;
        }
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
        public void AddParameters(String name, Object value)
        {
            if(value == null)
            {
                throw new Exception("参数化不应为null");
            }
            Parameters.Add(name, value);
        }
        /// <summary>
        /// 添加属性名,注意,此处是添加首先解析的字段
        /// 如 a.id = b.id 那么此处解析 a.id
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public void Add(String name, Object value)
        {
            ColumnRelevanceMapper column = new ColumnRelevanceMapper();
            if (value != null)
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
        protected virtual void AddColumn(ColumnRelevanceMapper column)
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
        public QueryParameterReduce AddTable(Type t, TableOperatorEnum tableOperator=TableOperatorEnum.None, Expression expression = null)
        {
            TableRelevanceMapper table = new TableRelevanceMapper(t, tableOperator);
            if(expression != null)
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
            if(operatorEnum != SqlOperatorEnum.None)
            {
                parameterReduce.AddOperator(operatorEnum);
            }
            expression.ConvertExpression(parameterReduce);
        }
    }
    /// <summary>
    /// Batch数据解析
    /// 因为包含Head与Where,所以只能通过Wrapper包含
    /// </summary>
    public sealed class BatchParameterReduce
    {
        public RenewalSqlHelper RenewalHelper { get; }
        public DynamicParameters Parameters { get; }
        public BatchParameterReduce()
        {
            RenewalHelper = new RenewalSqlHelper();
            Parameters = new DynamicParameters();
        }
        public List<ColumnRelevanceMapper> GetHeader()
        {
            return RenewalHelper.Header;
        }
        public TableRelevanceMapper GetTable()
        {
            return RenewalHelper.Table;
        }
        public List<ColumnRelevanceMapper> GetWhere()
        {
            return RenewalHelper.Where;
        }
        public BatchParameterReduce AddTable(Type t, TableOperatorEnum tableOperator)
        {
            RenewalHelper.Table.TableName = t;
            RenewalHelper.Table.TableOperatorEnum = tableOperator;
            return this;
        }
        public BatchParameterReduce AddHeader(String header, Type t = null)
        {
            if (!String.IsNullOrEmpty(header))
            {
                RenewalHelper.AddHeader(
                    new ColumnRelevanceMapper() { ColumnName = header, TableName = t });
            }
            return this;
        }
        public BatchParameterReduce AddHeader(Expression expression)
        {
            ConvertExpression(expression, RenewalHelper.Header);
            return this;
        }
        public BatchParameterReduce AddWhere(ColumnRelevanceMapper column)
        {
            RenewalHelper.AddWhere(column);
            return this;
        }
        public BatchParameterReduce AddWhere(Expression expression)
        {
            SqlOperatorEnum operatorEnum = SqlOperatorEnum.None;
            if (RenewalHelper.WhereCount() != 0)
            {
                operatorEnum = SqlOperatorEnum.And;
            }
            ConvertExpression(expression, RenewalHelper.Where, operatorEnum);
            return this;
        }
        public BatchParameterReduce AddParameters(String name, Object value)
        {
            if (value == null)
            {
                throw new Exception("参数化不应为null");
            }
            Parameters.Add(name, value);
            return this;
        }
        public BatchParameterReduce AddDynamicParams(Object obj)
        {
            Parameters.AddDynamicParams(obj);
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
