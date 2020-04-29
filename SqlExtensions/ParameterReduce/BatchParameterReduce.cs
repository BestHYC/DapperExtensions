using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Dapper.Framework
{
    /// <summary>
    /// Batch数据解析
    /// 因为包含Head与Where,所以只能通过Wrapper包含
    /// </summary>
    public sealed class BatchParameterReduce
    {
        public RenewalSqlHelper RenewalHelper { get; }
        public DynamicParameters Parameters { get; }
        public BatchParameterReduce()
        {
            RenewalHelper = new RenewalSqlHelper();
            Parameters = new DynamicParameters();
        }
        public List<ColumnRelevanceMapper> GetHeader()
        {
            return RenewalHelper.Header;
        }
        public TableRelevanceMapper GetTable()
        {
            return RenewalHelper.Table;
        }
        public List<ColumnRelevanceMapper> GetWhere()
        {
            return RenewalHelper.Where;
        }
        public BatchParameterReduce AddTable(Type t, TableOperatorEnum tableOperator)
        {
            RenewalHelper.Table.TableName = t;
            RenewalHelper.Table.TableOperatorEnum = tableOperator;
            return this;
        }
        public BatchParameterReduce AddHeader(String header, Type t = null)
        {
            if (!String.IsNullOrEmpty(header))
            {
                RenewalHelper.AddHeader(
                    new ColumnRelevanceMapper() { ColumnName = header, TableName = t });
            }
            return this;
        }
        public BatchParameterReduce AddHeader(Expression expression)
        {
            ConvertExpression(expression, RenewalHelper.Header);
            return this;
        }
        public BatchParameterReduce AddWhere(ColumnRelevanceMapper column)
        {
            RenewalHelper.AddWhere(column);
            return this;
        }
        public BatchParameterReduce AddWhere(Expression expression)
        {
            SqlOperatorEnum operatorEnum = SqlOperatorEnum.None;
            if (RenewalHelper.WhereCount() != 0)
            {
                operatorEnum = SqlOperatorEnum.And;
            }
            ConvertExpression(expression, RenewalHelper.Where, operatorEnum);
            return this;
        }
        public BatchParameterReduce AddParameters(String name, Object value)
        {
            if (value == null)
            {
                throw new Exception("参数化不应为null");
            }
            Parameters.Add(name, value);
            return this;
        }
        public BatchParameterReduce AddDynamicParams(Object obj)
        {
            Parameters.AddDynamicParams(obj);
            return this;
        }

        private void ConvertExpression(Expression expression, List<ColumnRelevanceMapper> columns, SqlOperatorEnum operatorEnum = SqlOperatorEnum.None)
        {
            ParameterReduce parameterReduce = new ParameterReduce(Parameters, columns);
            if (operatorEnum != SqlOperatorEnum.None)
            {
                parameterReduce.AddOperator(operatorEnum);
            }
            expression.ConvertExpression(parameterReduce);
        }
    }
}
