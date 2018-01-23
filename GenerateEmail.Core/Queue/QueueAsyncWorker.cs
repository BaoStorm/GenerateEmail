using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
namespace GenerateEmail.Core
{
    public class QueueAsyncWorker
    {
        public QueueAsyncWorker(int? parallelExcuteCount)
        {
            if (parallelExcuteCount != null)
                this.ParallelExcuteCount = parallelExcuteCount.Value;
            _actionQueue = new ConcurrentQueue<QueueTask>();
            _groupKey = new object();
            _counterLock = new object();
            _startLock = new object();
        }

        public int ParallelExcuteCount { get; set; }
        public int CurrentParallelExcuteCount { get; private set; }
        public bool IsBusy { get; private set; }
        private readonly ConcurrentQueue<QueueTask> _actionQueue;

        private object _groupKey;
        private readonly object _counterLock;
        private readonly object _startLock;

        private bool CanStartNext()
        {
            lock (_counterLock)
            {
                return CurrentParallelExcuteCount < ParallelExcuteCount;
            }
        }

        private void CounterAdd()
        {
            lock (_counterLock)
            {
                CurrentParallelExcuteCount++;
            }
        }

        private void CounterReduce()
        {
            lock (_counterLock)
            {
                CurrentParallelExcuteCount--;
            }
        }

        public void Clear()
        {
            if (_actionQueue.Count == 0) return;
            QueueTask queueTask;
            while (_actionQueue.TryDequeue(out queueTask)) { }
        }

        public void Add(QueueTask t)
        {
            _actionQueue.Enqueue(t);
            lock (_startLock)
            {
                if (!IsBusy)
                {
                    IsBusy = true;
                    Task.Factory.StartNew(DequeueGo);
                }
            }
        }

        private void DequeueGo()
        {
            do
            {
                if (!CanStartNext())
                {
                    Thread.Sleep(1000);
                    continue;
                }
                QueueTask queueTask = null;
                var isOk = _actionQueue.TryDequeue(out queueTask);
                if (isOk)
                {
                    CounterAdd();
                    Task.Factory.StartNew(() =>
                    {
                        queueTask.CurrentTask(queueTask.Param);
                    }).ContinueWith(t =>
                    {
                        CounterReduce();
                        try
                        {
                            t.Dispose();
                        }
                        catch { }
                    });
                }
                else
                {
                    IsBusy = false;
                    break;
                }
            } while (true);
        }
    }


    public class QueueTask
    {
        public object Param;
        public Action<object> CurrentTask;
    }
}