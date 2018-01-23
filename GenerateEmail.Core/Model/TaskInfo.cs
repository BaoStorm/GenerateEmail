using System;
using System.Collections.Generic;
using System.Text;

namespace GenerateEmail.Core.Model
{
    public class TaskInfo
    {
        public List<int> Start { set; get; }
        public List<int> End { set; get; }
        public string Keywords { set; get; }

        public TaskInfo()
        {
            Start = new List<int>();
            End = new List<int>();
        }
    }
}
