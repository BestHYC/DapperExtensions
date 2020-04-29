using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Text;

namespace Dapper.Framework
{
    
    public abstract class BaseRepository<T>: IConnectionCreate where T : IEntity, new()
    {
        protected ISqlBuilder<T> _sqlBuilder;
        protected Query<T> _query { get { return _sqlBuilder.CreateQueryService(); } }
        protected Renewal<T> _renewal { get { return _sqlBuilder.CreateRenewalService(); } }
        protected IExecuteBatch<T> _executeBatch { get { return _sqlBuilder.CreateDapper(); } }
        public BaseRepository()
        {
            _sqlBuilder =  new SqlBuilderFactory<T>(this).CreateBuilder();
        }
        /// <summary>
        /// 根据主键查询数据
        /// </summary>
        /// <param name="pkValue"></param>
        /// <returns></returns>
        public T Get(Object pkValue)
        {
            Type t = typeof(T);
            String name = EntityTableMapper.GetPkColumn(t);
            Expression<Func<T,Boolean>> expression = PropertyValueExpression<T>.BuildExpression(name, pkValue);
            return _executeBatch.Query(_query.Select().Where(expression).End());
        }
        /// <summary>
        /// 根据刷选条件查询第一条数据
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public T Get(Expression<Func<T, Boolean>> query)
        {
            return _executeBatch.Query(_query.Select().Where(query).End());
        }
        /// <summary>
        /// 查询所有数据
        /// </summary>
        /// <returns></returns>
        public IEnumerable<T> QueryAll()
        {
            return _executeBatch.QueryList(_query.Select().End());
        }
        public IEnumerable<T> QueryAllReader()
        {
            return _executeBatch.ExecuteReader(_query.Select().End());
        }
        /// <summary>
        /// 根据条件查询列表数据
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public IEnumerable<T> QueryList(Expression<Func<T, Boolean>> query)
        {
            return _executeBatch.QueryList(_query.Select().Where(query).End());
        }
        public IEnumerable<T> QueryListReader(Expression<Func<T, Boolean>> query)
        {
            return _executeBatch.ExecuteReader(_query.Select().Where(query).End());
        }
        /// <summary>
        /// 根据id修改数据,如果没主键则报错,注意主键无法修改
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public Int32 Update(T model)
        {
            return _executeBatch.Execute(_renewal.Update(model).End());
        }
        /// <summary>
        /// 需要更新的字段修改数据,注意主键无法修改
        /// </summary>
        /// <param name="model"></param>
        /// <param name="select">需要更新的字段</param>
        /// <returns></returns>
        public Int32 Update(T model, Expression<Func<T, Object>> select)
        {
            return _executeBatch.Execute(_renewal.Update(model, select).End());
        }
        /// <summary>
        /// 根据条件修改字段
        /// </summary>
        /// <param name="model"></param>
        /// <param name="where"></param>
        /// <returns></returns>
        public Int32 Update(T model, Expression<Func<T, Boolean>> where)
        {
            return _executeBatch.Execute(_renewal.Update(model).Where(where).End());
        }
        /// <summary>
        /// 根据条件修改字段 比如 只修改那么 (model, item=>new{item.name}, item=>item.id==model.id)
        /// </summary>
        /// <param name="model"></param>
        /// <param name="select">需要更新的字段</param>
        /// <param name="where">需要更新的条件</param>
        /// <returns></returns>
        public Int32 Update(T model, Expression<Func<T, Object>> select, Expression<Func<T, Boolean>> where)
        {
            return _executeBatch.Execute(_renewal.Update(model, select).Where(where).End());
        }
        public Int32 Count(Expression<Func<T, Boolean>> where = null)
        {
            if(where == null)
            {
                where = item => 1 == 1;
            }
            return _executeBatch.Query<Int32>(_query.Count(where).End());
        }
        /// <summary>
        /// 插入数据,注意主键无法新增
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public Int32 Insert(T model)
        {
            return _executeBatch.Execute(_renewal.Insert(model).End());
        }
        /// <summary>
        /// 插入数据,注意,主键无法插入
        /// </summary>
        /// <param name="model">传入的实体</param>
        /// <param name="select">需要插入的字段</param>
        /// <returns></returns>
        public Int32 Insert(T model, Expression<Func<T, Object>> select)
        {
            return _executeBatch.Execute(_renewal.Insert(model, select).End());
        }
        /// <summary>
        /// 根据id删除,如果没有主键 则报错
        /// </summary>
        /// <param name="model">传入的实体</param>
        /// <returns></returns>
        public Int32 Delete(T model)
        {
            return _executeBatch.Execute(_renewal.Delete(model).End());
        }
        /// <summary>
        /// 根据条件删除
        /// </summary>
        /// <param name="where"></param>
        /// <returns></returns>
        public Int32 Delete(Expression<Func<T, Boolean>> where)
        {
            return _executeBatch.Execute(_renewal.Delete(where).End());
        }
        public abstract IDbConnection CreateConnection();
    }
}
