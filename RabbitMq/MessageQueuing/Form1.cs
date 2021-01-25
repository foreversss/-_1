using Hxh.Tools;
using Hxh.Tools.RabiitMq;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using RabbitMQ.Client;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using LeaRun.Entity.Model_Entity.Children.WXModel;
using LeaRun.Business.Children;

namespace MessageQueuing
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }


        private void Form1_Load(object sender, EventArgs e)
        {
            var rabbitMqProxy = new RabbitMqService(new MqConfig
            {
                AutomaticRecoveryEnabled = true,
                NetworkRecoveryInterval = new TimeSpan(60),
                Host = "47.112.255.159",
                UserName = "HXH_Admin",
                Password = "admin123"
            });


            //订阅消息
            Subscribe(rabbitMqProxy, "Contact.ExchangeName", "Contact.QueueName", Pull);



            //订阅死信消息
            ReceiveDeadLetter(rabbitMqProxy, "letter_ContactExchangeName", "letter_ContactQueueName", Pull);
        }


        //订阅正常消息
        public void Subscribe(RabbitMqService rabbitMqProxy, string exchange, string queueName, Func<Byte[], bool> func)
        {
            //队列声明
            var channel = rabbitMqProxy.GetModel(exchange, queueName, true);

            var consumer = new EventingBasicConsumer(channel);

            consumer.Received += (model, ea) =>
            {

                if (!func(ea.Body))
                {
                    //死信内容
                    string byteStr = Encoding.UTF8.GetString(ea.Body);
                   
                    //发布消息到死信队列
                    rabbitMqProxy.PublishToDead("letter_ContactExchangeName", "letter_ContactQueueName", "letter_ContactQueueName", byteStr, true);

                   // channel.BasicConsume("letter_ContactQueueName", true, consumer);
                }
            };
          
            channel.BasicConsume(queueName, true, consumer);
        }



        /// <summary>
        /// 订阅死信消息
        /// </summary>
        /// <param name="queueName"></param>
        /// <param name="func"></param>
        public void ReceiveDeadLetter(RabbitMqService rabbitMqProxy, string exchange, string queueName, Func<Byte[], bool> func)
        {


            //队列声明
            var channel = rabbitMqProxy.GetModel(exchange, queueName, true);

            var consumer = new EventingBasicConsumer(channel);

            consumer.Received += (model, ea) =>
            {


                if (!func(ea.Body))
                {
                    //死信内容
                    string byteStr = Encoding.UTF8.GetString(ea.Body);

                    //发布消息到死信队列
                    rabbitMqProxy.PublishToDead("letter_ContactExchangeName", "letter_ContactQueueName", "letter_ContactQueueName", byteStr, true);                
                }
            };
            channel.BasicConsume(queueName, true, consumer);
        }

        /// <summary>
        /// 获取消息
        /// </summary>
        /// <param name="body"></param>
        /// <returns></returns>
        public static bool Pull(byte[] body)
        {
            try
            {
                //拿到订阅的消息体
                string byteStr = Encoding.UTF8.GetString(body);

                var urgentPeopleList = JsonHelper.JSONStringToList<UrgentPeopleModel>(byteStr);

                //调用下行数据接口
                var result = EquipmentBll.UpdateEquipmentParameters(urgentPeopleList);

                if (result == "0")
                {
                    ApiLoghelper.Info("调用下行接口成功", "消费消息成功！！！！！");
                    return true;
                              
                }
                var errorMsg = "";

                if (result == "4")
                {
                    errorMsg = "设备5分钟内都没有联网，设备没网，或者关机了";
                }

                else if  (result == "5")
                {
                    errorMsg = "表示设备不在线";
                }
                else
                {
                    errorMsg = result;
                }

                ApiLoghelper.Error("调用下行接口失败 原因:", errorMsg);

                return false;       
            }
            catch (Exception)
            {

                return false;
            }          
        }
    }
}
