using RabbitMQ.Client;
using System.Reflection;
using System.Text;
using System.Threading.Channels;

namespace LineBot.Service
{
    public class RBMQService
    {
        string queueName = "LineBot";
        string exchangeName = "Debug";
        string Key = "Debug";

        ConnectionFactory factory = new ConnectionFactory
        {
            UserName = "guest",
            Password = "guest",
            HostName = "localhost"
        };

        public RBMQService()
        {
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queueName, false, false, false, null);
                channel.ExchangeDeclare(exchangeName, ExchangeType.Direct, false, false, null);
                channel.QueueBind(queueName, exchangeName, Key);
            }
        }

        public void Send_Message(string type, string input)
        {
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                if (input != "" || input != null)
                {
                    var sendBytes = Encoding.UTF8.GetBytes(type + "\n" + input);
                    channel.BasicPublish(exchangeName, Key, null, sendBytes);
                }
            }
        }
    }
}
