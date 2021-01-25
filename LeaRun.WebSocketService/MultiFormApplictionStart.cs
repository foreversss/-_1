using LeaRun.AttendanceService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LeaRun.WebSocketService
{
    /// <summary>
      /// 多窗口同时启动类
      /// <remarks>继承ApplicationContext的原因是Application.Run(ApplicationContext context);参数的需要</remarks>
      /// <remarks>另一个是关闭同时启动的窗口</remarks>
      /// </summary>
    class MultiFormApplictionStart : ApplicationContext
    {
        private void OnFormClosed(object sender, EventArgs e)
        {
            if (Application.OpenForms.Count == 0)
            {
                ExitThread();
            }
        }
        public MultiFormApplictionStart()
        {
            /*
             *里面添加启动的窗口
             */
            var formList = new List<Form>() 
            {
                 new Websocket(),
                 new Form2()
            };
            foreach (var item in formList)
            {
                item.FormClosed += OnFormClosed;
            }
            foreach (var item in formList)
            {
                item.Show();
            }
        }
    }
}
