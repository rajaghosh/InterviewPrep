﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SingletonLogger.Logger
{
    public interface ILog
    {
        void LogException(string message);
    }
}
