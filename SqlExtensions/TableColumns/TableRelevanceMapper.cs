using System;
using System.Collections.Generic;
using System.Text;

namespace Dapper.Framework
{
    /// <summary>
    /// 表关联管理
    /// 如 A表 left join 或者是Inner Join B表
    /// </summary>
    public class TableRelevanceMapper
    {
        public TableRelevanceMapper() : this(null)
        {

        }
        public TableRelevanceMapper(Type t) : this(t, TableOperatorEnum.None)
        {

        }
        public TableRelevanceMapper(Type t, TableOperatorEnum tableOperator)
        {
            TableName = t;
            TableOperatorEnum = tableOperator;
            ColumnOperator = new List<ColumnRelevanceMapper>();
        }
        /// <summary>
        /// 注意是多个关联关系 比如 a.name = b.name and a.sex=b.sex
        /// </summary>
        public List<ColumnRelevanceMapper> ColumnOperator { get; }
        /// <summary>
        /// 若无引用,则为当前主表
        /// A left join B 那么A的TableReference 为null B 的为A的TableRelevanceMapper
        /// </summary>
        public TableRelevanceMapper TableReference { get; set; }
        /// <summary>
        /// 关联表结构关系 如Inner Join Right Join
        /// </summary>
        public TableOperatorEnum TableOperatorEnum { get; set; }
        public Type TableName { get; set; }
    }
    /// <summary>
    /// 字段关联管理
    /// </summary>
    public class ColumnRelevanceMapper
    {
        /// <summary>
        /// 若无引用,则为当前主字段
        /// </summary>
        public ColumnRelevanceMapper ColumnReference { get; set; }
        public SqlOperatorEnum SqlOperatorEnum { get; set; }
        public String ColumnName { get; set; }
        public Type TableName { get; set; }
    }
    
}
