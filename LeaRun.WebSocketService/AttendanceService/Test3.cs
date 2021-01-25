using Hxh.Tools;
using LeaRun.Business.FaceRecognition;
using LeaRun.Entity.Model_Entity.FaceRecognition;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeaRun.AttendanceService
{
    public class Test3
    {
        /// <summary>
        /// 时间处理逻辑
        /// </summary>
        /// <param name="checkInTime"></param>
        /// <param name="timerulesjson"></param>
        public static void HandlerTime(int userId, int gardenId, int checkInTime, Timerulesjson timerulesjson)
        {
            //查询这个用户的当天的数据
            var resultAttendanceRecords = AttendanceRecordsBll.GetAttendanceRecordsByUserId(userId, checkInTime);
            if (resultAttendanceRecords==null || resultAttendanceRecords.Count<=0)
            {
                throw new Exception("错误请检查系统有没有插入数据");
            }

            //0正常 1迟到 2早退 3未打卡
            DateTime dateTime = TimeStampHelper.GetDateTime(checkInTime);
            var aTime = TimeStampHelper.GetTimeStamp(dateTime.ToString("yyyy-MM-dd " + timerulesjson.a));
            var bTime = TimeStampHelper.GetTimeStamp(dateTime.ToString("yyyy-MM-dd " + timerulesjson.b));
            var cTime = TimeStampHelper.GetTimeStamp(dateTime.ToString("yyyy-MM-dd " + timerulesjson.c));
            var dTime = TimeStampHelper.GetTimeStamp(dateTime.ToString("yyyy-MM-dd " + timerulesjson.d));
            //if (!(checkInTime >= stime && checkInTime <= etime)) throw new ArgumentException("时间有问题");

            if (checkInTime <= aTime)//如果有数据添加数据 考勤状态为正常
            {
                var result = resultAttendanceRecords.Where(e => e.MorningTime <= aTime).FirstOrDefault();
                if (result == null || result.id <= 0)//没有添加
                {
                        var attendanceRecordId = AttendanceRecordsBll.UpdateAttendanceRecords(new Entity.DB_Entity.FaceRecognition.tb_attendancerecords
                        {
                            id= resultAttendanceRecords.FirstOrDefault().id,
                            MorningStatus = 0,
                            MorningTime = checkInTime,
                            GardenId = gardenId,
                            Remark = "上午上班时间",
                            UserId = userId
                        });;
                }
                else
                {
                   
                }
            }
            else if (checkInTime > aTime && checkInTime <= bTime)//
            {
                var result= resultAttendanceRecords.Where(e =>e.MorningTime <= aTime ).FirstOrDefault();
                if (result == null || result.id <= 0)//没有添加
                {
                    result = resultAttendanceRecords.Where(e => e.MorningTime > aTime && e.MorningTime <= bTime).FirstOrDefault();
                    if (result == null || result.id <= 0)//没有添加 上午未打开
                    {
                        var attendanceRecordId = AttendanceRecordsBll.UpdateAttendanceRecords(new Entity.DB_Entity.FaceRecognition.tb_attendancerecords
                        {
                            id= resultAttendanceRecords.FirstOrDefault().id,
                            MorningStatus = 1,
                            MorningTime = checkInTime,
                            GardenId = gardenId,
                            Remark = "上午上班时间",
                            UserId = userId
                        });
                    }
                }
                else
                {
                   
                }
            }
            else if (checkInTime>bTime && checkInTime<dTime)
            {
                var result = resultAttendanceRecords.Where(e => e.AfternoonTime > bTime && e.AfternoonTime < dTime ).FirstOrDefault();
                if (result == null || result.id <= 0)//没有添加
                {
                    var isResult = resultAttendanceRecords.Where(e => e.AfternoonTime > dTime).FirstOrDefault();
                    if (isResult == null || isResult.id <= 0)
                    {
                        var attendanceRecordId = AttendanceRecordsBll.UpdateAttendanceRecords(new Entity.DB_Entity.FaceRecognition.tb_attendancerecords
                        {
                            id= resultAttendanceRecords.FirstOrDefault().id,
                            AfternoonStatus = 2,
                            AfternoonTime = checkInTime,
                            GardenId = gardenId,
                            Remark = "下午下班时间",
                            UserId = userId
                        });
                    }
                    else
                    {
                        var attendanceRecordId = AttendanceRecordsBll.UpdateAttendanceRecords(new Entity.DB_Entity.FaceRecognition.tb_attendancerecords
                        {
                            id= isResult.id,
                            AfternoonStatus = 2,
                            AfternoonTime = checkInTime,
                            GardenId = gardenId,
                            Remark = "下午下班时间",
                            UserId = userId
                        });
                    }
                }
                else
                {
                    AttendanceRecordsBll.UpdateAttendanceRecords(new Entity.DB_Entity.FaceRecognition.tb_attendancerecords
                    {
                        id = result.id,
                        AfternoonStatus = 2,
                        AfternoonTime = checkInTime,
                        GardenId = gardenId,
                        Remark = "下午下班时间",
                        UserId = userId
                    });
                }

            }
            else//如果有数据添加数据 考勤状态为正常 
            {
              
                var resultUpdate = resultAttendanceRecords.Where(e => e.AfternoonTime >= dTime ).FirstOrDefault();
                if (resultUpdate==null|| resultUpdate.id<=0)//没有
                {
                    var result = resultAttendanceRecords.Where(e => e.AfternoonTime >=dTime && e.AfternoonStatus != 0).FirstOrDefault();
                    if (result == null || result.id <= 0)//没有添加
                    {
                        AttendanceRecordsBll.UpdateAttendanceRecords(new Entity.DB_Entity.FaceRecognition.tb_attendancerecords
                        {
                            id = resultAttendanceRecords.FirstOrDefault().id,
                            AfternoonStatus = 0,
                            AfternoonTime = checkInTime,
                            GardenId = gardenId,
                            Remark = "下午下班时间",
                            UserId = userId
                        });
                    }
                    else
                    {
                        
                    }
                }
                else
                {
                    
                }
            }
        }
    }
}
