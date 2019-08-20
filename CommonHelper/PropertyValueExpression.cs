using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Dapper.Framework
{
    public class PropertyValueExpression<T>
    {
        public static Expression<Func<T, Boolean>> BuildExpression(String name, Object value)
        {
            Type type = typeof(T);
            PropertyInfo prop = type.GetProperty(name);
            ParameterExpression param = Expression.Parameter(type, "c");
            MemberExpression left = Expression.Property(param, prop);
            var right = Expression.Convert(Expression.Constant(value), prop.PropertyType);
            BinaryExpression body = Expression.Equal(left, right);
            return Expression.Lambda<Func<T, Boolean>>(body, param);
        }
        private static Func<T, Object> BuildExpression(PropertyInfo str)
        {
            Type type = typeof(T);
            Type paramType = str.PropertyType;
            ParameterExpression param = Expression.Parameter(type, "c");
            MemberExpression left = Expression.Property(param, str);
            var convertExpression = Expression.Convert(left, typeof(Object));
            var lambdaExpression = Expression.Lambda<Func<T, Object>>(convertExpression, param);
            return lambdaExpression.Compile();
        }
        private static Action<T, Object> CreateSetPropertyValueAction(String name)
        {
            var property = typeof(T).GetProperty(name);
            var target = Expression.Parameter(typeof(T));
            var propertyValue = Expression.Parameter(typeof(Object));
            var castPropertyValue = Expression.Convert(propertyValue, property.PropertyType);
            var setPropertyValue = Expression.Call(target, property.GetSetMethod(), castPropertyValue);
            return Expression.Lambda<Action<T, Object>>(setPropertyValue, target, propertyValue).Compile();
        }
        public static void SetValue(T obj, String name, Object value)
        {
            Action<T, Object> d;
            if (SetCache.TryGetValue(name, out d))
            {
                d(obj, value);
            }
            var getDelg = CreateSetPropertyValueAction(name);
            SetCache[name] = getDelg;
            getDelg(obj, value);
        }
        /// <summary>
        /// 通过表达式返回对应的属性的值
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="property"></param>
        /// <returns></returns>
        public static object GetValue(T obj, PropertyInfo property)
        {
            Delegate d;
            if (delegateCache.TryGetValue(property, out d))
            {
                var func = (Func<T, object>)d;
                return func(obj);
            }
            var getDelg = BuildExpression(property);
            delegateCache[property] = getDelg;
            return getDelg(obj);
        }
        private static ConcurrentDictionary<PropertyInfo, Delegate> delegateCache = new ConcurrentDictionary<PropertyInfo, Delegate>();
        private static ConcurrentDictionary<String, Action<T, Object>> SetCache = new ConcurrentDictionary<String, Action<T, Object>>();
    }
}
