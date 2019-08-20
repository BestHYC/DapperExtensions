using System;
using System.Collections.Generic;
using System.Text;

namespace Dapper.Framework.Example
{
    public class Unittest
    {
        public void Test()
        {
            StudentInfoBusiness business = new StudentInfoBusiness();
            StudentInfo info = business.Get(1);
            info.Stu_ID = "123";
            business.Insert(info);
            StudentInfo info1 = business.Get(item => item.Stu_ID == "123");
            info1.Stu_ID = "234";
            business.Update(info1);
            business.Delete(info1);
            business.Delete(item => item.Stu_ID == "234");
            
        }
    }
}
