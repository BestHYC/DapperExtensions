using System;
using System.Collections.Generic;
using System.Text;

namespace Dapper.Framework
{
    /// <summary>
    /// 实体中此字段不做sql处理
    /// 常见中为
    /// public int Flag{get;set;}
    /// [Ignore]
    /// public String FlagString{
    /// get
    ///    {
    ///     if(Flag==0)
    ///        return"确认";
    ///     else return "取消";
    ///    }
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class IgnoreAttribute : Attribute
    {
    }
}
