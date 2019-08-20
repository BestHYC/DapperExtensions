using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Dapper.Framework.SqlExtensions
{
    /// <summary>
    /// 表属性汇总
    /// </summary>
    public class EntityTableValue
    {
        /// <summary>
        /// 获取对应表的字段名
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public String this[String name] => ColoumNamePairs[name];
        /// <summary>
        /// 缓存表名
        /// </summary>
        public String Tablename { get; set; }
        /// <summary>
        /// 缓存主键名,如果多个健凑合成主键,此处没有,在写sql时候,必须手动
        /// </summary>
        public String PkColumnName { get; set; }
        /// <summary>
        /// 缓存当前字段对应的sql名,如果没有标注,那么此字段名便为表中字段名
        /// </summary>
        public Dictionary<String, String> ColoumNamePairs { get; set; }
        public void AddColum(String name, String value)
        {
            ColoumNamePairs.Add(name, value);
        }
    }
    /// <summary>
    /// 静态缓存所有实体对应的表的字段名
    /// </summary>
    public static class EntityTableMapper
    {
        private static Object _lock = new Object();
        public static Dictionary<Type, EntityTableValue> ValuePairs = new Dictionary<Type, EntityTableValue>();
        /// <summary>
        /// 添加类型,并缓存字段对应的所有属性
        /// </summary>
        /// <param name="type"></param>
        public static void Add(Type type)
        {
            if (type == null) throw new ArgumentException("类型不能为空");
            lock (_lock)
            {
                if (!ValuePairs.ContainsKey(type))
                {
                    Type t = type;
                    EntityTableValue value = new EntityTableValue();
                    Attribute table = t.GetCustomAttribute(typeof(TableMapperAttribute));
                    Dictionary<String, String> valuePairs = new Dictionary<String, String>();
                    //若没有写对应的特性,那么类名便是表名
                    String TableName = t.Name;
                    if (table != null)
                    {
                        TableName = (table as TableMapperAttribute).TableName;
                    }
                    value.Tablename = TableName;
                    foreach (PropertyInfo pi in t.GetProperties())
                    {
                        if (pi.GetCustomAttribute(typeof(IgnoreAttribute)) != null) continue;
                        String name = pi.Name;
                        Attribute attribute = pi.GetCustomAttribute(typeof(PropertyNameAttribute));
                        if (attribute != null)
                        {
                            name = ((PropertyNameAttribute)attribute).ColumnName;
                        }
                        //主键只允许有一个,如果想多个主键,请实现表达式树写法
                        if (String.IsNullOrEmpty(value.PkColumnName))
                        {
                            Attribute pk = pi.GetCustomAttribute(typeof(PrimaryKeyAttribute));
                            if (pk != null)
                            {
                                value.PkColumnName = pi.Name;
                            }
                        }
                        valuePairs.Add(pi.Name, name);
                    }
                    value.ColoumNamePairs = valuePairs;
                    ValuePairs.Add(t, value);
                }
            }
        }
        /// <summary>
        /// 获取类型对应表的详细信息
        /// </summary>
        /// <param name="type">类型</param>
        /// <returns></returns>
        public static Dictionary<String, String> GetColumns(Type type)
        {
            if (type == null) throw new ArgumentException("类型不能为空");
            lock (_lock)
            {
                if (!ValuePairs.ContainsKey(type))
                {
                    Add(type);
                }
                return ValuePairs[type].ColoumNamePairs;
            }
        }
        /// <summary>
        /// 获取表类型对应的主键
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static String GetPkColumn(Type type)
        {
            if (type == null) throw new ArgumentException("类型不能为空");
            lock (_lock)
            {
                if (!ValuePairs.ContainsKey(type))
                {
                    Add(type);
                }
                return ValuePairs[type].PkColumnName;
            }
        }
        /// <summary>
        /// 获取类型对应的表名
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static String GetTableName(Type type)
        {
            if (type == null) throw new ArgumentException("请传入对应的表类型");
            lock (_lock)
            {
                if (!ValuePairs.ContainsKey(type))
                {
                    Add(type);
                }
                return ValuePairs[type].Tablename;
            }
        }
        /// <summary>
        /// 获取类型字段对应的sql中的名称
        /// </summary>
        /// <param name="type"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static String GetColoumName(Type type, String name)
        {
            if (String.IsNullOrEmpty(name)) throw new ArgumentException("Table对应的属性值不能为空");
            EntityTableValue pairs = null;
            if (ValuePairs.TryGetValue(type, out pairs))
            {
                return pairs[name];
            }
            return null;
        }
    }
}
