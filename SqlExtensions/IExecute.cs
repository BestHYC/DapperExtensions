using System;
using System.Collections.Generic;
using System.Text;

namespace Dapper.Framework.SqlExtensions
{
    public interface IExecute
    {
        IBatch End();
    }
}
