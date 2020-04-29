using Dapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Dapper.Framework
{
    /// <summary>
    /// 简单粗暴,不用反射及表达式树
    /// 并且记录当前解析的字段及顺序,在反解析时候直接转换即可
    /// </summary>
    public sealed class QuerySQLHelper
    {
        public QuerySQLHelper()
        {
            Query = new List<ColumnRelevanceMapper>();
            Top = new ColumnRelevanceMapper();
            Table = new List<TableRelevanceMapper>();
            Where = new List<ColumnRelevanceMapper>();
            Orderby = new List<String>();
        }
        public ColumnRelevanceMapper Top { get;}
        /// <summary>
        /// 保存查询字段,目前不可做重复查询比如 
        /// 后期支持查询的时候 通过别名查询字段
        /// 自连接 from a as a1, a as a2
        /// 目前是通过表名 a.id 而不是a1.id来区分,所以不适合重复查询字段
        /// </summary>
        public List<ColumnRelevanceMapper> Query { get; }
        /// <summary>
        /// 
        /// </summary>
        public List<TableRelevanceMapper> Table { get; }
        /// <summary>
        /// 
        /// </summary>
        public List<ColumnRelevanceMapper> Where { get; }
        public List<String> Orderby { get; }
        private Object _lock = new object();
        /// <summary>
        /// 添加所有的查询字段,保存表名及对应的字段名
        /// </summary>
        /// <param name="type">类型</param>
        /// <param name="columnName">字段名</param>
        /// <returns></returns>
        public QuerySQLHelper AddTop(String name)
        {
            Top.ColumnName = name;
            Top.SqlOperatorEnum = SqlOperatorEnum.Top;
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
        public QuerySQLHelper AddQueryList(List<ColumnRelevanceMapper> columns)
        {
            Query.AddRange(columns);
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
        public QuerySQLHelper AddWhere(List<ColumnRelevanceMapper> columns)
        {
            if (columns != null)
            {
                Where.AddRange(columns);
            }
            return this;
        }
        public Int32 WhereCount()
        {
            return Where.Count();
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
        protected QueryParameterReduce Reduce;
        /// <summary>
        /// 当前类型的对应的sql表字段
        /// </summary>
        private Dictionary<String, EntityPropertyValue> ValuePairs;
        private Object _lock = new object();
        static Query()
        {
            EntityTableMapper.Add(typeof(T));
        }
        protected Query()
        {
            Type t = typeof(T);
            ValuePairs = EntityTableMapper.GetColumns(t);
            Reduce = new QueryParameterReduce();
            Reduce.AddTable(t);
        }
        /// <summary>
        /// 如果ColumnRelevanceMapper中没有table表名,就说明他是单独的一个执行sql
        /// </summary>
        /// <param name="where"></param>
        /// <returns></returns>
        public Query<T> Count(Expression<Func<T, Boolean>> where)
        {
            Reduce.AddQuery(new ColumnRelevanceMapper() { ColumnName=" count(1) " });
            Where(where);
            return this;
        }
        public Query<T> Select()
        {
            Type t = typeof(T);
            foreach(var item in ValuePairs)
            {
                Reduce.AddQuery(t, item.Key);
            }
            return this;
        }
        public Query<T> Select(Expression<Func<T, Object>> select)
        {
            if (select.Body.NodeType != ExpressionType.New) 
                throw new Exception("选择性查询请传new{}的表达式");
            Reduce.AddQuery(select);
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
            Reduce.AddTable(t, TableOperatorEnum.InnerJoin, foreign);
            return this;
        }
        public Query<T> Left<T1>(Expression<Func<T1, Object>> foreign, Expression<Func<T1, Object>> query = null)
        {
            SetQueryOtherTable(query);
            Type t = typeof(T1);
            Reduce.AddTable(t, TableOperatorEnum.LeftJoin, foreign);
            return this;
        }
        public Query<T> Where(Expression<Func<T, Boolean>> expressions)
        {
            Reduce.AddWhere(expressions);
            return this;
        }
        public Query<T> Where<T1>(Expression<Func<T, T1, Boolean>> expressions)
        {
            Reduce.AddWhere(expressions);
            return this;
        }
        public Query<T> Top(int num)
        {
            Reduce.AddTop(num);
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
                        Reduce.AddQuery(tablename, item.Key);
                    }
                }
                else
                {
                    Reduce.AddQuery(query);
                }
            }
        }
        public abstract IBatch End();
    }
}
