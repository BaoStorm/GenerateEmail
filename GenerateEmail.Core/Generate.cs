using GenerateEmail.Core.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;

namespace GenerateEmail.Core
{
    public class Generate
    {
        private string _mailFrom => "check@verify-email.org";
        private int _timeout = 10000;

        private string _keywords;
        public string Keywords => _keywords;

        private List<int> _list;
        public List<int> List => _list;

        private QueueAsyncWorker _queueAsyncWorker;
        private QueueAsyncWorker _emailQueueAsyncWorker;

        private string _emailSuffix;

        public string EmailSuffix => _emailSuffix;

        public string MailServer;

        public Generate(int count,string keywords,int parallelExcuteCount,string emailSuffix)
        {
            //Console.WriteLine(DateTime.Now.ToString("HH:mm:ss ffffff") + " 开始");
            _keywords = keywords;
            _list = new List<int>();
            _emailSuffix = emailSuffix;
            MailServer = new CheckEmail(10000).getMailServer("a" + _emailSuffix, true);
            for (int i = 0; i < count; i++)
            {
                _list.Add(-1);
            }
            _queueAsyncWorker = new QueueAsyncWorker(parallelExcuteCount);
            _emailQueueAsyncWorker = new QueueAsyncWorker(100);
        }

        public void Setup()
        {
            var taskInfos = new List<TaskInfo>();
            int segmentation = 5;
            var empty = new List<int>();
            for (int i = 0; i < segmentation; i++)
            {
                empty.Add(-1);
            }
            var end = new List<int>();
            for (int i = 0; i < _list.Count-segmentation; i++)
            {
                end.Add(-1);
            }
            if (_list.Count > segmentation)
            {
                
                var list = new List<int>();
                list.AddRange(_list.Take(_list.Count - segmentation));
                int i = 0;
                //分段穷举
                while (list[i] < _keywords.Length)
                {
                    list[i]++;
                    if (list[i] == _keywords.Length)
                    {

                        if (i + 1 == list.Count)
                        {
                            //超出数组范围停止
                            break;
                        }
                        else
                        {
                            //等待下一位+1
                            list[i] = -1;
                            i++;
                            continue;
                        }
                    }
                    else if (list[i] < _keywords.Length && i > 0)
                    {
                        //等待下一位+1
                        i--;
                        continue;
                    }
                    else
                    {
                        var taskinfo = new TaskInfo();
                        taskinfo.Keywords = _keywords;
                        taskinfo.Start = new List<int>();
                        taskinfo.Start.AddRange(empty);
                        taskinfo.Start.AddRange(end);
                        taskinfo.End = new List<int>();
                        taskinfo.End.AddRange(empty);
                        taskinfo.End.AddRange(list);
                        end = new List<int>();
                        end.AddRange(list);
                        taskInfos.Add(taskinfo);
                        _queueAsyncWorker.Add(new QueueTask()
                        {
                            CurrentTask = Perform,
                            Param = taskinfo
                        });
                    }
                }
            }
            else
            {
                var taskinfo = new TaskInfo();
                taskinfo.Keywords = _keywords;
                taskinfo.Start.AddRange(_list);
                for (int i = 0; i < _list.Count; i++)
                {
                    _list[i] = _keywords.Length - 1;
                }
                taskinfo.End.AddRange(_list);
                _queueAsyncWorker.Add(new QueueTask()
                {
                    CurrentTask=Perform,
                    Param=taskinfo
                });
            }
        }
        /// <summary>
        /// 穷举执行方法
        /// </summary>
        /// <param name="obj"></param>
        public void Perform(object obj)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            var taskinfo = obj as TaskInfo;
            var start = taskinfo.Start;
            var end = taskinfo.End;
            var keywords = taskinfo.Keywords;
            int i = 0;
            //end = new List<int>() { 37, 37, 37,2 };
            while (start[i] < keywords.Length)
            {
                start[i]++;
                if (IsExcessive(start, end))
                {
                    //超出结束范围停止
                    break;
                }
                if (start[i] == keywords.Length)
                {

                    if (i + 1 == start.Count)
                    {
                        //超出数组范围停止
                        break;
                    }
                    else
                    {
                        //等待下一位+1
                        start[i] = -1;
                        i++;
                        continue;
                    }
                }
                else if(start[i]<keywords.Length&&i>0)
                {
                    //等待下一位+1
                    i--;
                    continue;
                }
                else
                {
                    string email = "";
                    foreach (var item in start)
                    {
                        if (item >= 0)
                            email += keywords[item];
                    }
                    CheckEmail(email + EmailSuffix);
                    //Console.WriteLine(str);
                    //Console.WriteLine(string.Join(",", start));
                    //_emailQueueAsyncWorker.Add(new QueueTask()
                    //{
                    //    CurrentTask = CheckEmail,
                    //    Param = email + EmailSuffix
                    //});
                }
            }
            stopwatch.Stop();
            //Console.WriteLine(DateTime.Now.ToString("HH:mm:ss ffffff") + "  " + string.Join(",", end) + "结束 耗时" + stopwatch.ElapsedMilliseconds);
        }
        /// <summary>
        /// 是否超过
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        private bool IsExcessive(List<int> start,List<int> end)
        {
            bool flag = false;
            for (int i = end.Count - 1; i >= 0; i--)
            {
                if (end[i] < 0)
                {
                    continue;
                }
                if (end[i] > 0) {
                    flag = true;
                }
                if (flag)
                {
                    if (start[i] > end[i])
                    {
                        return true;
                    }
                    if (start[i] <= end[i])
                    {
                        return false;
                    }
                }
            }
            return false;
        }


        private void CheckEmail(object obj)
        {
            var email = obj as string;
            CheckEmail checkEmail = new Core.CheckEmail(_timeout);
            if(checkEmail.Check(_mailFrom, email, MailServer))
            {
                Console.WriteLine(email);
            }
            
        }
    }
}
