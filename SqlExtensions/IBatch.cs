﻿using Dapper;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dapper.Framework.SqlExtensions
{
    public interface IBatch
    {
        String SqlBuilder { get; set; }
        DynamicParameters DynamicParameters { get; set; }
    }
}
