using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Logging
{
    [System.Flags]
    public enum LogLevel
    {
        None = 0,                 //        0
        Debug = 1,                //        1
        Stack = 2,                //       10
        System = 4,              //      100
        Color = 8,                //     1000
        //FunctionalMessage = 16,   //    10000
        //FunctionalError = 32,     //   100000
       [HideInInspector] All = 63                  //   111111
    }
}