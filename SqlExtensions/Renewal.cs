using Dapper;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Dapper.Framework.SqlExtensions
{
    public class RenewalSqlHelper
    {
        public List<ColumnRelevanceMapper> Header { get; } = new List<ColumnRelevanceMapper>();
        public TableRelevanceMapper Table { get; private set; } = new TableRelevanceMapper();
        public List<ColumnRelevanceMapper> Where { get; } = new List<ColumnRelevanceMapper>();
        public void AddHeader(String header, Type t = null)
        {
            if (!String.IsNullOrEmpty(header))
            {
                Header.Add(
                    new ColumnRelevanceMapper() { ColumnName = header, TableName = t });
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
        public void AddWhere(ColumnRelevanceMapper column)
        {
            if(column != null)
            {
                Where.Add(column);
            }
        }
    }
    public abstract class Renewal<T> : IExecute where T:IEntity, new()
    {
        protected RenewalSqlHelper _sqlHelper = new RenewalSqlHelper();
        protected DynamicParameters _dynamic = new DynamicParameters();
        /// <summary>
        /// 当前类型的对应的sql表字段
        /// </summary>
        protected Dictionary<String, String> ValuePairs = EntityTableMapper.GetColumns(typeof(T));
        private static Object _lock = new object();
        static Renewal()
        {
            EntityTableMapper.Add(typeof(T));
        }

        public Renewal<T> Insert(T model)
        {
            Type t = typeof(T);
            _sqlHelper.Table.TableName = t;
            _sqlHelper.Table.TableOperatorEnum = TableOperatorEnum.Insert;
            foreach(var item in ValuePairs)
            {
                _sqlHelper.AddHeader(item.Key, t);
            }
            _dynamic.AddDynamicParams(model);
            return this;
        }
        public Renewal<T> Insert(T model, Expression<Func<T, Object>> select)
        {
            if (select.Body.NodeType != ExpressionType.New) throw new Exception("选择性查询请传new{}的表达式");
            Type t = typeof(T);
            _sqlHelper.Table.TableName = t;
            _sqlHelper.Table.TableOperatorEnum = TableOperatorEnum.Insert;
            ParameterReduce reduce = new ParameterReduce(_dynamic, _sqlHelper.Header);
            select.ConvertExpression(reduce);
            _dynamic.AddDynamicParams(model);
            return this;
        }
        public Renewal<T> Update(T model)
        {
            Type t = typeof(T);
            _sqlHelper.Table.TableName = t;
            _sqlHelper.Table.TableOperatorEnum = TableOperatorEnum.Update;
            foreach (var item in ValuePairs)
            {
                _sqlHelper.AddHeader(item.Key, t);
            }
            _dynamic.AddDynamicParams(model);
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
            _sqlHelper.Table.TableName = t;
            _sqlHelper.Table.TableOperatorEnum = TableOperatorEnum.Update;
            ParameterReduce reduce = new ParameterReduce(_dynamic, _sqlHelper.Header);
            select.ConvertExpression(reduce);
            _dynamic.AddDynamicParams(model);
            return this;
        }
        /// <summary>
        /// 对数据库进行自身操作 如 统计执行次数,对数据库中synctimes字段进行+1 synctimes = synctimes+1
        /// </summary>
        /// <param name="select"></param>
        /// <returns></returns>
        public Renewal<T> Set(String select)
        {
            _sqlHelper.AddHeader(select);
            return this;
        }
        public Renewal<T> Delete(T model)
        {
            Type t = typeof(T);
            _sqlHelper.Table.TableName = t;
            _sqlHelper.Table.TableOperatorEnum = TableOperatorEnum.Delete;
            _dynamic.AddDynamicParams(model);
            return this;
        }
        public Renewal<T> Delete(Expression<Func<T, Boolean>> where)
        {
            Type t = typeof(T);
            _sqlHelper.Table.TableName = t;
            _sqlHelper.Table.TableOperatorEnum = TableOperatorEnum.Delete;
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
            ParameterReduce reduce = new ParameterReduce(_dynamic, _sqlHelper.Where);
            expressions.ConvertExpression(reduce);
            return this;
        }
        public Renewal<T> Where<K>(Expression<Func<T, K, Boolean>> expressions)
        {
            ParameterReduce reduce = new ParameterReduce(_dynamic, _sqlHelper.Where);
            expressions.ConvertExpression(reduce);
            return this;
        }
        public Renewal<T> In(Expression<Func<T, Object>> item, IEnumerable<Object> objs)
        {
            ParameterReduce reduce = new ParameterReduce(_dynamic, _sqlHelper.Where);
            if (reduce.ReduceSql.Count != 0)
            {
                reduce.AddOperator(SqlOperatorEnum.And);
            }
            item.ConvertExpression(reduce);
            String name = objs.RandromName();
            _sqlHelper.AddWhere(new ColumnRelevanceMapper() { ColumnName = $"@{name}", SqlOperatorEnum = SqlOperatorEnum.In });
            reduce.Parameters.Add(name, objs);
            return this;
        }
        public Renewal<T> In(Expression<Func<T, Object>> item, IBatch batch)
        {
            ParameterReduce reduce = new ParameterReduce(_dynamic, _sqlHelper.Where);
            if(reduce.ReduceSql.Count != 0)
            {
                reduce.AddOperator(SqlOperatorEnum.And);
            }
            item.ConvertExpression(reduce);
            _sqlHelper.AddWhere(new ColumnRelevanceMapper() { ColumnName = $"({batch.SqlBuilder})", SqlOperatorEnum = SqlOperatorEnum.In });
            reduce.Parameters.AddDynamicParams(batch.DynamicParameters);
            return this;
        }
        public abstract IBatch End();
    }
}
