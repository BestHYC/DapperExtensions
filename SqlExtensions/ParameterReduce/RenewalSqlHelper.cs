using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dapper.Framework
{
    /// <summary>
    /// 只针对解析Sql帮助字段包装
    /// 因为增删改操作,只对单一数据表进行修改
    /// 如 Insert A => insert into A (id, name) values(1, 1);
    /// 所以只有一个表,但是有多个Column
    /// 注意,如果是自增主键,那么在新增会直接排斥掉
    /// </summary>
    public class RenewalSqlHelper
    {
        public List<ColumnRelevanceMapper> Header { get; }
        public TableRelevanceMapper Table { get; }
        public List<ColumnRelevanceMapper> Where { get; }
        public RenewalSqlHelper()
        {
            Header = new List<ColumnRelevanceMapper>();
            Table = new TableRelevanceMapper();
            Where = new List<ColumnRelevanceMapper>();
        }
        public void AddHeader(String header, Type t = null)
        {
            if (!String.IsNullOrEmpty(header))
            {
                AddHeader(
                    new ColumnRelevanceMapper() { ColumnName = header, TableName = t });
            }
        }
        public void AddHeader(ColumnRelevanceMapper column)
        {
            if (column != null)
            {
                Header.Add(column);
            }
        }
        public void AddWhere(String where)
        {
            if (!String.IsNullOrEmpty(where))
            {
                Where.Add(
                    new ColumnRelevanceMapper() { ColumnName = where, TableName = Table.TableName });
            }
        }
        public Int32 WhereCount()
        {
            return Where.Count();
        }
        public void AddWhere(ColumnRelevanceMapper column)
        {
            if (column != null)
            {
                Where.Add(column);
            }
        }
    }
}
