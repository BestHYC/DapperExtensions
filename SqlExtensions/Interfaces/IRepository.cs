using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Dapper.Framework
{
    public interface IRepository<T>
    {
        /// <summary>
        /// 通过id查询
        /// </summary>
        /// <param name="pkValue"></param>
        /// <returns></returns>
        T Get(Object pkValue);
        /// <summary>
        /// 通过条件查询
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        T Get(Expression<Func<T, Boolean>> query);
        /// <summary>
        /// 查询所有
        /// </summary>
        /// <returns></returns>
        IEnumerable<T> QueryAll();
        /// <summary>
        /// 通过Reader查询全部数据
        /// </summary>
        /// <returns></returns>
        IEnumerable<T> QueryAllReader();
        /// <summary>
        /// 根据条件查询
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        IEnumerable<T> QueryList(Expression<Func<T, Boolean>> query);
        /// <summary>
        /// 通过Reader查询数据
        /// </summary>
        /// <param name="query">查询条件</param>
        /// <returns></returns>
        IEnumerable<T> QueryListReader(Expression<Func<T, Boolean>> query);
        /// <summary>
        /// 通过主键进行修改
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Int32 Update(T model);
        /// <summary>
        /// 根据条件进行修改
        /// </summary>
        /// <param name="model"></param>
        /// <param name="select"></param>
        /// <returns></returns>
        Int32 Update(T model, Expression<Func<T, Object>> select);
        Int32 Update(T model, Expression<Func<T, Boolean>> where);
        Int32 Update(T model, Expression<Func<T, Object>> select, Expression<Func<T, Boolean>> where);
        /// <summary>
        /// 插入
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Int32 Insert(T model);
        /// <summary>
        /// 选择性插入数据,其他字段以默认形式插入
        /// </summary>
        /// <param name="model"></param>
        /// <param name="select"></param>
        /// <returns></returns>
        Int32 Insert(T model, Expression<Func<T, Object>> select);
        /// <summary>
        /// 通过id删除
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Int32 Delete(T model);
        /// <summary>
        /// 通过条件删除
        /// </summary>
        /// <param name="model"></param>
        /// <param name="where"></param>
        /// <returns></returns>
        Int32 Delete(Expression<Func<T, Boolean>> where);
    }
}
