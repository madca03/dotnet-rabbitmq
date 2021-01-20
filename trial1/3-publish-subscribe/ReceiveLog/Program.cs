using System;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace ReceiveLog
{
    class Program
    {
        static void Main(string[] args)
        {
            var factory = new ConnectionFactory() {HostName = "localhost"};
            using var connection = factory.CreateConnection("3-publish-subscribe-receiver");
            using (var channel = connection.CreateModel())
            {
                var exchangeName = "logs";
                channel.ExchangeDeclare(exchangeName, ExchangeType.Fanout);

                channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

                var queue = channel.QueueDeclare();
                var queueName = queue.QueueName;

                channel.QueueBind(queue: queueName,
                    exchange: exchangeName,
                    routingKey: "");

                Console.WriteLine(" [*] Waiting for logs");

                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += OnReceive;
                channel.BasicConsume(queueName, false, consumer);

                Console.WriteLine(" Press [enter] to exit");
                Console.ReadLine();
            }
        }

        private static void OnReceive(object model, BasicDeliverEventArgs ea)
        {
            var channel = ((EventingBasicConsumer) model).Model;
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            Console.WriteLine(" [x] {0}", message);
            channel.BasicAck(ea.DeliveryTag, false);
        }
    }
}
