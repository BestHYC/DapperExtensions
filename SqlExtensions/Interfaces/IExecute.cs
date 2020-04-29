using System;
using System.Collections.Generic;
using System.Text;

namespace Dapper.Framework
{
    public interface IExecute
    {
        IBatch End();
    }
}
