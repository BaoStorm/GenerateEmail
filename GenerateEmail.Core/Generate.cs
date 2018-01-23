using GenerateEmail.Core.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace GenerateEmail.Core
{
    public class Generate
    {
        private string _keywords;
        public string Keywords => _keywords;

        private List<int> _list;
        public List<int> List => _list;

        private QueueAsyncWorker _queueAsyncWorker;

        public Generate(int count,string keywords,int parallelExcuteCount)
        {
            _keywords = keywords;
            _list = new List<int>() { -1 };
            for (int i = 0; i < count-1; i++)
            {
                _list.Add(-1);
            }
            _queueAsyncWorker = new QueueAsyncWorker(parallelExcuteCount);
        }

        public void Setup()
        {
            if (_list.Count > 4)
            {
                //分段穷举
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

        public void Perform(object obj)
        {
            var taskinfo = obj as TaskInfo;
            var start = taskinfo.Start;
            var end = taskinfo.End;
            var keywords = taskinfo.Keywords;
            int i = 0;
            end = new List<int>() { 37, 37, 37,2 };
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
                    //string str = "";
                    //foreach (var item in start)
                    //{
                    //    if (item >= 0)
                    //        str += keywords[item];
                    //}
                    //Console.WriteLine(str);
                    Console.WriteLine(string.Join(",", start));
                }
            }
            Console.WriteLine("结束");
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
                    if (start[i] >= end[i])
                    {
                        return true;
                    }
                    if (start[i] < end[i])
                    {
                        return false;
                    }
                }
            }
            return false;
        }

    }
}
