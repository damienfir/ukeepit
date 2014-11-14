using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace SafeBox.Burrow
{
    // For multiple tasks.
    public class TaskGroup
    {
        public delegate void HandlerDelegate();

        public readonly System.Threading.SynchronizationContext SynchronizationContext;
        private HandlerDelegate handler = null;
        private int Expecting = 1;

        public TaskGroup()
        {
            SynchronizationContext = SynchronizationContext.Current;
        }

        // Asynchronously run a function, and put the result into a Future.
        public Future<T> Run<T>(Asynchronous.AsyncDelegate<T> asyncTask, Asynchronous.SyncDelegate<T> syncTask = null)
        {
            // assert(handler == null)
            Expecting += 1;
            var future = new Future<T>(this);
            Asynchronous.Run(asyncTask, (result) => { if (syncTask != null) syncTask(result); future.Done(result); });
            return future;
        }

        // A task running synchronously can call this to register itself.
        public Future<T> WaitForMe<T>()
        {
            // assert(handler == null)
            Expecting += 1;
            return new Future<T>(this);
        }

        // This must be called exactly once from the main thread. Once this is called, the handler may be called any time. 
        public void WhenDone(HandlerDelegate handler)
        {
            // assert(handler == null)
            this.handler = handler;
            Done();
        }

        private void Done()
        {
            Expecting -= 1;
            if (Expecting == 0) handler();
        }

        public class Future<T>
        {
            private readonly TaskGroup TaskGroup;
            public T Result;
            public bool IsDone;

            public Future(TaskGroup taskGroup) {
                this.TaskGroup = taskGroup;
            }

            public void Done(T result) {
                this.Result = result;
                TaskGroup.Done();
            }
        }
    }
}
