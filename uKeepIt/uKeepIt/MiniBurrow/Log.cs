using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace uKeepIt.MiniBurrow
{
    public class LogLevel
    {
        // We need to keep ints here, because the compiler expects compile time constants in default function arguments
        public const int Error = 20;
        public const int Warning = 10;
        public const int Info = 5;
        public const int Debug = 1;
        public const int None = 0;

        public static string StringFromLevel(int level)
        {
            if (level >= Error) return "Error";
            if (level >= Warning) return "Warning";
            if (level >= Info) return "Info";
            if (level >= Debug) return "Debug";
            return "";
        }
    }

    public class Log
    {

        public event EventHandler<LogEntryEventArgs> NewEntry;
        
        public void Message(int level, string text) {
            if (level == LogLevel.None) return;
            var entry = new LogEntryEventArgs(level, text);
            Static.SynchronizationContext.Post(new SendOrPostCallback(obj => { if (NewEntry != null) NewEntry(null, entry); }), null);
        }

        public void Error(string text) { Message(LogLevel.Error, text); }
        public void Warning(string text) { Message(LogLevel.Warning, text); }
        public void Info(string text) { Message(LogLevel.Info, text); }
        public void Debug(string text) { Message(LogLevel.Debug, text); }
    }

    public class LogEntryEventArgs: EventArgs
    {
        public readonly int Level;
        public readonly string Text;

        public LogEntryEventArgs(int level, string text)
        {
            this.Level = level;
            this.Text = text;
        }

        public override string ToString()
        {
            return this.Level + ": " + this.Text;
        }
    }
}
