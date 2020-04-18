using System;
using RpcStub;
using Shared;

namespace RpcClient {

	/// <summary>
	/// This sample is based on RabbitMQ turotrials
	/// Source: https://www.rabbitmq.com/tutorials/tutorial-six-dotnet.html
	/// </summary>
	class Program {
		static void Main (string[] args) {
			// Create RabbitMQ RPC Client 
			var rpcClient = new RabbitRpcClient ();
			Console.WriteLine (" [x] Sending requests... ");

			// Create proxy for shared IMyClass interface via RpcFactory.CreateProxy
			var myClass = RpcFactory.CreateProxy<IMyClass> (rpcClient);
			Console.WriteLine (" [TestInt] Response: '{0}'", myClass.TestInt (10));
			Console.WriteLine (" [Min] Response: '{0}'", myClass.Min (1, 5));
			Console.WriteLine (" [Max] Response: '{0}'", myClass.Max (1, 5));
			Console.WriteLine (" [TestMessage] Response: '{0}'", myClass.TestMessage ("Hello world"));
			Console.WriteLine (" [StringJoin] Response: '{0}'", myClass.StringJoin (new [] { "Hello", "world" }));
			Console.WriteLine (" [x] Sending requests...done ");
			rpcClient.Close ();
		}
	}
}