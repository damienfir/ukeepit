using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace SafeBox.Burrow
{
    public class Asynchronous
    {
        public delegate T AsyncDelegate<T>();
        public delegate void SyncDelegate<T>(T obj);

        // This has to be called from the main thread
        public static Future<T> Run<T>(AsyncDelegate<T> asyncTask, SyncDelegate<T> syncTask = null)
        {
            return new Future<T>(asyncTask, syncTask);
        }

        public class Future<T>
        {
            public readonly System.Threading.SynchronizationContext SynchronizationContext;
            private readonly AsyncDelegate<T> AsyncTask;
            private readonly SyncDelegate<T> SyncTask;
            public T Result;

            internal Future(AsyncDelegate<T> asyncTask, SyncDelegate<T> syncTask)
            {
                SynchronizationContext = SynchronizationContext.Current;
                AsyncTask = asyncTask;
                SyncTask = syncTask;
                ThreadPool.QueueUserWorkItem(new WaitCallback(RunAsync));
            }

            private void RunAsync(Object dummy)
            {
                Result = AsyncTask();
                SynchronizationContext.Post(new SendOrPostCallback(RunSync), null);
            }

            private void RunSync(Object dummy)
            {
                if (SyncTask != null) SyncTask(Result);
            }
        }
    }
}
