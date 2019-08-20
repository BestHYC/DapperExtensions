using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Dapper.Framework.SqlExtensions
{
    /// <summary>
    /// 数据DB仓储层
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IDbRepository<T> where T : IEntity, new()
    {
        /// <summary>
        /// 读取符合条件的单条数据
        /// </summary>
        /// <param name="expressions">条件表达式</param>
        /// <returns></returns>
        T Get(Expression<Func<T, object>> expressions);
        /// <summary>
        /// 读取单条数据
        /// </summary>
        /// <param name="obj">匿名参数类</param>
        /// <returns></returns>
        T Get(string exper, object obj);
        /// <summary>
        /// 读取多条数据
        /// </summary>
        /// <param name="obj">参数字典</param>
        /// <returns></returns>
        IEnumerable<T> Get(string exper, Dictionary<string, object> dictionary);
        /// <summary>
        /// 读取全部数据
        /// </summary>
        /// <returns></returns>
        IEnumerable<T> All();
        /// <summary>
        /// 读取指定数量符合条件的数据
        /// </summary>
        /// <param name="expressions">条件表达式</param>
        /// <param name="num">数量(默认全部读取)</param>
        /// <returns></returns>
        IEnumerable<T> GetList(Expression<Func<T, object>> expressions, int num = -1);
        /// <summary>
        /// 插入数据
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        dynamic Insert(T entity);
        /// <summary>
        /// 更新数据
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="expressions"></param>
        /// <returns></returns>
        dynamic Update(T entity, Expression<Func<T, object>> expressions);
        /// <summary>
        /// 更新数据
        /// </summary>
        /// <param name="entity">实体模型</param>
        /// <param name="func">属性</param>
        /// <param name="expressions">条件表达式</param>
        /// <returns>影响行数</returns>
        dynamic Update(T entity, Func<string[]> func, Expression<Func<T, object>> expressions);
        /// <summary>
        /// 删除数据
        /// </summary>
        /// <param name="pkValue">主键值</param>
        /// <returns></returns>
        int Delete(dynamic pkValue);
        /// <summary>
        /// 删除符合条件的数据
        /// </summary>
        /// <param name="expressions">条件表达式</param>
        /// <returns></returns>
        int Delete(Expression<Func<T, object>> expressions);
        /// <summary>
        /// 读取分页数据
        /// </summary>
        /// <param name="pagingIndex">页码</param>
        /// <param name="showLine">显示行数</param>
        /// <param name="rowId">行号字段</param>
        /// <returns></returns>
        IEnumerable<T> GetPagingList(int pagingIndex, int showLine, out int count, string searchValue, Expression<Func<T, object>> searchFiled, Expression<Func<T, object>> rowId);

        int Count(string pkName, string pkValue);
    }
}
