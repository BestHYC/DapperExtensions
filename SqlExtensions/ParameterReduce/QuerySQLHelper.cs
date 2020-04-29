using System;
using System.Collections.Generic;
using System.Linq;
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
        public ColumnRelevanceMapper Top { get; }
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
}
