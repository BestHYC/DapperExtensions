using Dapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Dapper.Framework
{
    
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
