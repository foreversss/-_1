using SuperWebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace LeaRun.WebSocketService.AttendanceService
{
    public class AttendanceWebSocket
    {
        public static List<WebSocketSession> _attendanceWSDic;

        static AttendanceWebSocket()
        {
            _attendanceWSDic = new List<WebSocketSession>();
        }
    }
}
