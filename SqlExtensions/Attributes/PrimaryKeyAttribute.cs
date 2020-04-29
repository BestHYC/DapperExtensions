using System;
using System.Collections.Generic;
using System.Text;

namespace Dapper.Framework
{
    /// <summary>
    /// 主键特性,对于orm来说,基本上只是一个标识
    /// </summary>
    public class PrimaryKeyAttribute : Attribute
    {
    }
    /// <summary>
    /// 自增特性
    /// </summary>
    public class IncreaseKeyAttribute : Attribute
    {

    }
}
