using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Dapper.Framework.SqlExtensions
{
    public class SqlParameters:IEnumerable<SqlKeyValuePair>
    {
        private List<Object> _list = new List<Object>();
        private Dictionary<String, Object> _keyValues = new Dictionary<string, object>();
        private Object _lock = new object();
        public void AddObject(Object obj)
        {
            lock (_lock)
            {
                if (obj != null)
                {
                    _list.Add(obj);
                }
            }
        }
        public void AddValuePairs(String name, Object value)
        {
            lock (_lock)
            {
                if (String.IsNullOrEmpty(name) && value != null)
                {
                    _keyValues.Add(name, value);
                }
            }    
        }

        public IEnumerator<SqlKeyValuePair> GetEnumerator()
        {
            foreach(var item in _list)
            {
                yield return new SqlKeyValuePair() { Name = "", Value = item };
            }
            foreach(var item in _keyValues)
            {
                yield return new SqlKeyValuePair() { Name = item.Key, Value = item.Value };
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
    public class SqlKeyValuePair
    {
        public String Name { get; set; }
        public Object Value { get; set; }
    }
}
