using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hxh.Tools.RabiitMq
{
    public class RabbitMqService
    {

        //RabbitMQ建议客户端线程之间不要共用Model，至少要保证共用Model的线程发送消息必须是串行的，但是建议尽量共用Connection。
        private static readonly ConcurrentDictionary<string, IModel> ModelDic = new ConcurrentDictionary<string, IModel>();


        //连接对象
        private static IConnection _conn;

        private static readonly object LockObj = new object();


        /// <summary>
        /// 建立mq连接
        /// </summary>
        /// <param name="config"></param>
        private static void Open(MqConfig config)
        {
            if (_conn != null) return;

            lock (LockObj)
            {
                //建立RabbitMq连接
                var factory = new ConnectionFactory()
                {
                    //是否自动重连
                    AutomaticRecoveryEnabled = config.AutomaticRecoveryEnabled,
                    //重新连接间隔
                    NetworkRecoveryInterval = config.NetworkRecoveryInterval,
                    //主机名
                    HostName = config.Host,
                    //用户名
                    UserName = config.UserName,
                    //密码
                    Password = config.Password,
                };
                _conn = _conn ?? factory.CreateConnection();
            }
        }

        public RabbitMqService(MqConfig config)
        {
            Open(config);

        }

        #region 交换器声明
        /// <summary>
        /// 交换器声明
        /// </summary>
        /// <param name="iModel"></param>
        /// <param name="exchange">交换器</param>
        /// <param name="type">交换器类型：
        /// 1、Direct Exchange – 处理路由键。需要将一个队列绑定到交换机上，要求该消息与一个特定的路由键完全
        /// 匹配。这是一个完整的匹配。如果一个队列绑定到该交换机上要求路由键 “dog”，则只有被标记为“dog”的
        /// 消息才被转发，不会转发dog.puppy，也不会转发dog.guard，只会转发dog
        /// 2、Fanout Exchange – 不处理路由键。你只需要简单的将队列绑定到交换机上。一个发送到交换机的消息都
        /// 会被转发到与该交换机绑定的所有队列上。很像子网广播，每台子网内的主机都获得了一份复制的消息。Fanout
        /// 交换机转发消息是最快的。
        /// 3、Topic Exchange – 将路由键和某模式进行匹配。此时队列需要绑定要一个模式上。符号“#”匹配一个或多
        /// 个词，符号“*”匹配不多不少一个词。因此“audit.#”能够匹配到“audit.irs.corporate”，但是“audit.*”
        /// 只会匹配到“audit.irs”。</param>
        /// <param name="durable">持久化</param>
        /// <param name="autoDelete">自动删除</param>
        /// <param name="arguments">参数</param>
        private static void ExchangeDeclare(IModel iModel, string exchange,
            string type = ExchangeType.Direct,
            bool durable = true,
            bool autoDelete = false, IDictionary<string, object> arguments = null)
        {
            exchange = string.IsNullOrWhiteSpace(exchange) ? "" : exchange.Trim();
            iModel.ExchangeDeclare(exchange, type, durable, autoDelete, arguments);
        }
        #endregion


        #region 声明队列

        /// <summary>
        /// 队列声明
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="queue">队列</param>
        /// <param name="durable">持久化</param>
        /// <param name="exclusive">排他队列，如果一个队列被声明为排他队列，该队列仅对首次声明它的连接可见，
        /// 并在连接断开时自动删除。这里需要注意三点：其一，排他队列是基于连接可见的，同一连接的不同信道是可
        /// 以同时访问同一个连接创建的排他队列的。其二，“首次”，如果一个连接已经声明了一个排他队列，其他连
        /// 接是不允许建立同名的排他队列的，这个与普通队列不同。其三，即使该队列是持久化的，一旦连接关闭或者
        /// 客户端退出，该排他队列都会被自动删除的。这种队列适用于只限于一个客户端发送读取消息的应用场景。</param>
        /// <param name="autoDelete">自动删除</param>
        /// <param name="arguments">参数</param>
        public static void QueueDeclare(IModel channel, string queue, bool durable = true, bool exclusive = false,
            bool autoDelete = false, IDictionary<string, object> arguments = null)
        {
            queue = string.IsNullOrWhiteSpace(queue) ? "UndefinedQueueName" : queue.Trim();
            channel.QueueDeclare(queue, durable, exclusive, autoDelete, arguments);
        }

        #endregion


        #region 获取Model

        /// <summary>
        /// 获取Model
        /// </summary>
        /// <param name="exchange">交换机名称</param>
        /// <param name="queue">队列名称</param>
        /// <param name="routingKey"></param>
        /// <param name="isProperties">是否持久化</param>
        /// <returns></returns>
        private static IModel GetModel(string exchange, string queue, string routingKey, bool isProperties = false)
        {          

            return ModelDic.GetOrAdd(queue, key =>
            {
                var model = _conn.CreateModel();
                ExchangeDeclare(model, exchange, ExchangeType.Fanout, isProperties);
                QueueDeclare(model, queue, isProperties);
                model.QueueBind(queue, exchange, routingKey);
                ModelDic[queue] = model;
                return model;
            });
        }

        /// <summary>
        /// 获取Model
        /// </summary>
        /// <param name="queue">队列名称</param>
        /// <param name="isProperties"></param>
        /// <returns></returns>
        public  IModel GetModel(string exchange, string queue, bool isProperties = false)
        {
            return ModelDic.GetOrAdd(queue, value =>
            {
                var model = _conn.CreateModel();
                ExchangeDeclare(model, exchange, ExchangeType.Fanout, isProperties);
                QueueDeclare(model, queue, isProperties);

                model.QueueBind(queue, exchange, queue);

                //每次消费的消息数
                model.BasicQos(0, 1, false);

                ModelDic[queue] = model;

                return model;
            });
        }

        #endregion

        #region 发布消息

        /// <summary>
        /// 发布消息
        /// </summary>
        /// <param name="exchange">交换机名称</param>
        /// <param name="queue">队列名</param>
        /// <param name="routingKey">路由键</param>
        /// <param name="body">队列信息</param>
        /// <param name="isProperties">是否持久化</param>
        public void Publish(string exchange, string queue, string routingKey, string body, bool isProperties = false)
        {
            // 获取Model
            var channel = GetModel(exchange, queue, routingKey, isProperties);

            try
            {
                var properties = channel.CreateBasicProperties();
                properties.Persistent = true;
                channel.BasicPublish(exchange, routingKey, properties, Encoding.UTF8.GetBytes(body));
            }
            catch (Exception ex)
            {
                throw ex.InnerException;
            }
        }

        /// <summary>
        /// 发布死信消息
        /// </summary>
        /// <param name="exchange"></param>
        /// <param name="queue"></param>
        /// <param name="routingKey"></param>
        /// <param name="body"></param>
        /// <param name="isProperties"></param>
        public void PublishToDead(string exchange, string queue, string routingKey, string body, bool isProperties = false)
        {
            Publish(exchange, queue, routingKey, body, isProperties);
        }
        #endregion

        #region 订阅消息

        /// <summary>
        /// 接收消息
        /// </summary>
        /// <param name="queue">队列名称</param>
        /// <param name="isProperties">是否持久化</param>
        /// <param name="isDeadLetter">是否为死信队列</param>
        public void Subscribe(string exchange, string queue, bool isProperties, bool isDeadLetter)
        {
            //队列声明
            var channel = GetModel(exchange,queue, isProperties);

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (sender, ea) =>
            {

                var body = ea.Body;
                var msgStr = Encoding.UTF8.GetString(ea.Body);
                Console.WriteLine(msgStr);

                Console.ReadLine();
            };
            channel.BasicConsume(queue, true, consumer);
        }
        #endregion
    }
}
