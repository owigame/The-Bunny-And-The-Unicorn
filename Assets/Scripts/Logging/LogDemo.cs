using UnityEngine;
using Logging;
using System.Collections;

public class LogDemo : MonoBehaviour
{
    [SerializeField][EnumFlag] LogLevel LoggingLevel;

    [ContextMenu("Log Demo")]
    void Start()
    {
        Logging.Logging logger, logger1;
        logger = new ConsoleLogger(LogLevel.All);
        // chain the stack log to follow the console. makes it reachable through logger.message
        logger1 = logger.SetNext(new StackLogger( LogLevel.Stack,5));

        logger.Message("None Log", LogLevel.None);
        logger.Message("All Log", LogLevel.All);
        logger.Message("Debug log", LogLevel.Debug);
        logger.Message("Stack log", LogLevel.Stack);

        logger.Message("Log level :"+LoggingLevel, LoggingLevel );


        Debug.Log("--- Stack Contents ---");
        IEnumerator stackMessages = (logger1 as StackLogger).LogStack.GetEnumerator();
        while (stackMessages.MoveNext())
        {
            object item = stackMessages.Current;
            // Perform logic on the item
            Debug.Log("> "+item);
        }
        Debug.Log("--- End Stack Contents ---");
    }
}
