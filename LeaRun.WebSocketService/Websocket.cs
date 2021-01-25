
using Hxh.Tools;
using LeaRun.WebSocketService.AttendanceService;
using LeaRun.WebSocketService.Meeting;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Config;
using SuperWebSocket;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LeaRun.WebSocketService
{
    public partial class Websocket : Form
    {
        //socke服务
        private WebSocketServer ws = null;

        public Websocket()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //打开连接
            WebSocketConnect();
        }


        //连接websocket
        public void WebSocketConnect()
        {
            try
            {
                ws = new WebSocketServer();
                var serverConfig = new ServerConfig
                {
                    //Name = serverName,
                    //MaxConnectionNumber = 10000, //最大允许的客户端连接数目，默认为100。
                    Mode = SocketMode.Tcp,
                    Port = Convert.ToInt32(this.txtprot.Text), //服务器监听的端口。
                    ClearIdleSession = false,   //true或者false， 是否清除空闲会话，默认为false。
                    // ClearIdleSessionInterval = 120,//清除空闲会话的时间间隔，默认为120，单位为秒。
                    //ListenBacklog = 10,
                    ReceiveBufferSize = 64 * 1024, //用于接收数据的缓冲区大小，默认为2048。
                    SendBufferSize = 64 * 1024,   //用户发送数据的缓冲区大小，默认为2048。
                    KeepAliveInterval = 1,     //keep alive消息发送时间间隔。单位为秒。
                    // KeepAliveTime = 60,    //keep alive失败重试的时间间隔。单位为秒。
                    SyncSend = false
                };

                //开启服务器
                if (!ws.Setup(new RootConfig(), serverConfig))
                {
                    ApiLoghelper.Error("开启服务器失败: ", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                }
                if (!ws.Start())
                {
                    ApiLoghelper.Error("ip地址无效: ", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                }

                ApiLoghelper.Info("成功开启websocket服务: ", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

                this.txtInfo.Text = "成功开启websocket服务:" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                //注册事件
                ws.NewSessionConnected += WsNewSessionConnected;
                ws.NewMessageReceived += WsNewMessageReceived;
                ws.SessionClosed += WsSessionClosed;
            }
            catch (Exception ex)
            {
                ApiLoghelper.Error("开启连接错误:", ex.Message);
            }
        }


        //连接事件
        public void WsNewSessionConnected(WebSocketSession session)
        {
            var path = session.Origin + session.Path;
            Uri uri = new Uri(path);
            string queryString = uri.Query;
            //获取url参数值
            NameValueCollection col = URLQueryString.GetQueryString(queryString);        
            //模块名称
            var modular = col["Modular"];

            switch (modular)
            {
                    //应急指挥
                case "Plan":
                    //页面类型 PageType = “Room”会议室外面   PageType = “RoomInside” 会议室里面
                    var PageType = col["PageType"];
                    //会议室
                    if (PageType == "Plan_Room")
                    {
                        //用户Id
                        var userId = Convert.ToInt32(col["UserId"]);
                        //添加用户
                        MeetingRoom.AddUserWs(userId, session);
                        return;
                    }
                    //会议室里面的内容(开会中)
                    if (PageType == "Plan_RoomInside")
                    {
                        //用户Id
                        var userId = Convert.ToInt32(col["UserId"]);
                        //添加用户 
                        Meetingprrsonnel.AddUserWs(userId, session);                       
                    }        
                    break;


               //人脸考勤
                case "Attendance":
                    AttendanceWebSocket._attendanceWSDic.Add(session);
                    break;

                default:
                    break;
            }
                    
        }
        /// <summary>
        /// 收到消息事件
        /// </summary>
        /// <param name="session"></param>
        /// <param name="values"></param>
        public void WsNewMessageReceived(WebSocketSession session, string values)
        {
            var path = session.Origin + session.Path;
            Uri uri = new Uri(path);
            string queryString = uri.Query;
            //获取url参数值
            NameValueCollection col = URLQueryString.GetQueryString(queryString);

            //模块名称
            var modular = col["Modular"];

            switch (modular)
            {
                case "Plan":
                    //页面类型 PageType = “Room”会议室外面   PageType = “RoomInside” 会议室里面
                    var PageType = col["PageType"];
                    //会议室
                    if (PageType == "Plan_Room")
                    {
                        var pageIndex = Convert.ToInt32(col["page"]);

                        var pageSize = Convert.ToInt32(col["PageSize"]);

                        MeetingRoom.RoomMessage(session, pageIndex, pageSize, values);
                        return;
                    }
                    //会议室里面的内容(开会中)
                    if (PageType == "Plan_RoomInside")
                    {
                        //会议室Id
                        var RoomId = Convert.ToInt32(col["RoomId"]);

                        Meetingprrsonnel.RoomInside(session, values, RoomId);
                        return;
                    }
                    break;



                default:
                    break;
            }
        }

        /// <summary>
        /// 关闭事件
        /// </summary>
        /// <param name="session"></param>
        /// <param name="value"></param>
        public void WsSessionClosed(WebSocketSession session, SuperSocket.SocketBase.CloseReason value)
        {
            var path = session.Origin + session.Path;
            Uri uri = new Uri(path);
            string queryString = uri.Query;
            //获取url参数值
            NameValueCollection col = URLQueryString.GetQueryString(queryString);

            //模块名称
            var modular = col["Modular"];

            switch (modular)
            {
                case "Plan":
                    //页面类型 PageType = “Room”会议室外面   PageType = “RoomInside” 会议室里面
                    var PageType = col["PageType"];
                    //会议室
                    if (PageType == "Plan_Room")
                    {
                        //用户Id
                        var userId = Convert.ToInt32(col["UserId"]);

                        MeetingRoom.RemoveUserWS(userId);

                        return;
                    }
                    //会议室里面的内容(开会中)
                    if (PageType == "Plan_RoomInside")
                    {
                        //用户Id
                        var userId = Convert.ToInt32(col["UserId"]);

                        //会议室Id
                        var RoomId = Convert.ToInt32(col["RoomId"]);

                        Meetingprrsonnel.RemoveUserWS(userId, RoomId);
                    }
                    break;

                case "Attendance":
                    AttendanceWebSocket._attendanceWSDic.Remove(session);
                    break;
                default:
                    break;
            }         
        }
    }
}
