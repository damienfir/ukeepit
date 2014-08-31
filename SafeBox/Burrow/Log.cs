using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace SafeBox.Burrow
{
    public class LogEntryType
    {
        public static LogEntryType Error = new LogEntryType("Error");
        public static LogEntryType Warning = new LogEntryType("Warning");
        public static LogEntryType Info = new LogEntryType("info");
        public static LogEntryType Debug = new LogEntryType("Debug");
       
        public string Label;
        public LogEntryType(string label) { this.Label = label; }
    };

    public class Log
    {
        public event EventHandler<LogEntryEventArgs> NewEntry;
        
        public void Message(LogEntryType type, string text) {
            var entry = new LogEntryEventArgs(type, text);
            Static.SynchronizationContext.Post(new SendOrPostCallback(obj => { if (NewEntry != null) NewEntry(null, entry); }), null);
        }

        public void Error(string text) { Message(LogEntryType.Error, text); }
        public void Warning(string text) { Message(LogEntryType.Warning, text); }
        public void Info(string text) { Message(LogEntryType.Info, text); }
        public void Debug(string text) { Message(LogEntryType.Debug, text); }
    }

    public class LogEntryEventArgs: EventArgs
    {
        public readonly LogEntryType Type;
        public readonly string Text;

        public LogEntryEventArgs(LogEntryType type, string text)
        {
            this.Type = type;
            this.Text = text;
        }

        public override string ToString()
        {
            return this.Type.Label + ": " + this.Text;
        }
    }
}
