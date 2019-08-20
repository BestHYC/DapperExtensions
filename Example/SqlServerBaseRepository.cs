using Dapper.Framework.SqlExtensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dapper.Framework.Example
{
    public class SqlServerBaseRepository<T> : BaseRepository<T> where T : IEntity, new()
    {

        public SqlServerBaseRepository() : base("server=xxxx;uid=xx;pwd=xx;database=xxxx;")
        {

        }

        public override SqlVarietyEnum CreateSqlEnum()
        {
            return SqlVarietyEnum.SqlServer;
        }
    }
}
