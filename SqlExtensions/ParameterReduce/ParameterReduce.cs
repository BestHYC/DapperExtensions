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
    
    
}
