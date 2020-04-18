using System;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RpcStub;
using Shared;

namespace RpcServer {
	/// <summary>
	/// This sample is based on RabbitMQ turotrials
	/// Source: https://www.rabbitmq.com/tutorials/tutorial-six-dotnet.html
	/// </summary>
	class Program {

		static void Main (string[] args) {

			// RPC Controller, which invokes the methods from the proxied class (MyClass) 
			// through the interface (IMyClass), which also is proxied from the RPC client
			var controller = new RpcController<IMyClass, MyClass> (ProxyLifetime.ControllerLifetime);

			var factory = new ConnectionFactory () { HostName = "localhost" };
			using (var connection = factory.CreateConnection ())
			using (var channel = connection.CreateModel ()) {
				channel.ExchangeDeclare (exchange: "rpc_queue", type : ExchangeType.Topic);
				var queueName = channel.QueueDeclare ().QueueName;

				channel.QueueBind (queue: queueName,
					exchange: "rpc_queue",
					routingKey: $"{nameof(IMyClass)}.*");

				Console.WriteLine (" [*] Waiting for messages.");

				var consumer = new EventingBasicConsumer (channel);
				consumer.Received += (model, ea) => {
					var body = ea.Body;
					var props = ea.BasicProperties;

					var replyProps = channel.CreateBasicProperties ();
					replyProps.CorrelationId = props.CorrelationId;

					var message = Encoding.UTF8.GetString (body);
					Console.WriteLine ($" [x] Received '{ea.RoutingKey}:'{message}'");

					// Invoked proxy method from RPC call
					var res = controller.Invoke (ea.RoutingKey, message);
					var response = res != null ? Encoding.UTF8.GetBytes (res) : null;

					channel.BasicPublish (exchange: "", routingKey : props.ReplyTo,
						basicProperties : replyProps, body : response);

					channel.BasicAck (deliveryTag: ea.DeliveryTag, multiple: false);
				};

				channel.BasicConsume (queue: queueName,
					autoAck: false,
					consumer: consumer);

				Console.WriteLine (" Press [enter] to exit.");
				Console.ReadLine ();

			}
		}

	}
}