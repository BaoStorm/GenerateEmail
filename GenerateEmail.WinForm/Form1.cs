using GenerateEmail.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GenerateEmail.WinForm
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Console.ForegroundColor = ConsoleColor.Red; //设置字体颜色为红色 
            string keyword = "abcdefghijklmnopqrstuvwzyx0123456789._";
            Generate generate = new Generate(6, keyword, 500);
            Task.Factory.StartNew(() =>
            {
                generate.Setup();
            });
            
        }
    }
}
