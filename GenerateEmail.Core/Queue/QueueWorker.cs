using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using ThreadState = System.Threading.ThreadState;
using Timer = System.Timers.Timer;

namespace GenerateEmail.Core
{
    /// <summary>
    /// 并发队列
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class QueueWorker<T> where T:class 
    {
        /// <summary>
        /// 队列
        /// </summary>
        private readonly ConcurrentQueue<T> _queue;

        private readonly List<T> _list;
        /// <summary>
        /// 线程数组
        /// </summary>
        private readonly Thread[] _threads;
        /// <summary>
        /// Lock Object
        /// </summary>
        private readonly object _syncObject = new object();

        private readonly Timer _timer;

        private readonly Stopwatch _stopwatch;
        /// <summary>
        /// 是否繁忙
        /// </summary>
        public bool IsBusy { get; private set; }

        #region 委托

        /// <summary>
        /// 运行时委托
        /// </summary>
        /// <param name="target">
        /// 对象
        /// </param>
        public delegate void RunQueueEventHandler(T target,int index,int total);
        /// <summary>
        /// 完成委托
        /// </summary>
        public delegate void CompletedQueueEventHandler(TimeSpan timeSpan);

        #endregion

        #region 事件

        /// <summary>
        /// 运行时事件
        /// </summary>
        public event RunQueueEventHandler RunQueueEvent;
        /// <summary>
        /// 完成时事件
        /// </summary>
        public event CompletedQueueEventHandler CompletedQueueEvent;
        #endregion

        /// <summary>
        /// 构造
        /// </summary>
        /// <param name="threadCount"></param>
        /// <param name="objs"></param>
        public QueueWorker(int threadCount,List<T> objs)
        {
            _threads = new Thread[threadCount];
            _queue = new ConcurrentQueue<T>();
            _list = objs;
            foreach (var obj in objs)
            {
                //加上 队列
                _queue.Enqueue(obj);
            }
            _timer = new Timer {Interval = 100};
            _timer.Elapsed += _timer_Elapsed;
            _timer.Enabled = true;

            _stopwatch = new Stopwatch();
        }

        public QueueWorker(int threadCount)
        {
            _threads = new Thread[threadCount];
            _list = new List<T>();
            _queue = new ConcurrentQueue<T>();
            _timer = new Timer { Interval = 100 };
            _timer.Elapsed += _timer_Elapsed;
            _timer.Enabled = true;

            _stopwatch = new Stopwatch();
        }

        private void _timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (Monitor.TryEnter(_timer))
            {
                try
                {
                    if (!_queue.Any())
                    {
                        if (_threads.All(p => p == null || (p.ThreadState & ThreadState.Stopped) != 0))
                        {
                            _timer.Stop();
                            _stopwatch.Stop();
                            CompletedQueueEvent?.Invoke(_stopwatch.Elapsed);
                            IsBusy = false;
                        }
                        return;
                    }
                    for (int i = 0; i < _threads.Length; i++)
                    {
                        if (_threads[i] == null || (_threads[i].ThreadState & ThreadState.Stopped) != 0)
                        {
                            _threads[i] = new Thread(Do);
                            _threads[i].Start();
                        }
                    }
                }
                catch(Exception)
                {
                    
                }
                finally
                {
                    Monitor.Exit(_timer);
                }
            }
            
        }

        private void Do()
        {
            T info = null;
            lock (_syncObject)
            {
                if (_queue.Any())
                {
                    _queue.TryDequeue(out info);
                }
            }
            if (info == null)
                return;
            RunQueueEvent?.Invoke(info, _list.IndexOf(info), _list.Count);
        }

        /// <summary>
        /// 开始
        /// </summary>
        public void Start()
        {
            IsBusy = true;
            _stopwatch.Start();
            _timer.Start();
        }
        /// <summary>
        /// 停止
        /// </summary>
        public void Stop()
        {
            _timer.Stop();
            foreach (var thread in _threads)
            {
                if (thread != null && thread.ThreadState==ThreadState.Running)
                {
                    thread.Abort();
                }
            }
            CompletedQueueEvent?.Invoke(_stopwatch.Elapsed);
            IsBusy = false;
        }

        public void Add(T obj)
        {
            _list.Add(obj);
            _queue.Enqueue(obj);
        }
    }
}
