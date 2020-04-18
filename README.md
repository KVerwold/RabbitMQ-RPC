# RabbitMQ-RPC

RabbitMQ-RPC is sample to ease RPC calls using RabbitMQ by proxying methods calls from shared interfaces from client to the server.

On client side, the shared interfaces are proxied by using the DynamixProxy class from System.Reflection. 
The client executes a RPC call to the server, which calls the Invoke method of the DynamixProxy implementation class to:
- Create a dynamic object by mapping the method parameters as object properties
- Copy the method parameter values to the object properties
- Serialize the dynamic object as JSON string
- Sends a message to the server queye by using the routing key 'Interfacename.Methodname' as 'IMyClass.MyMethod'

On server side, same shared interface and its implementation class are assigned to a RPC controller, which invokes the methods from the implementation class through a RPC call.

When receiving a RPC call on server side:

- The Controller.Invoke method is called by using routing key and JSON string
- The routing key is validated against the assigned interface of the controller
- A dynamic object is created using target method from RPC call
- JSON string is deserialized to the dynamic object
- Method from interface's implementation class is invoked by using the dynamic object property values as method parameters
- A result object is dynamically created to obtain the method result
- The method result, if any, is copied to the result object
- The result object is serialized and send back to the client

Sample project structure
- RpcClient: A RPC client console application, proxing the interface IMyClass
- RpcServer: A RPC server console application, containing implementation class of the interface IMyClass 
- RpcStub  : RPC library for creating interace proxies (RpcFactory) and dynamic objects (RpcBuilder) for RPC data transfer
- Shared   : Library for shared interface IMyClass

Both RpcClient and RpcServer are inspired  from RabbitMQ tutorial 
[Remote procedure call (RPC)](https://www.rabbitmq.com/tutorials/tutorial-six-dotnet.html)
