using System;
using System.Collections.Generic;
using System.Text;

namespace Dapper.Framework
{
    public enum SqlOperatorEnum
    {
        /// <summary>
        /// 无,初始化使用
        /// </summary>
        None ,
        /// <summary>
        /// 左小括号 (
        /// </summary>
        LeftBrace,
        /// <summary>
        /// 右小括号 )
        /// </summary>
        RightBrace,
        /// <summary>
        /// 相等于 = 
        /// </summary>
        Equal,
        /// <summary>
        /// 不等于 !=
        /// </summary>
        NotEqual,
        /// <summary>
        /// 为空 is null
        /// </summary>
        EqualNull,
        /// <summary>
        /// 不为空 is not null
        /// </summary>
        EqualNotNull,
        /// <summary>
        /// 以及 and 
        /// </summary>
        And,
        /// <summary>
        /// 或 or
        /// </summary>
        Or,
        /// <summary>
        /// 选择限定性条件 in
        /// </summary>
        In,
        /// <summary>
        /// On 条件
        /// </summary>
        On,
        /// <summary>
        /// where 条件
        /// </summary>
        Where,
        /// <summary>
        /// 排序 order by 
        /// </summary>
        OrderBy,
        /// <summary>
        /// having 排序
        /// </summary>
        Having,
        /// <summary>
        /// 选择
        /// </summary>
        Select,
        /// <summary>
        /// 大于等于
        /// </summary>
        GreaterThan,
        /// <summary>
        /// 大于
        /// </summary>
        GreaterThanOrEqual,
        /// <summary>
        /// 小于等于
        /// </summary>
        LessThanOrEqual,
        /// <summary>
        /// 小于
        /// </summary>
        LessThan,
        /// <summary>
        /// top 选择
        /// </summary>
        Top,
    }
    public enum TableOperatorEnum
    {
        /// <summary>
        /// 无连接
        /// </summary>
        None,
        /// <summary>
        /// 左连接
        /// </summary>
        LeftJoin,
        /// <summary>
        /// 内连接
        /// </summary>
        InnerJoin,
        /// <summary>
        /// 右连接
        /// </summary>
        RightJoin,
        /// <summary>
        /// 插入
        /// </summary>
        Insert,
        /// <summary>
        /// 修改
        /// </summary>
        Update,
        /// <summary>
        /// 删除
        /// </summary>
        Delete,
    }
    /// <summary>
    /// 表结构关联标准语法
    /// </summary>
    public static class SqlOperatorExtention
    {
        
        public static String Top = "top";
        public static String Select = " select ";
        /// <summary>
        /// 左括号
        /// </summary>
        public static String LeftBrace = " ( ";
        /// <summary>0
        /// 右括号
        /// </summary>
        public static String RightBrace = " ) ";
        /// <summary>
        /// 等于
        /// </summary>
        public static String Equal = " = ";
        /// <summary>
        /// 不等于
        /// </summary>
        public static String NotEqual = " != ";
        /// <summary>
        /// 为空
        /// </summary>
        public static String EqualNull = " is null ";
        /// <summary>
        /// 不为空
        /// </summary>
        public static String EqualNotNull = " is not null ";
        /// <summary>
        /// 与
        /// </summary>
        public static String And = " and ";
        /// <summary>
        /// 或
        /// </summary>
        public static String Or = " or ";
        /// <summary>
        /// in
        /// </summary>
        public static String In = " in ";
        /// <summary>
        /// 
        /// </summary>
        public static String Having = " having ";
        public static String GreaterThan = " >= ";
        public static String Greater = " > ";
        public static String LessThan = " <= ";
        public static String Less = " < ";
        public static String InnerJoin = " inner join ";
        public static String LeftJoin = " left join ";
        public static String RightJoin = " right join ";
        public static String On = " on ";
        public static String OrderBy = " order by ";
        public static String Where = " where ";
        public static String Insert = " insert into ";
        public static String Update = " Update ";
        public static String Delete = "delete from ";
        public static String GetOperator(this TableOperatorEnum table)
        {
            switch (table)
            {
                case TableOperatorEnum.InnerJoin:
                    return InnerJoin;
                case TableOperatorEnum.LeftJoin:
                    return LeftJoin;
                case TableOperatorEnum.RightJoin:
                    return RightJoin;
                case TableOperatorEnum.Insert:
                    return Insert;
                case TableOperatorEnum.Update:
                    return Update;
                case TableOperatorEnum.Delete:
                    return Delete;
                default:return "";
            }
        }

        public static String GetOperator(this SqlOperatorEnum sql)
        {
            switch (sql)
            {
                case SqlOperatorEnum.And:
                    return And;
                case SqlOperatorEnum.Equal:
                    return Equal;
                case SqlOperatorEnum.EqualNotNull:
                    return EqualNotNull;
                case SqlOperatorEnum.EqualNull:
                    return EqualNull;
                case SqlOperatorEnum.Having:
                    return Having;
                case SqlOperatorEnum.In:
                    return In;
                case SqlOperatorEnum.LeftBrace:
                    return LeftBrace;
                case SqlOperatorEnum.NotEqual:
                    return NotEqual;
                case SqlOperatorEnum.On:
                    return On;
                case SqlOperatorEnum.Or:
                    return Or;
                case SqlOperatorEnum.OrderBy:
                    return OrderBy;
                case SqlOperatorEnum.RightBrace:
                    return RightBrace;
                case SqlOperatorEnum.Select:
                    return Select;
                case SqlOperatorEnum.Where:
                    return Where;
                case SqlOperatorEnum.LessThan:
                    return Less;
                case SqlOperatorEnum.LessThanOrEqual:
                    return LessThan;
                case SqlOperatorEnum.GreaterThan:
                    return Greater;
                case SqlOperatorEnum.GreaterThanOrEqual:
                    return GreaterThan;
                case SqlOperatorEnum.Top:
                    return Top;
                default:
                    return "";
            }
        }
        /// <summary>
        /// 获取别名,简化操作
        /// </summary>
        private static String strArray = "ABCDEFGHJKMNOPQRSTWXYZabcdefhijkmnoprstwxyz";
        public static String RandromName(this Object obj)
        {
            StringBuilder sb = new StringBuilder();
            Random random = new Random(Guid.NewGuid().GetHashCode());
            for (Int32 i = 0; i < 7; i++)
            {
                Int32 s = random.Next(0, strArray.Length - 1);
                sb.Append(strArray[s]);
            }
            return sb.ToString();
        }
    }
}
