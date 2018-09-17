using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logging
{
    public class SystemLogger : Logging
    {
        public SystemLogger(LogLevel mask)
            : base(mask)
        { }

        protected override void WriteMessage(string msg)
        {
            Debug.LogWarning("---> " + msg);
        }
    }
}