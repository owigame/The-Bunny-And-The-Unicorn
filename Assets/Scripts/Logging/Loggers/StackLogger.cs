using UnityEngine;
using DataStructures;

namespace Logging
{
    public class StackLogger : Logging
    {
        public MaxStack<string> LogStack;
        public StackLogger(LogLevel mask,int maxStack)
            : base(mask)
        {
            LogStack = new MaxStack<string>(maxStack);
        }

        protected override void WriteMessage(string msg)
        {
            LogStack.Push(msg);
        }
    }
}