using UnityEngine;
using DataStructures;

namespace Logging
{
    public class ColorLogger : Logging
    {
        public string _color;
        public ColorLogger(LogLevel mask,string color)
            : base(mask)
        {
            _color = color;
        }

        protected override void WriteMessage(string msg)
        {
            Debug.Log("<color="+ _color + ">" + msg+ "</color>");
        }
    }
}