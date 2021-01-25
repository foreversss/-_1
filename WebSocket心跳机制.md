# WebSocket心跳机制

//***********WebSocket函数*****************//



//创建Websocket连接

function createWebSocket(url) {

 try {

  //判断浏览器是否支持websocket

  if ("WebSocket" in window) {

   ws = new WebSocket(url);

  }



  initEventHandle();

 } catch (e) {

  //重新连接

  reconnect(url);

 }

}



//webSocket事件

function initEventHandle() {

  //关闭事件

  ws.onclose = function (e) {

​    reconnect(wsUrl);

​    console.log("连接关闭!" + new Date().toLocaleString() + "原因:" + e.code);

  };



  //打开连接事件

  ws.onopen = function () {

​    //给后台推消息 有人进来了。

​    ws.send("ComeIn");

​    console.log("连接成功!" + new Date().toLocaleString());

  };

  //收到消息事件

  ws.onmessage = function (event) {

​    //心跳检测重置

​    heartCheck.reset().start();



​    //拿到任何消息都说明当前连接是正常的

​    if (event.data != "pong") {

​      var json = event.data;

​      //结束会议

​      if (json == "EenMeeting") {

​        if (PrrsonnelVue.Room.UserId == UserId) {

​          layer.msg("成功结束", { time: 500 });

​          setTimeout(function () {

​            window.location.replace("MeetingRoom.html");

​          }, 500);

​          return;

​        }

​        layer.alert(

​         "会议已结束",

​         {

​           icon: 5,

​           title: "提示",

​         },

​         function () {

​           window.location.replace("MeetingRoom.html");

​         }

​        );

​        return;

​      }

​      //结束共享

​      if (json == "End") {

​        

​        PrrsonnelVue.PreviewPath = "";

​        Load();

​        return;

​      }

​      json = JSON.parse(event.data);

​      //如果是数组那么就是刷新人员列表

​      if (json instanceof Array) {

​        

​        if (json.length == 0) {



​          PrrsonnelVue.MeetingFile = json;

​          return;

​        }

​        if (json[0].FileName !== undefined) {

​          //会议共享文件列表

​          PrrsonnelVue.MeetingFile = json;



​          //循环文件 查看有没有文件正在预览

​          $.each(PrrsonnelVue.MeetingFile, function () {

​            if (this.Isprevie) {

​              PrrsonnelVue.PreviewPath = this.PreviewPath;

​              return;

​            }

​          });



​        } else {

​          //会议人员列表

​          PrrsonnelVue.Prrsonnellist = json;

​          PrrsonnelVue.Room.CurrentNumber = PrrsonnelVue.Prrsonnellist.length;

​          //循环文件 查找有没有文件正在预览

​          $.each(PrrsonnelVue.MeetingFile, function () {

​            if (this.Isprevie) {

​              PrrsonnelVue.PreviewPath = this.PreviewPath;

​              return;

​            }

​          });

​        }

​        return;

​      }     

​      //聊天消息

​      PrrsonnelVue.MessagereCord.push(json);     

​     

​      //提醒有未读消息

​      var title = $(".define-toggle-title>.active").text();

​      if (title == "文件") {

​        $(".define-toggle-title>div:eq(0)").append("<i></i>");

​      }

​      if (title == "聊天" && json.MessageType == 3) {

​        $(".define-toggle-title>div:eq(1)").append("<i></i>");

​      }

​    }

   

  };

}



//重新连接

function reconnect(url) {

 //如果lockReconnect等于true时 说明ws是打开连接的状态

 if (lockReconnect) {

  return;

 }

 lockReconnect = true;



 //没连接上会一直重连，设置延迟避免请求过多

 setTimeout(function () {

  createWebSocket(url);

  lockReconnect = false;

 }, 2000);

}





//心跳检测

var heartCheck = {

  timeout: 120000, //2分钟发一次心跳

  timeoutObj: null,

  serverTimeoutObj: null,

  reset: function () {

​    clearTimeout(this.timeoutObj);

​    clearTimeout(this.serverTimeoutObj);

​    return this;

  },

  start: function () {

​    var self = this;

​    this.timeoutObj = setTimeout(function () {

​      //这里发送一个心跳，后端收到后，返回一个心跳消息，

​      //onmessage拿到返回的心跳就说明连接正常 

​      ws.send("ping");

​      console.log(new Date().toLocaleString());

​      //如果超过一定时间还没重置，说明后端主动断开了

​      self.serverTimeoutObj = setTimeout(function () {       

​        //关闭连接进行重新连接

​        ws.close();

​      }, self.timeout);

​    }, this.timeout);

  }

}