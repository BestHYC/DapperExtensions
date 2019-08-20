using System;
using System.Collections.Generic;
using System.Text;

namespace Dapper.Framework.SqlExtensions
{
    public class MysqlBatch : IBatch
    {
        public String SqlBuilder { get; set; }
        public DynamicParameters DynamicParameters { get; set; }
    }
}
