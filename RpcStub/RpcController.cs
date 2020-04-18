using System;
using Newtonsoft.Json;

namespace RpcStub {

	public enum ProxyLifetime {
		PerCall, // Creates a new instance for each call
		ControllerLifetime // Only one instance with same lifetime as controller
	}

	public class RpcController<TInterface, TClass> where TClass : TInterface, new () {
		private RpcBuilder _rpcBuilder;
		private TClass _proxyInstance;
		private ProxyLifetime _proxyLifetime;

		/// <summary>
		/// RpcController constructor
		/// </summary>
		/// <param name="proxyLifetime">Defines the lifetime of the proxied class instance</param>
		public RpcController (ProxyLifetime proxyLifetime = ProxyLifetime.PerCall) {
			_rpcBuilder = new RpcBuilder ();
			_proxyLifetime = proxyLifetime;
		}

		internal TInterface ProxyInstance {
			get {
				return _proxyLifetime == ProxyLifetime.PerCall ?
					Activator.CreateInstance<TClass> () :
					_proxyInstance ?? (_proxyInstance = Activator.CreateInstance<TClass> ());
			}
		}

		/// <summary>
		/// Invoke the method from the proxied interface by 
		/// Create an instance 
		/// deserialize Call Parameret
		/// </summary>
		/// <param name="classAndMethodName">Name and method from proxied interface as eg. 'IMyClass.MyMethod'</param>
		/// <param name="callData">Call data for method call as RpcCallParameters serialized JSON string</param>
		/// <returns>Method result as RpcCallParameters serialized JSON string</returns>
		public string Invoke (string classAndMethodName, string callData) {

			var interfaceType = typeof (TInterface);
			var names = classAndMethodName.Split ('.');
			var className = names[0];
			var methodName = names[1];

			if (!string.Equals (className, interfaceType.Name)) {
				return null;
			}

			var proxyObj = ProxyInstance;
			var methodInfo = proxyObj.GetType ().GetMethod (methodName);
			var rpcParamsType = _rpcBuilder.CreateCallParamsType (className, methodInfo);
			var rpcParams = (RpcCallParameters) JsonConvert.DeserializeObject (callData, rpcParamsType);

			var methodResult = methodInfo.Invoke (proxyObj, rpcParams.GetValues ());

			var rpcResult = _rpcBuilder.CreateResultParams (className, methodInfo);
			rpcResult.SetResult (methodResult);
			return JsonConvert.SerializeObject (rpcResult);

		}
	}
}