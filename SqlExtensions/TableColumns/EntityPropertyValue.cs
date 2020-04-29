using System;
using System.Collections.Generic;
using System.Text;

namespace Dapper.Framework
{
    public class EntityPropertyValue
    {
        /// <summary>
        /// 字段名
        /// </summary>
        public String ColumnName { get; set; }
        /// <summary>
        /// 是否自增字段,请注意
        /// 如果是自增字段,那么在新增数据会排出掉此字段
        /// </summary>
        public Boolean IsIncrease { get; set; }
        /// <summary>
        /// 此字段对应的是哪个表的外键
        /// 请注意,此处没添加此功能,需手写sql
        /// </summary>
        //public EntityTableValue ReferenceValue { get; set; }
    }
}
