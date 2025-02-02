using System;
using System.Text;
using RabbitMQ.Client;


// Creates an instance of ConnectionFactory, which is used to establish a connection to the RabbitMQ server.
// Sets the HostName property to "localhost", indicating that the RabbitMQ server is running on the same machine.
ConnectionFactory factory = new ConnectionFactory() { HostName = "localhost" };
using IConnection connection = factory.CreateConnection();
// Creates a communication channel within the established connection.
using IModel channel = connection.CreateModel();

// Declares a queue named "letterbox" on the RabbitMQ server.
channel.QueueDeclare(
    queue: "letterbox",
    durable: false,
    exclusive: false,
    autoDelete: false,
    arguments: null);

for (int i = 0; i < 1000000; i++)
{
    String message = "This is my first Message: " + i;
    byte[] body = Encoding.UTF8.GetBytes(message);
    channel.BasicPublish(
        exchange: string.Empty,
        routingKey: "letterbox",
        basicProperties: null,
        body: body);
    Console.WriteLine($"Published message: {message}");
}
Console.WriteLine(" Press [enter] to exit.");
Console.ReadLine();