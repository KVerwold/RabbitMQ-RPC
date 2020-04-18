using System;
using System.Collections.Concurrent;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RpcStub;

namespace RpcClient {

	public class RabbitRpcClient : IRpcClient {
		private readonly IConnection connection;
		private readonly IModel channel;
		private readonly string replyQueueName;
		private readonly EventingBasicConsumer consumer;
		private readonly BlockingCollection<string> respQueue = new BlockingCollection<string> ();
		private readonly IBasicProperties props;

		public RabbitRpcClient () {
			var factory = new ConnectionFactory () { HostName = "localhost" };

			connection = factory.CreateConnection ();
			channel = connection.CreateModel ();
			channel.ExchangeDeclare (exchange: "rpc_queue", type : ExchangeType.Topic);

			replyQueueName = channel.QueueDeclare ().QueueName;
			consumer = new EventingBasicConsumer (channel);

			props = channel.CreateBasicProperties ();
			var correlationId = Guid.NewGuid ().ToString ();
			props.CorrelationId = correlationId;
			props.ReplyTo = replyQueueName;

			consumer.Received += (model, ea) => {
				var body = ea.Body;
				var response = Encoding.UTF8.GetString (body);
				if (ea.BasicProperties.CorrelationId == correlationId) {
					respQueue.Add (response);
				}
			};
		}

		public string Call (string className, string method, string message) {
			var messageBytes = Encoding.UTF8.GetBytes (message);
			channel.BasicPublish (
				exchange: "rpc_queue",
				routingKey: $"{className}.{method}",
				basicProperties : props,
				body : messageBytes);

			channel.BasicConsume (
				consumer: consumer,
				queue: replyQueueName,
				autoAck: true);

			return respQueue.Take ();
		}

		public void Close () {
			connection.Close ();
		}
	}
}