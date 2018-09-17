using UnityEngine;
using Logging;
using System.Collections;

public class LogStack : MonoBehaviour
{
    #region  Singleton management
    private static LogStack _instance;
    Logging.Logging logger,LoggerSystem ,loggerStack;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            LogLevel console = (int)LogLevel.All - LogLevel.System;
            logger = new ConsoleLogger(console);
            // chain the stack log to follow the console. makes it reachable through logger.message
            LoggerSystem = logger.SetNext(new SystemLogger(LogLevel.System | LogLevel.System));
            loggerStack = LoggerSystem.SetNext(new StackLogger(LogLevel.Stack, 5));
        }
        else
        {
            Destroy(this);
        }
    }

    private void OnDestroy()
    {
        _instance = null;
    }
    #endregion

    public static void Log(string msg, LogLevel severity)
    {
        _instance.logger.Message(msg, severity);
    }

    public static IEnumerator StackEnumerator()
    {
        return (_instance.loggerStack as StackLogger).LogStack.GetEnumerator();
    }

    [ContextMenu("Log Stack")]
    public void LogOutStack()
    {
        Debug.Log("--- Stack Contents ---");
        IEnumerator stackMessages = (_instance.loggerStack as StackLogger).LogStack.GetEnumerator();
        while (stackMessages.MoveNext())
        {
            object item = stackMessages.Current;
            // Perform logic on the item
            Debug.Log("> " + item);
        }
        Debug.Log("--- End Stack Contents ---");
    }
}
