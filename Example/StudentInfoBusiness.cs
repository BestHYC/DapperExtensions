using Dapper.Framework.SqlExtensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dapper.Framework.Example
{
    public class StudentInfoBusiness : SqlServerBaseRepository<StudentInfo>, IRepository<StudentInfo>
    {
        public void InsertList(IEnumerable<StudentInfo> list)
        {
            using (var conn = _executeBatch.Create.Create())
            {
                //只当做sql解析使用,不做整体
                String sql = _renewal.Insert(new StudentInfo()).End().SqlBuilder;
                foreach (var item in list)
                {
                    conn.Execute(sql, item);
                    //仓储层标准写法,但是需要每次解析
                    //_executeBatch.Execute(conn, _renewal.Insert(item).End(), null);
                }
            }
        }
    }
}
