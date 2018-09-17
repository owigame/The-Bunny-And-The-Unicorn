namespace Logging
{
    public abstract class Logging
    {
        protected LogLevel logMask;

        // The next Handler in the chain
        protected Logging next;

        public Logging(LogLevel mask)
        {
            this.logMask = mask;
        }

        /// <summary>
        /// Sets the Next logger to make a list/chain of Handlers.
        /// </summary>
        public Logging SetNext(Logging nextlogger)
        {
            next = nextlogger;
            return nextlogger;
        }

        public void Message(string msg, LogLevel severity)
        {
            if ((severity & logMask) != 0) //True only if all logMask bits are set in severity
            {
                WriteMessage(msg);
            }
            if (next != null)
            {
                next.Message(msg, severity);
            }
        }

        abstract protected void WriteMessage(string msg);
    }
}