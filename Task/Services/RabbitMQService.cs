using RabbitMQ.Client;
using System;
using System.Text;

namespace TaskApp.Services
{
    public class RabbitMQService
    {
        private readonly ConnectionFactory _factory;

        // �غc�l�A��l�Ƴs���u�t
        public RabbitMQService()
        {
            _factory = new ConnectionFactory() { HostName = "localhost" };  // �]�w�s���� RabbitMQ ���D��
        }

        // �o�e�T������k
        public void SendMessage(int loopCount, string routingKey)
        {
            // �إ߻P RabbitMQ ���s��
            using var connection = _factory.CreateConnection();
            using var channel = connection.CreateModel();

            // �ŧi���C
            channel.QueueDeclare(queue: routingKey,
                                 durable: true,     // �]�w���C�����[��
                                 exclusive: false,  // ���]���W�e���C
                                 autoDelete: false, // ���۰ʧR��
                                 arguments: null);  // �L���[�Ѽ�

            // �o�e�h���T��
            for (int i = 0; i < loopCount; i++)
            {
                var message = $"Message {i}";  // �T�����e
                var body = Encoding.UTF8.GetBytes(message);  // �s�X�T��

                var properties = channel.CreateBasicProperties();
                properties.Persistent = true;  // �]�w�T�������[��

                // �o�e�T���� RabbitMQ
                channel.BasicPublish(exchange: string.Empty,
                                     routingKey: routingKey,
                                     basicProperties: properties,
                                     body: body);
                Console.WriteLine($" [x] Sent {message}");  // ��X�o�e���T��
            }
        }
    }
}
