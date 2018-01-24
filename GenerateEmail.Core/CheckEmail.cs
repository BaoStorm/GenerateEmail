using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;

namespace GenerateEmail.Core
{
    public class CheckEmail
    {
        private TcpClient tcpClient;
        private byte[] bytes;
        private NetworkStream networkStream;

        public CheckEmail(int timeOut)
        {
        }

        private void Connect(string mailServer)
        {
            try
            {
                tcpClient.Connect(mailServer, 25);
                networkStream = tcpClient.GetStream();
                var len = networkStream.Read(bytes, 0, bytes.Length);
                var str = Encoding.UTF8.GetString(bytes);
            }
            catch (Exception e)
            {


            }
        }

        private bool SendCommand(string command)
        {
            try
            {

                var arrayToSend = Encoding.UTF8.GetBytes(command.ToCharArray());
                networkStream.Write(arrayToSend, 0, arrayToSend.Length);
                var len = networkStream.Read(bytes, 0, bytes.Length);
                var str = Encoding.UTF8.GetString(bytes);
                if (str.StartsWith("250"))
                    return true;
                else
                    return false;
            }
            catch (IOException e)
            {
                return false;
            }
        }

        public bool Check(string mailForm, string checkemail,string mailServer)
        {
            try
            {

                tcpClient = new TcpClient();
                tcpClient.NoDelay = true;
                tcpClient.ReceiveTimeout = 3000;
                tcpClient.SendTimeout = 3000;
                bytes = new byte[512];
                //var mailServer = getMailServer(checkemail, true);
                Connect(mailServer);
                string command = "helo " + mailServer + "\r\n"; ////写入HELO命令 
                var flag = SendCommand(command);
                if (flag == false)
                {
                    return false;
                }
                command = "mail from:<" + mailForm + ">" + "\r\n"; ////写入Mail From命令  
                flag = SendCommand(command);
                if (flag == false)
                {
                    return false;
                }
                command = "rcpt to:<" + checkemail + ">" + "\r\n";//写入RCPT命令，这是关键的一步，后面的参数便是查询的Email的地址  
                flag = SendCommand(command);
                if (flag == true)
                {
                    return true; //邮箱存在  
                }
                else
                {
                    return false; //邮箱不存在  
                }
            }
            catch(Exception ex)
            {
                return false;
            }
            finally
            {
                tcpClient.Close();
            }
        }
        public string getMailServer(string strEmail, bool IsCheck)
        {
            var strDomain = strEmail.Split('@')[1];
            ProcessStartInfo info = new ProcessStartInfo();   //指定启动进程时使用的一组值。  
            info.UseShellExecute = false;
            info.RedirectStandardInput = true;
            info.RedirectStandardOutput = true;
            info.FileName = "nslookup";
            info.CreateNoWindow = true;
            info.Arguments = "-type=mx " + strDomain;
            Process ns = Process.Start(info);        //提供对本地和远程进程的访问并使您能够启动和停止本地系统进程。  
            StreamReader sout = ns.StandardOutput;

            Regex reg = new Regex(@"mail exchanger = (?<mailServer>[^\s]+)");
            string strResponse = "";
            while ((strResponse = sout.ReadLine()) != null)
            {

                Match amatch = reg.Match(strResponse);   // Match  表示单个正则表达式匹配的结果。  

                if (reg.Match(strResponse).Success)
                {
                    return amatch.Groups["mailServer"].Value;   //获取由正则表达式匹配的组的集合  

                }
            }
            return null;
        }
    }
}
