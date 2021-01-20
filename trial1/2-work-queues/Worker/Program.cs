using System;
using System.Text;
using System.Threading;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Worker
{
    class Program
    {
        static void Main(string[] args)
        {
            var factory = new ConnectionFactory() {HostName = "localhost"};
            using var connection = factory.CreateConnection("2-work-queues-worker");
            using (var channel = connection.CreateModel())
            {
                var queueName = "task_queue";

                channel.QueueDeclare(queue: queueName,
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null);

                channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

                Console.WriteLine("Waiting for messages. Press [enter] to exit");

                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += OnReceive;
                channel.BasicConsume(queueName, false, consumer);

                Console.ReadLine();
            }
        }

        private static void OnReceive(object sender, BasicDeliverEventArgs ea)
        {
            var channel = ((EventingBasicConsumer) sender).Model;
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            Console.WriteLine(" [x] Received {0}", message);

            int dots = message.Split(".").Length - 1;
            Thread.Sleep(dots * 1000);

            Console.WriteLine(" [x] Done");
            channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
        }
    }
}
