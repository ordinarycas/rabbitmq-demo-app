using RabbitMQ.Client;
using System;
using System.Text;

namespace TaskApp.Services
{
    public class RabbitMQService
    {
        private readonly ConnectionFactory _factory;

        // 建構子，初始化連接工廠
        public RabbitMQService()
        {
            _factory = new ConnectionFactory() { HostName = "localhost" };  // 設定連接到 RabbitMQ 的主機
        }

        // 發送訊息的方法
        public void SendMessage(int loopCount, string routingKey)
        {
            // 建立與 RabbitMQ 的連接
            using var connection = _factory.CreateConnection();
            using var channel = connection.CreateModel();

            // 宣告隊列
            channel.QueueDeclare(queue: routingKey,
                                 durable: true,     // 設定隊列為持久化
                                 exclusive: false,  // 不設為獨占隊列
                                 autoDelete: false, // 不自動刪除
                                 arguments: null);  // 無附加參數

            // 發送多條訊息
            for (int i = 0; i < loopCount; i++)
            {
                var message = $"Message {i}";  // 訊息內容
                var body = Encoding.UTF8.GetBytes(message);  // 編碼訊息

                var properties = channel.CreateBasicProperties();
                properties.Persistent = true;  // 設定訊息為持久化

                // 發送訊息到 RabbitMQ
                channel.BasicPublish(exchange: string.Empty,
                                     routingKey: routingKey,
                                     basicProperties: properties,
                                     body: body);
                Console.WriteLine($" [x] Sent {message}");  // 輸出發送的訊息
            }
        }
    }
}
