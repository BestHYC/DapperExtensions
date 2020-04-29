using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;

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
        public void AddColum(String name, EntityPropertyValue value)
        {
            if (ColoumNamePairs == null)
            {
                ColoumNamePairs = new Dictionary<String, EntityPropertyValue>();
            }
            ColoumNamePairs.Add(name, value);
        }
    }
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
    /// <summary>
    /// 静态缓存所有实体对应的表的字段名
    /// 
    /// </summary>
    public static class EntityTableMapper
    {
        private static Object _lock = new Object();
        public readonly static Dictionary<Type, EntityTableValue> ValuePairs = 
            new Dictionary<Type, EntityTableValue>();
        /// <summary>
        /// 加载某个程序集中继承IEntity的类型
        /// </summary>
        /// <param name="assembly"></param>
        public static void AddAssembly(Assembly assembly)
        {
            lock (_lock)
            {
                var alltype = from t in assembly.ExportedTypes
                              where typeof(IEntity).IsAssignableFrom(t) && t.IsPublic
                              select t;
                foreach (var item in alltype)
                {
                    Add(item);
                }
            }
        }
        /// <summary>
        /// 添加类型,并缓存字段对应的所有属性
        /// </summary>
        /// <param name="type"></param>
        public static void Add(Type type)
        {
            if (type == null) throw new ArgumentException("类型不能为空");
            if (!typeof(IEntity).IsAssignableFrom(type))
                throw new ArgumentException("必须基于IEntity接口");
            if (!ValuePairs.ContainsKey(type))
            {
                lock (_lock)
                {
                    if (ValuePairs.ContainsKey(type)) return;
                    Type t = type;
                    EntityTableValue value = new EntityTableValue();
                    Attribute table = t.GetCustomAttribute(typeof(TableMapperAttribute));
                    //若没有写对应的特性,那么类名便是表名
                    String TableName = t.Name;
                    if (table != null)
                    {
                        TableName = (table as TableMapperAttribute).TableName;
                    }
                    value.Tablename = TableName;
                    //将字段与数据库中字段名对照起来
                    Dictionary<String, EntityPropertyValue> valuePairs = new Dictionary<String, EntityPropertyValue>();
                    foreach (PropertyInfo pi in t.GetProperties())
                    {
                        if (pi.GetCustomAttribute(typeof(IgnoreAttribute)) != null) continue;
                        String name = pi.Name;
                        Attribute attribute = pi.GetCustomAttribute(typeof(PropertyNameAttribute));
                        if (attribute != null)
                        {
                            name = ((PropertyNameAttribute)attribute).ColumnName;
                        }
                        //自增主键只允许有一个,如果想多个主键,请实现表达式树写法
                        if (String.IsNullOrEmpty(value.PkColumnName))
                        {
                            Attribute pk = pi.GetCustomAttribute(typeof(PrimaryKeyAttribute));
                            if (pk != null)
                            {
                                value.PkColumnName = pi.Name;
                            }
                        }
                        EntityPropertyValue propertyValue = new EntityPropertyValue()
                        {
                            ColumnName = name,
                            IsIncrease = false
                        };
                        Attribute isincrease = pi.GetCustomAttribute(typeof(IncreaseKeyAttribute));
                        if (isincrease != null)
                        {
                            propertyValue.IsIncrease = true;
                        }
                        valuePairs.Add(pi.Name, propertyValue);
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
        public static Dictionary<String, EntityPropertyValue> GetColumns(Type type)
        {
            if (type == null) throw new ArgumentException("类型不能为空");
            if (!ValuePairs.ContainsKey(type))
            {
                lock (_lock)
                {
                    Add(type);
                }
            }
            return ValuePairs[type].ColoumNamePairs;
        }
        /// <summary>
        /// 获取表类型对应的自增主键
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static String GetPkColumn(Type type)
        {
            if (type == null) throw new ArgumentException("类型不能为空");
            if (!ValuePairs.ContainsKey(type))
            {
                lock (_lock)
                {
                    Add(type);
                }
            }
            return ValuePairs[type].PkColumnName;
        }
        /// <summary>
        /// 获取类型对应的表名
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static String GetTableName(Type type)
        {
            if (type == null) throw new ArgumentException("请传入对应的表类型");
            if (!ValuePairs.ContainsKey(type))
            {
                lock (_lock)
                {
                    Add(type);
                }
            }
            return ValuePairs[type].Tablename;
        }
        /// <summary>
        /// 获取类型字段对应的sql中的名称
        /// </summary>
        /// <param name="type"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static EntityPropertyValue GetColoum(Type type, String name)
        {
            if (String.IsNullOrEmpty(name)) throw new ArgumentException("Table对应的属性值不能为空");
            if (!ValuePairs.ContainsKey(type))
            {
                lock (_lock)
                {
                    Add(type);
                }
            }
            EntityTableValue pairs = null;
            if (ValuePairs.TryGetValue(type, out pairs))
            {
                return pairs[name];
            }
            return null;
        }
        public static String GetColoumName(Type type, String name)
        {
            if (String.IsNullOrEmpty(name)) throw new ArgumentException("Table对应的属性值不能为空");
            if (!ValuePairs.ContainsKey(type))
            {
                lock (_lock)
                {
                    Add(type);
                }
            }
            EntityTableValue pairs = null;
            if (ValuePairs.TryGetValue(type, out pairs))
            {
                return pairs[name].ColumnName;
            }
            return null;
        }
    }
}
