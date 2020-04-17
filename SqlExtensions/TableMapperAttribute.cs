using System;
using System.Collections.Generic;
using System.Text;

namespace Dapper.Framework.SqlExtensions
{
    /// <summary>
    /// 类型对应的数据库中的表名
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class TableMapperAttribute : Attribute
    {
        public String TableName { get; }
        public TableMapperAttribute(String name)
        {
            TableName = name;
        }
    }
}
