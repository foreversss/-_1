using Hxh.Tools;
using LeaRun.Business.Meeting;
using LeaRun.Entity.DB_Entity.Meeting;
using Newtonsoft.Json;
using SuperWebSocket;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeaRun.WebSocketService.Meeting
{
    /// <summary>
    /// 会议室外面
    /// </summary>
    public class Meetingprrsonnel
    {
           //ConcurrentDictionary是线程安全的    
        private static ConcurrentDictionary<int, WebSocketSession> _userWSDic;

        static Meetingprrsonnel()
        {
            _userWSDic = new ConcurrentDictionary<int, WebSocketSession>();
        }


        /// <summary>
        /// 用户第一次进来(添加用户)
        /// </summary>
        /// <param name="UserId">用户</param>
        /// <param name="ws"></param>
        public static void AddUserWs(int UserId, WebSocketSession ws)
        {
            _userWSDic[UserId] = ws;
        }

        /// <summary>
        /// 用户退出(删除用户)
        /// </summary>
        /// <param name="UserId">用户Id</param>
        public static void RemoveUserWS(int userId, int RoomId)
        {          
            //执行删除会议人员
            MeetingPrrsonnelBll.DeletePrrsonnel(userId, RoomId);
            //会议室人员列表
            var PrrsonnelList = MeetingPrrsonnelBll.GetAllPrrsonnel(RoomId);

            //给前台推会议人员列表过去
            Broadcast(JsonConvert.SerializeObject(PrrsonnelList));

            int PageCount = 0;
            //查询会议室集合
            var RoomList = MeetingRoomBll.GetAllMeetingRoom(1, 5, ref PageCount, null);

            var RoomJson = JsonConvert.SerializeObject(RoomList);
         
            WebSocketSession socket = null;
            _userWSDic.TryRemove(userId, out socket);


            //刷新会议室外面当前人数
            MeetingRoom.Broadcast(RoomJson);
        }


        /// <summary>
        /// 广播所有消息
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static void Broadcast(string values)
        {
            //给客户端发送消息(广播)
            foreach (var item in _userWSDic.Values)
            {
                item.Send(values);
            }
        }



        /// <summary>
        /// 会议室里面接收消息
        /// </summary>
        /// <param name="values"></param>
        public static void RoomInside(WebSocketSession session, string values, int RoomId)
        {
            //接收消息
            switch (values)
            {
                //接收心跳
                case "ping":
                    //返回消息给客户端  告诉客户端心跳还是活的
                    session.Send("pong");
                    break;

                //用户进来了
                case "ComeIn":
                    //会议室人员列表
                    var PrrsonnelList = MeetingPrrsonnelBll.GetAllPrrsonnel(RoomId);
                    Broadcast(JsonConvert.SerializeObject(PrrsonnelList));
                    break;

                case "File":
                    //把会议文件列表推到前台去刷新
                    var filelist = MeetingFileBll.GetAllFile(RoomId);
                    Broadcast(JsonConvert.SerializeObject(filelist));
                    break;

                //会议已结束 通知在会议室里的人 强行退出来
                case "EenMeeting":
                    //给客户端发送消息(广播)                   
                    Broadcast("EenMeeting");
                    break;

                //接受共享
                case "End":
                    //给客户端发送消息(广播)
                    Broadcast("End");
                    break;

                //发送聊天消息
                default:
                    var messagerecord = JsonHelper.JSONToObject<tb_meetingmessagerecord>(values);
                    //添加消息记录
                    MeetingMessageBll.InsertMessagerecord(messagerecord);
                    Broadcast(JsonConvert.SerializeObject(messagerecord));
                    break;
            }
        }
    }
}
