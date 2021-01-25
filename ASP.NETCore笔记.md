# **.net core 笔记**

## 	AspNetCoreHostingModel

- ### 1.什么是进程内和进程外?

   		进程内: **InProcess** 指的是我们想要使用进程内托管模型，即在IIS工作进程(w3wp.exe)中托管我们的**ASP.NET Core**应用程序。

  ​		 进程外: **OutOfProcess**指的是我们要使用进程外托管模型，即将web请求转发到后端的ASP.NETCore中，二整个应用程序是运行在ASP.NETCore中内置的Kestrel中。

  ​			注意：ASP.NETCore默认采用的是OutOfProcess托管
  
  ###  **2.ASP.NETCore 中的main方法**
  
  ​	main方法中调用了两个方法 返回了IWebHostBuilder对象
  
  ​			将整个项目编译成编译文件然后放到对应的服务器上面进行托管服务器包括(iis，apache等) 托管之后要接受客户端发过来的请求和响应这个时候我们就要通过run方法启动
  
  ​			 CreateDefaultBuilder：定义了默认使用的Web服务器（UseKestrel，使用的是KestrelServer）和一些基础的配置，包括文件路径、应用配置（按appsettings.json，appsettings.{Environment}.json次序加载）、环境变量、日志，IIS集成等，如果需要的话，还可以指定其他类型的Server（IIS HTTP Server，HTTP.sys Server）和自定义Server（继承IServer）。
  
  
  
  Startup中的两个方法:
  
  ​        ConfigureServices:用于配置一个应用程序所需要的服务和内容比如：第三方插件，组件 都需要他进行调用
  
  ​		Configure:管理运行池里面的接收各种各样的http管道
  
  
  
  
  
  Asp.NET Core 的中间件特点:
  
  ​	1.可同时被访问和请求
  
  ​	2.可以处理请求后，然后将请求传递给下一个中间件
  
  3. 可以处理请求后，并使用管道短路
  4. 可以处理传出响应
  5. 中间件是按照添加顺序执行的 
  
  ​	
  
  Asp.net core 中的环境变量：
  
  ​	开发环境 (Development)
  
  ​	集成环境 (integration)
  
  ​	测试环境(testing)
  
  ​	验证环境(QA)
  
  ​	模拟环境(staging)
  
  ​	生成环境(production)





### 3.什么是依赖注入?

依赖:当一个类需要另一个类协作来完成工作的时候就会产生依赖。
       比如我们在AccountController这个控制器需要完成和用户相关的注册、登录 等事情。
       其中的登录我们由EF结合Idnetity来完成，所以我们封装了一个EFLoginService。
       这里AccountController就有一个ILoginService的依赖。
注入:注入体现的是一个IOC（控制反转的的思想）。
	什么是控制反转？  
	正转：当我们需要用到一个类的时候，我们是直接new出来的。
	反转:  当你需要用到某个类的时候，由你的调用者给你，而不是你直接去找他要,这个过程可以通过构造函数实现。
把依赖的创建丢给其它人，自己只负责使用，其它人丢给你依赖的这个过程理解为注入。





**AddSingleton**：该方法只会创建一个 ‘Singleton’ 服务。首次请求时会创建 ‘Singleton’服务。然后，所有后续的请求中都会使用相同的实例。因此，通常，每个应用程序只创建一次‘Singleton’服务，并且在整个应用程序生命周期中使用该单个实例

**AddScoped：** 此方法创建一个 ‘Scoped’服务。在范围内的每个请求中创建一个新的Scoped服务实例。例如，在web应用程序中，它为每个http请求创建一个实例，但在同一web请求中的其他服务在调用这个请求的时候，都会使用相同的实例，注意，它在一个客户端请求中是相同的，但在多个客户端请求中是不同的。

**AddTransient:**   每次请求都会创建一个新的Transient服务

​	