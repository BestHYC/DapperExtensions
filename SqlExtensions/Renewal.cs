using Dapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Dapper.Framework
{
    /// <summary>
    /// 只针对解析Sql帮助字段包装
    /// 因为增删改操作,只对单一数据表进行修改
    /// 如 Insert A => insert into A (id, name) values(1, 1);
    /// 所以只有一个表,但是有多个Column
    /// 注意,如果是自增主键,那么在新增会直接排斥掉
    /// </summary>
    public class RenewalSqlHelper
    {
        public List<ColumnRelevanceMapper> Header { get; } 
        public TableRelevanceMapper Table { get; }
        public List<ColumnRelevanceMapper> Where { get; }
        public RenewalSqlHelper()
        {
            Header = new List<ColumnRelevanceMapper>();
            Table = new TableRelevanceMapper();
            Where = new List<ColumnRelevanceMapper>();
        }
        public void AddHeader(String header, Type t = null)
        {
            if (!String.IsNullOrEmpty(header))
            {
                AddHeader(
                    new ColumnRelevanceMapper() { ColumnName = header, TableName = t });
            }
        }
        public void AddHeader(ColumnRelevanceMapper column)
        {
            if(column != null)
            {
                Header.Add(column);
            }
        }
        public void AddWhere(String where)
        {
            if (!String.IsNullOrEmpty(where))
            {
                Where.Add(
                    new ColumnRelevanceMapper() { ColumnName = where, TableName = Table.TableName });
            }
        }
        public Int32 WhereCount()
        {
            return Where.Count();
        }
        public void AddWhere(ColumnRelevanceMapper column)
        {
            if (column != null)
            {
                Where.Add(column);
            }
        }
    }
    public abstract class Renewal<T> : IExecute where T:IEntity, new()
    {
        protected BatchParameterReduce Reduce = new BatchParameterReduce();
        /// <summary>
        /// 当前类型的对应的sql表字段
        /// </summary>
        protected Dictionary<String, EntityPropertyValue> ValuePairs = EntityTableMapper.GetColumns(typeof(T));
        private static Object _lock = new object();
        static Renewal()
        {
            EntityTableMapper.Add(typeof(T));
        }

        public Renewal<T> Insert(T model)
        {
            Type t = typeof(T);
            Reduce.AddTable(t, TableOperatorEnum.Insert);
            foreach (var item in ValuePairs)
            {
                Reduce.AddHeader(item.Key, t);
            }
            Reduce.AddDynamicParams(model);
            return this;
        }
        public Renewal<T> Insert(T model, Expression<Func<T, Object>> select)
        {
            if (select.Body.NodeType != ExpressionType.New) throw new Exception("选择性查询请传new{}的表达式");
            Type t = typeof(T);
            Reduce.AddTable(t, TableOperatorEnum.Insert)
            .AddHeader(select)
            .AddDynamicParams(model);
            return this;
        }
        public Renewal<T> Update(T model)
        {
            Type t = typeof(T);
            Reduce.AddTable(t, TableOperatorEnum.Update);
            foreach (var item in ValuePairs)
            {
                Reduce.AddHeader(item.Key, t);
            }
            Reduce.AddDynamicParams(model);
            return this;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="model">模型实体</param>
        /// <param name="select">需要更新的字段</param>
        /// <returns></returns>
        public Renewal<T> Update(T model, Expression<Func<T, Object>> select)
        {
            if (select.Body.NodeType != ExpressionType.New) throw new Exception("选择性查询请传new{}的表达式");
            Type t = typeof(T);
            Reduce.AddTable(t, TableOperatorEnum.Update)
            .AddHeader(select)
            .AddDynamicParams(model);
            return this;
        }
        /// <summary>
        /// 对数据库进行自身操作 如 统计执行次数,对数据库中synctimes字段进行+1 synctimes = synctimes+1
        /// </summary>
        /// <param name="select"></param>
        /// <returns></returns>
        public Renewal<T> Set(String select)
        {
            Reduce.AddHeader(select);
            return this;
        }
        public Renewal<T> Delete(T model)
        {
            Type t = typeof(T);
            Reduce.AddTable(t, TableOperatorEnum.Delete)
            .AddDynamicParams(model);
            return this;
        }
        public Renewal<T> Delete(Expression<Func<T, Boolean>> where)
        {
            Type t = typeof(T);
            Reduce.AddTable(t, TableOperatorEnum.Delete);
            Where(where);
            return this;
        }
        /// <summary>
        /// i
        /// </summary>
        /// <param name="expressions"></param>
        /// <returns></returns>
        public Renewal<T> Where(Expression<Func<T, Boolean>> expressions)
        {
            Reduce.AddWhere(expressions);
            return this;
        }
        public Renewal<T> Where<K>(Expression<Func<T, K, Boolean>> expressions)
        {
            Reduce.AddWhere(expressions);
            return this;
        }
        public Renewal<T> In(Expression<Func<T, Object>> item, IEnumerable<Object> objs)
        {
            String name = objs.RandromName();
            Reduce.AddWhere(item)
            .AddWhere(new ColumnRelevanceMapper() { ColumnName = $"@{name}", SqlOperatorEnum = SqlOperatorEnum.In })
            .AddParameters(name, objs);
            return this;
        }
        public Renewal<T> In(Expression<Func<T, Object>> item, IBatch batch)
        {
            Reduce.AddWhere(item)
            .AddWhere(new ColumnRelevanceMapper() { ColumnName = $"({batch.SqlBuilder})", SqlOperatorEnum = SqlOperatorEnum.In })
            .AddDynamicParams(batch.DynamicParameters);
            return this;
        }
        public abstract IBatch End();
    }
}
