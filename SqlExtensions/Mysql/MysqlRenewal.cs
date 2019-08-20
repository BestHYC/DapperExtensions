using System;
using System.Collections.Generic;
using System.Text;

namespace Dapper.Framework.SqlExtensions
{
    public class MysqlRenewal<T> : Renewal<T> where T : IEntity, new()
    {
        public override IBatch End()
        {
            IBatch batch = new SqlServerBatch();
            Tuple<List<String>, List<String>> tuple = ConvertHeader(_sqlHelper.Header);
            Type t = typeof(T);
            StringBuilder sb = new StringBuilder();
            sb.Append(_sqlHelper.Table.TableOperatorEnum.GetOperator());
            sb.Append($" {EntityTableMapper.GetTableName(t)} ");
            switch (_sqlHelper.Table.TableOperatorEnum)
            {
                case TableOperatorEnum.Insert:
                    sb.Append(GetInsertSql(tuple));
                    break;
                case TableOperatorEnum.Update:
                    sb.Append(" set ");
                    sb.Append(GetUpdateSql(tuple));
                    if (_sqlHelper.Where.Count == 0)
                    {
                        SetNoneWhere(t);
                    }
                    break;
                case TableOperatorEnum.Delete:
                    if (_sqlHelper.Where.Count == 0)
                    {
                        SetNoneWhere(t);
                    }
                    break;
                default:
                    throw new Exception("请先选择操作种类");
            }
            String where = ConvertToWhere(_sqlHelper.Where);
            if (!String.IsNullOrEmpty(where))
            {
                sb.Append($" where {where}");
            }
            batch.SqlBuilder = sb.ToString();
            batch.DynamicParameters = _dynamic;
            return batch;
        }
        private void SetNoneWhere(Type t)
        {
            String pk = EntityTableMapper.GetPkColumn(t);
            if (String.IsNullOrEmpty(pk)) throw new ArgumentException("请使用带主键的或者添加删选条件");
            String pkValue = ValuePairs[pk];
            ColumnRelevanceMapper column = new ColumnRelevanceMapper();
            column.ColumnName = pk;
            column.TableName = t;
            ColumnRelevanceMapper value = new ColumnRelevanceMapper();
            value.ColumnName = pk;
            value.SqlOperatorEnum = SqlOperatorEnum.Equal;
            _sqlHelper.AddWhere(column);
            _sqlHelper.AddWhere(value);
        }
        /// <summary>
        /// 此处去掉主键,即插入操作不能操作id
        /// </summary>
        /// <param name="tuple"></param>
        /// <returns></returns>
        private String GetInsertSql(Tuple<List<String>, List<String>> tuple)
        {
            String pk = EntityTableMapper.GetPkColumn(typeof(T));
            StringBuilder colstr = new StringBuilder();
            StringBuilder valstr = new StringBuilder();
            List<String> cols = tuple.Item1;
            List<String> props = tuple.Item2;
            for (Int32 i = 0; i < cols.Count; i++)
            {
                //去掉主键
                if (cols[i] == pk) continue;
                if (colstr.Length != 0)
                {
                    colstr.Append(",");
                    valstr.Append(",");
                }
                colstr.Append(cols[i]);
                valstr.Append($"@{props[i]}");
            }
            return $"({colstr.ToString()}) values ({valstr.ToString()})";
        }
        private String GetUpdateSql(Tuple<List<String>, List<String>> tuple)
        {
            String pk = EntityTableMapper.GetPkColumn(typeof(T));
            StringBuilder colstr = new StringBuilder();
            List<String> cols = tuple.Item1;
            List<String> props = tuple.Item2;
            for (Int32 i = 0; i < cols.Count; i++)
            {
                //去掉主键
                if (cols[i] == pk) continue;
                if (colstr.Length != 0)
                {
                    colstr.Append(",");
                }
                // synctimes = synctimes+1 的形式便直接拼接
                //注意,此处在解析header时候,便是1:1的关系,如果是set中便是为空
                if (String.IsNullOrEmpty(props[i]))
                {
                    colstr.Append($" {cols[i]} ");
                }
                else
                {
                    colstr.Append($"{cols[i]} = @{props[i]}");
                }
            }
            return colstr.ToString();
        }
        /// <summary>
        /// Renewal只对单表操作
        /// </summary>
        /// <param name="columns"></param>
        /// <returns> tuple的1是表字段,2为类属性</returns>
        private Tuple<List<String>, List<String>> ConvertHeader(List<ColumnRelevanceMapper> columns)
        {
            List<String> cols = new List<String>();
            List<String> props = new List<String>();
            foreach (var column in columns)
            {
                String colName = "";
                String propName = "";
                if (column.TableName != null)
                {
                    if (!ValuePairs.ContainsKey(column.ColumnName)) throw new ArgumentException("请传正确表字段");
                    colName = ValuePairs[column.ColumnName];
                    propName = column.ColumnName;
                }
                else
                {
                    colName = column.ColumnName;
                }
                cols.Add(colName);
                props.Add(propName);
            }
            return new Tuple<List<string>, List<string>>(cols, props);
        }
        private String ConvertToWhere(List<ColumnRelevanceMapper> where)
        {
            StringBuilder sb = new StringBuilder();
            // item.id != null 或者 null != item.id的解析步骤
            Boolean isNullOrNot = false;
            foreach (var column in where)
            {
                String str = ConditionConvert(column, isNullOrNot);
                if (String.IsNullOrEmpty(str))
                {
                    isNullOrNot = true;
                }
                else
                {
                    isNullOrNot = false;
                    sb.Append($" {str} ");
                }
            }
            return sb.ToString();
        }
        private String ConditionConvert(ColumnRelevanceMapper column, bool isNull = false)
        {
            StringBuilder sb = new StringBuilder();
            //如果 null == item.id 的写法, 会解析成 
            //column.ColumnName == null && column.SqlOperatorEnum == SqlOperatorEnum.None
            // null,那么就直接返回"", 接下来的  == item.id 会传递false,然后解析 item.id is null
            if (column.ColumnName == null && column.SqlOperatorEnum == SqlOperatorEnum.None)
            {
                return "";
            }
            //如果是 item.id == null 中写法 那么 == null 则返回 is null
            if (isNull || column.ColumnName == null)
            {
                column.SqlOperatorEnum = SqlEnumConvert(column.SqlOperatorEnum);
            }
            if (column.SqlOperatorEnum == SqlOperatorEnum.EqualNotNull ||
                column.SqlOperatorEnum == SqlOperatorEnum.EqualNull)
            {
                String str = "";
                if (!String.IsNullOrEmpty(column.ColumnName))
                {
                    Type t = column.TableName;
                    String tableName = EntityTableMapper.GetTableName(t);
                    String columnName = EntityTableMapper.GetColoumName(t, column.ColumnName);
                    str = $"{tableName}.{columnName}";
                }
                return $"{str} {column.SqlOperatorEnum.GetOperator()}";
            }
            if (column.SqlOperatorEnum != SqlOperatorEnum.None)
            {
                sb.Append(column.SqlOperatorEnum.GetOperator());
            }
            //如果是 直接传递字符串 如 id in (select id from xxx)中,则 括号中则直接拼接,而不用@数字
            //如果是 item.id == 1 中的  == 1,那么只 保存 = @ColumnName
            if (column.TableName == null)
            {
                if (column.ColumnName.IndexOf("(") != -1)
                {
                    sb.Append($" {column.ColumnName} ");
                }
                else
                {
                    sb.Append($"@{column.ColumnName}");
                }
            }
            //如果是 item.id == 1 中的  item.id ,那么就保存 tablename.id
            else
            {
                Type t = column.TableName;
                String tableName = EntityTableMapper.GetTableName(t);
                String columnName = EntityTableMapper.GetColoumName(t, column.ColumnName);
                sb.Append($" {tableName}.{columnName} ");
            }
            return sb.ToString();
        }
        private SqlOperatorEnum SqlEnumConvert(SqlOperatorEnum sqlOperatorEnum)
        {
            if (sqlOperatorEnum == SqlOperatorEnum.Equal)
            {
                return SqlOperatorEnum.EqualNull;
            }
            if (sqlOperatorEnum == SqlOperatorEnum.NotEqual)
            {
                return SqlOperatorEnum.EqualNotNull;
            }
            return SqlOperatorEnum.None;
        }
    }
}
