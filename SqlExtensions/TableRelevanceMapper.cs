using System;
using System.Collections.Generic;
using System.Text;

namespace Dapper.Framework.SqlExtensions
{
    /// <summary>
    /// 表关联管理
    /// </summary>
    public class TableRelevanceMapper
    {
        /// <summary>
        /// 
        /// </summary>
        public List<ColumnRelevanceMapper> ColumnOperator { get; private set; } 
            = new List<ColumnRelevanceMapper>();
        /// <summary>
        /// 若无引用,则为当前主表
        /// </summary>
        public TableRelevanceMapper TableReference { get; set; }
        /// <summary>
        /// 关联表结构
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
