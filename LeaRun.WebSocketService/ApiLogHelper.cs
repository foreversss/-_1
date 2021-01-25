using System;
using System.Collections.Generic;
using System.Web;
using System.IO;
using System.Text;

namespace LeaRun.WebSocketService
{

    /// <summary>
    /// 日志帮助类
    /// </summary>
    public class ApiLoghelper
    {
        //在网站根目录下创建日志目录
        public static string path = "logs";

        //商户编号作为日志
        public static string merchant_code = "Unknown_Error";

        /**
         * 向日志文件写入调试信息
         * @param className 类名
         * @param content 写入内容
         */
        public static void Debug(string className, string content)
        {
            WriteLog("DEBUG", className, content);
        }

        /**
        * 向日志文件写入运行时信息
        * @param className 类名
        * @param content 写入内容
        */
        public static void Info(string className, string content)
        {
            WriteLog("INFO", className, content);
        }

        /**
        * 向日志文件写入出错信息
        * @param className 类名
        * @param content 写入内容
        */
        public static void Error(string className, string content)
        {

            WriteLog("ERROR", className, content);

        }

        /**
        * 实际的写日志操作
        * @param type 日志记录类型
        * @param className 类名
        * @param content 写入内容
        */
        protected static void WriteLog(string type, string className, string content)
        {
            try
            {
                string NewPath = System.IO.Directory.GetCurrentDirectory() + "/log";//商户编号为日志文件夹

                if (!Directory.Exists(NewPath))//如果日志目录不存在就创建
                {
                    Directory.CreateDirectory(NewPath);
                }

                string time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");//获取当前系统时间
                string filename = NewPath + "/" + DateTime.Now.ToString("yyyy-MM-dd") + ".log";//用日期对日志文件命名

                //向日志文件写入内容
                string write_content = time + " " + type + " " + className + ": " + content + "\r\n";

                //创建或打开日志文件，向日志文件末尾追加记录
                using (StreamWriter mySw = File.AppendText(filename))
                {
                    mySw.WriteLine(write_content);

                    //关闭日志文件
                    mySw.Close();
                }
            }
            catch(Exception ex)
            {
                string ss = ex.Message;
            }
        }
    }
}