using LeaRun.Business.Meeting;
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
    /// 会议室
    /// </summary>
    public class MeetingRoom
    {
          //ConcurrentDictionary是线程安全的    
        private static ConcurrentDictionary<int, WebSocketSession> _userWSDic;

        static MeetingRoom()
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
        public static void RemoveUserWS(int UserId)
        {
            WebSocketSession socket = null;
            _userWSDic.TryRemove(UserId, out socket);
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
        /// 收到消息
        /// </summary>
        public static void RoomMessage(WebSocketSession session, int pageIndex, int pageSize, string values)
        {
            //判断接收的消息是否有值
            if (string.IsNullOrEmpty(values))
            {
                return;
            }

            //接收心跳
            if (values == "ping")
            {
                //返回消息给客户端  告诉客户端心跳还是活的
                session.Send("pong");
                return;
            }

            int pageCount = 0;

            //会议室集合
            var RoomList = MeetingRoomBll.GetAllMeetingRoom(pageIndex, pageSize, ref pageCount, null);

            var RoomJson = JsonConvert.SerializeObject(RoomList);

            //发送消息
            Broadcast(RoomJson);
        }
    }
}
