using Dapper.Framework.SqlExtensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dapper.Framework.Example
{
    [TableMapper("tb_stu_info")]
    public class StudentInfo : IEntity
    {
        /// <summary>
        /// 自增ID
        /// </summary>
        [PrimaryKey]
        public int ID { get; set; }
        /// <summary>
        /// 会员类型,1-会员,2-潜客
        /// </summary>
        public int Stu_Type { get; set; }
        /// <summary>
        /// 数据来源系统中ID
        /// </summary>
        public string Stu_ID { get; set; }
    }
}
