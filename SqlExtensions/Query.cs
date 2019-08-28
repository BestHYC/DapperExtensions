using Dapper;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Dapper.Framework.SqlExtensions
{
    /// <summary>
    /// 简单粗暴,不用反射,表达式树
    /// </summary>
    public sealed class QuerySQLHelper
    {
        public ColumnRelevanceMapper Top { get; private set; }
        /// <summary>
        /// 保存查询字段,目前不可做重复查询,但是后期需支持别名
        /// </summary>
        public List<ColumnRelevanceMapper> Query { get; } = new List<ColumnRelevanceMapper>();
        public List<TableRelevanceMapper> Table { get; } = new List<TableRelevanceMapper>();
        public List<ColumnRelevanceMapper> Where { get; } = new List<ColumnRelevanceMapper>();
        public List<String> Orderby { get; } = new List<String>();
        private Object _lock = new object();
        /// <summary>
        /// 添加所有的查询字段,保存表名及对应的字段名
        /// </summary>
        /// <param name="type">类型</param>
        /// <param name="columnName">字段名</param>
        /// <returns></returns>
        public QuerySQLHelper AddQuery(Type type, String columnName)
        {
            lock (_lock)
            {
                ColumnRelevanceMapper column = new ColumnRelevanceMapper
                {
                    ColumnName = columnName,
                    TableName = type
                };
                Query.Add(column);
            }
            return this;
        }
        public QuerySQLHelper AddTop(String name)
        {
            Top = new ColumnRelevanceMapper()
            {
                ColumnName = name,
                SqlOperatorEnum = SqlOperatorEnum.Top
            };
            return this;
        }
        public QuerySQLHelper AddQuery(ColumnRelevanceMapper column)
        {
            lock (_lock)
            {
                Query.Add(column);
            }
            return this;
        }
        public QuerySQLHelper AddQueryList(Queue<ColumnRelevanceMapper> columns)
        {
            foreach(var item in columns)
            {
                Query.Add(item);
            }
            return this;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="tablename"></param>
        /// <returns></returns>
        public QuerySQLHelper AddTable(TableRelevanceMapper table)
        {
            if (table != null)
            {
                Table.Add(table);
            }
            return this;
        }
        public QuerySQLHelper AddWhere(ColumnRelevanceMapper column)
        {
            if (column != null)
            {
                Where.Add(column);
            }
            return this;
        }
        public QuerySQLHelper AddOrder(String orderby)
        {
            if (!String.IsNullOrEmpty(orderby))
            {
                Orderby.Add(orderby);
            }
            return this;
        }
    }
    /// <summary>
    /// 实现sql查询实现
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class Query<T> : IExecute where T :IEntity
    {
        protected QuerySQLHelper _querySqlHelper;
        protected DynamicParameters _querySqlParam = new DynamicParameters();
        /// <summary>
        /// 当前类型的对应的sql表字段
        /// </summary>
        private Dictionary<String, String> ValuePairs;
        private Object _lock = new object();
        static Query()
        {
            EntityTableMapper.Add(typeof(T));
        }
        protected Query()
        {
            Type t = typeof(T);
            ValuePairs = EntityTableMapper.GetColumns(t);
            _querySqlHelper = new QuerySQLHelper();
            _querySqlHelper.AddTable(new TableRelevanceMapper() { TableName = t });
        }
        /// <summary>
        /// 如果ColumnRelevanceMapper中没有table表名,就说明他是单独的一个执行sql
        /// </summary>
        /// <param name="where"></param>
        /// <returns></returns>
        public Query<T> Count(Expression<Func<T, Boolean>> where)
        {
            _querySqlHelper.AddQuery(new ColumnRelevanceMapper() { ColumnName="count(*)" });
            Where(where);
            return this;
        }
        public Query<T> Select()
        {
            Type t = typeof(T);
            foreach(var item in ValuePairs)
            {
                _querySqlHelper.AddQuery(t, item.Key);
            }
            return this;
        }
        public Query<T> Select(Expression<Func<T, Object>> select)
        {
            if (select.Body.NodeType != ExpressionType.New) throw new Exception("选择性查询请传new{}的表达式");
            ParameterReduce reduce = new ParameterReduce(_querySqlParam, _querySqlHelper.Query);
            select.ConvertExpression(reduce);
            return this;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <param name="foreign"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        public Query<T> Inner<T1>(Expression<Func<T, T1, Boolean>> foreign, Expression<Func<T1, Object>> query = null)
        {
            SetQueryOtherTable(query);
            Type t = typeof(T1);
            TableRelevanceMapper table = new TableRelevanceMapper();
            table.TableName = t;
            table.TableOperatorEnum = TableOperatorEnum.InnerJoin;
            ParameterReduce reduce = new ParameterReduce(_querySqlParam, table.ColumnOperator);
            foreign.ConvertExpression(reduce);
            _querySqlHelper.AddTable(table);
            return this;
        }
        public Query<T> Left<T1>(Expression<Func<T1, Object>> foreign, Expression<Func<T1, Object>> query = null)
        {
            SetQueryOtherTable(query);
            Type t = typeof(T1);
            TableRelevanceMapper table = new TableRelevanceMapper();
            table.TableName = t;
            table.TableOperatorEnum = TableOperatorEnum.LeftJoin;
            ParameterReduce reduce = new ParameterReduce(_querySqlParam, table.ColumnOperator);
            foreign.ConvertExpression(reduce);
            _querySqlHelper.AddTable(table);
            return this;
        }
        /// <summary>
        /// 通过限定条件生成sql,非特定生成sql,不建议使用
        /// </summary>
        /// <param name="expressions"></param>
        /// <returns></returns>
        public Query<T> Where(Expression<Func<T, Object>> expressions)
        {
            ParameterReduce reduce = new ParameterReduce();
            expressions.ConvertExpression(reduce);
            Type t = typeof(T);
            foreach (var item in reduce.ReduceSql)
            {
                ColumnRelevanceMapper left = new ColumnRelevanceMapper();
                ColumnRelevanceMapper right = new ColumnRelevanceMapper();
                if(_querySqlHelper.Where.Count != 0)
                {
                    left.SqlOperatorEnum = SqlOperatorEnum.And;
                }
                left.TableName = t;
                left.ColumnName = item.ColumnName;
                right.SqlOperatorEnum = SqlOperatorEnum.Equal;
                right.ColumnName = item.ColumnName;
                _querySqlHelper.AddWhere(left);
                _querySqlHelper.AddWhere(right);
            }
            return this;
        }
        public Query<T> Where(Expression<Func<T, Boolean>> expressions)
        {
            ParameterReduce reduce = new ParameterReduce(_querySqlParam, _querySqlHelper.Where);
            if (_querySqlHelper.Where.Count != 0)
            {
                reduce.AddOperator(SqlOperatorEnum.And);
            }
            expressions.ConvertExpression(reduce);
            return this;
        }
        public Query<T> Where<T1>(Expression<Func<T, T1, Boolean>> expressions)
        {
            ParameterReduce reduce = new ParameterReduce(_querySqlParam, _querySqlHelper.Where);
            if (_querySqlHelper.Where.Count != 0)
            {
                reduce.AddOperator(SqlOperatorEnum.And);
            }
            expressions.ConvertExpression(reduce);
            return this;
        }
        public Query<T> Top(int num)
        {
            String name = num.RandromName();
            _querySqlHelper.AddTop(name);
            _querySqlParam.Add(name, num);
            return this;
        }

        private void SetQueryOtherTable<T1>(Expression<Func<T1, Object>> query = null)
        {
            lock (_lock)
            {
                Type tablename = typeof(T1);
                var valuePairs = EntityTableMapper.GetColumns(tablename);
                if (query == null)
                {
                    foreach(var item in valuePairs)
                    {
                        _querySqlHelper.AddQuery(tablename, item.Key);
                    }
                }
                else
                {
                    NewExpression newExpression = (NewExpression)query.Body;
                    ParameterReduce reduce = new ParameterReduce(_querySqlParam, _querySqlHelper.Query);
                    query.ConvertExpression(reduce);
                }
            }
        }
        public abstract IBatch End();
    }
}
