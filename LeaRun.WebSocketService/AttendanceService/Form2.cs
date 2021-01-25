using Apache.NMS;
using Apache.NMS.ActiveMQ;
using Hxh.Tools;
using Hxh.Tools.DaHuaHelp;
using LeaRun.Business.FaceRecognition;
using LeaRun.Entity.Model_Entity.FaceRecognition;
using LeaRun.WebSocketService.AttendanceService;
using Newtonsoft.Json;
using SuperWebSocket;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ApiLoghelper = LeaRun.WebSocketService.ApiLoghelper;

namespace LeaRun.AttendanceService
{
    public partial class Form2 : Form
    {
        private string DESTINATION = "mq.alarm.msg.topic";
        private bool IsServerRuning = false;
        private IConnection connection = null;
        private ISession session = null;
        private IMessageConsumer consumer = null;
        public Form2()
        {
            InitializeComponent();
        }
       

        private string GetTime(int time1, int time2)
        {
            if (time1 > time2)
            {
                return "下午";
            }
            return "上午";
        }


        private int GetStatus(int time, Timerulesjson2 timerulesjson)
        {
            //0正常 1迟到 2早退 3未打卡
            DateTime dateTime = TimeStampHelper.GetDateTime(time);
            var aTime = TimeStampHelper.GetTimeStamp(dateTime.ToString("yyyy-MM-dd " + timerulesjson.a));
            var bTime = TimeStampHelper.GetTimeStamp(dateTime.ToString("yyyy-MM-dd " + timerulesjson.b));
            //if (!(time >= stime && time <= etime)) throw new ArgumentException("时间有问题");
            switch (GetTime(time, bTime))
            {
                case "上午":
                    if (time <= aTime) return 0;//考勤正常
                    else return 1;//考勤迟到
                case "下午":
                    if (time <= bTime) return 0;//考勤正常
                    else return 2;//考勤早退
                default:
                    throw new ArgumentException("不支持此格式的");
            }
        }

        /// <summary>
        /// 获取当前对应的规则索引
        /// </summary>
        /// <param name="dayOfWeek"></param>
        /// <returns></returns>
        private int GetCurrentIndex(DayOfWeek dayOfWeek)
        {
            switch (dayOfWeek)
            {
                case DayOfWeek.Sunday:
                    return 6;
                case DayOfWeek.Monday:
                    return 0;
                case DayOfWeek.Tuesday:
                    return 1;
                case DayOfWeek.Wednesday:
                    return 2;
                case DayOfWeek.Thursday:
                    return 3;
                case DayOfWeek.Friday:
                    return 4;
                case DayOfWeek.Saturday:
                    return 5;
                default:
                    throw new ArgumentException("DayOfWeek有问题");
            }
        }


        #region  webSocket服务
        private void SendToAll(string msg)
        {
            byte[] value = Encoding.UTF8.GetBytes(msg);
            //广播
            foreach (var sendSession in AttendanceWebSocket._attendanceWSDic)
            {
                sendSession.Send(value, 0, value.Length);//将流发送回去
            }
        }
        #endregion

        private void button2_Click(object sender, EventArgs e)
        {
            DESTINATION = this.textBox3.Text;
            if (IsServerRuning)
            {
                if (consumer != null)
                {
                    consumer.Close();
                    consumer = null;
                }
                if (session != null)
                {
                    session.Close();
                    session = null;
                }
                if (connection != null)
                {
                    connection.Stop();//停止监听
                    connection.Close();//关闭连接

                    connection = null;
                }

                IsServerRuning = false;
                this.button2.Text = "Start Listener";
            }
            else
            {
                try
                {
                    IConnectionFactory factory = new ConnectionFactory(this.textBox1.Text)
                    {
                        UserName = "system",
                        Password = "manager"
                    };
                    connection = factory.CreateConnection();

                    connection.ClientId = DESTINATION + "xzlxzl1";
                    connection.Start();//开启监听

                    session = connection.CreateSession();
                    consumer = session.CreateDurableConsumer(new Apache.NMS.ActiveMQ.Commands.ActiveMQTopic(DESTINATION), "xzl", null, false);
                    consumer.Listener += OnMessageReceived;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                IsServerRuning = true;
                this.button2.Text = "Stop Listener";
            }
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }




        private void OnMessageReceived(IMessage message)
        {
            try
            {
                if (message is ITextMessage)
                {
                    this.BeginInvoke(new Action<string>((msg) => { this.textBox2.AppendText(DateTime.Now + "--" + msg + "\r\n"); }), ((ITextMessage)message).Text);
                    //考勤数据编号
                    string resultJsonData = ((ITextMessage)message).Text;
                    ApiLoghelper.Info("订阅消息", resultJsonData);

                    ////获取考勤记录
                    var resultAttendance = AttendanceRulesBll.GetAttendanceInfo(Convert.ToInt32(resultJsonData));

                    if (resultAttendance != null && resultAttendance.userId > 0)//存在记录
                    {
                        //打卡时间
                        var checkInTime = resultAttendance.AlarmDate;
                        //考勤状态
                        int status = 0;
                        //将时间戳转为对应的星期几
                        var weekDay = TimeStampHelper.GetDateTime(checkInTime).DayOfWeek;
                        Timerulesjson timeJson = null;
                        //将时间转为json格式
                        var attendanceTimeJson = JsonConvert.DeserializeObject<Timerulesjson[]>(resultAttendance.TimeRulesJSON).Where(i => i.a != "00:00:00").ToArray();

                        if (resultAttendance.RulesType == 1)//固定时间
                        {
                            timeJson = attendanceTimeJson[0];

                        }
                        else
                        {
                            //获取当前规则对应的索引
                            var currentIndex = GetCurrentIndex(weekDay);
                            if (attendanceTimeJson.Length >= currentIndex)
                            {
                                //获取考勤对应的某一天的规则
                                timeJson = attendanceTimeJson[currentIndex];
                            }
                        }
                        
                        Test3.HandlerTime(resultAttendance.userId, resultAttendance.GardenId, checkInTime, timeJson);
                       
                        if (AttendanceWebSocket._attendanceWSDic != null && AttendanceWebSocket._attendanceWSDic.Count>0)
                        {
                            SendToAll("ok");
                        }

                    }

                }

            }
            catch (Exception ex)
            {

                ApiLoghelper.Error("消息处理异常",ex.ToString());
            }

        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.textBox2.Clear();
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void groupBox2_Enter(object sender, EventArgs e)
        {

        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            this.textBox2.Clear();
        }

        private void Form2_Load_1(object sender, EventArgs e)
        {
        }
        /// <summary>
        /// 定时器
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timer1_Tick(object sender, EventArgs e)
        {
            //处理某一天考勤规则不需要打卡的情况 
            //1.读取考勤规则和用户编号对应的信息 根据考勤规则编号分组
            //2.根据考勤规则编号查询当前时间是否要插入数据 
            try
            {
                //当前时间
                var dateNow = DateTime.Now;

                var dateTime = dateNow.Hour;
                //判断是否对上了
                if (dateTime == 3)
                {
                    List<int> UserIds = new List<int>();//存放要插入数据的用户


                    var resultDetails = AttendanceDetailsBll.GetAttendanceDetailsList(0);//查询所有的记录

                    var resultAttendanceIds = resultDetails.GroupBy(r => r.AttendanceId);//根据考勤规则编号分组

                    foreach (var item in resultAttendanceIds)
                    {
                        if (item.Key > 0)
                        {
                            var resultRecored = AttendanceRulesBll.GetAttendanceRulesGetById(item.Key);

                            if (resultRecored != null && resultRecored.id > 0)
                            {
                                //将时间戳转为对应的星期几
                                var weekDay = dateNow.DayOfWeek;
                                Timerulesjson timeJson = null;
                                //将时间转为json格式
                                var attendanceTimeJson = resultRecored.TimeRulesJSON;

                                if (resultRecored.RulesType == 1)//固定时间
                                {
                                    timeJson = attendanceTimeJson[0];
                                    var currentIndex = GetCurrentIndex(weekDay);
                                    if (currentIndex==6 || currentIndex==5)
                                    {
                                        continue;
                                    }
                                   
                                }
                                else
                                {
                                    //获取当前规则对应的索引
                                    var currentIndex = GetCurrentIndex(weekDay);
                                    if (attendanceTimeJson.Length >= currentIndex)
                                    {
                                        //获取考勤对应的某一天的规则
                                        timeJson = attendanceTimeJson[currentIndex];
                                    }
                                }

                                if (timeJson.a != "00:00:00")//是否存在当天的考勤规则要插入数据
                                {
                                    UserIds.AddRange(resultDetails.Where(r => r.AttendanceId == item.Key).Select(r => r.UserId));
                                }
                            }
                        }
                    }
                    if (UserIds.Count > 0)
                    {
                        //插入数据
                        AttendanceRecordsBll.InstallFaceRecognitiondata(TimeStampHelper.GetTimeStamp(dateNow.ToString("yyyy-MM-dd 03:00:00")), string.Join(",", UserIds));
                    }
                }
            }
            catch (Exception ex)
            {
                ApiLoghelper.Error(DateTime.Now + "_定时插入数据库失败", ex.ToString());
            }
        }
    }
}
