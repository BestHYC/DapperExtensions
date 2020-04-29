using System;
using System.Collections.Generic;
using System.Text;

namespace Dapper.Framework
{
    /// <summary>
    /// 表属性集合
    /// 记录表的主键,表名,实体字段与数据库字段的对照关系
    /// </summary>
    public class EntityTableValue
    {
        /// <summary>
        /// 获取对应表的字段名
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public EntityPropertyValue this[String name] => ColoumNamePairs[name];
        /// <summary>
        /// 缓存表名
        /// </summary>
        public String Tablename { get; set; }
        /// <summary>
        /// 缓存主键名,
        /// 如果多个健凑合成主键,此处没有,在写sql时候,必须手动
        /// </summary>
        public String PkColumnName { get; set; }
        /// <summary>
        /// 缓存当前字段对应的sql名,如果没有标注,那么此字段名便为表中字段名
        /// </summary>
        public Dictionary<String, EntityPropertyValue> ColoumNamePairs { get; set; }
        public HashSet<String> IncresementColumn { get; } = new HashSet<String>();
        public void AddColum(String name, EntityPropertyValue value)
        {
            if (ColoumNamePairs == null)
            {
                ColoumNamePairs = new Dictionary<String, EntityPropertyValue>();
            }
            ColoumNamePairs.Add(name, value);
        }
    }
}
