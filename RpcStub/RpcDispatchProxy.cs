using System.Reflection;
using Newtonsoft.Json;

namespace RpcStub {
	public class RpcDispatchProxy : DispatchProxy {

		public IRpcClient RpcClient { get; set; }

		/// <summary>
		/// 
		/// This method is invoked from the proxied interface methods.
		/// From proxied method parameters a dynamic class is created (RpcCallParameters) 
		/// by mapping the method parameters as class properties which allows 
		/// serializing / deserializing as JSON object for the RPC call.
		/// 
		/// The Rpc Call is excuted through the attached RpcClient interface and the
		/// result from the call is returned from the Invokde method.
		/// /// </summary>
		/// <param name="targetMethod">Method to be invokded from the proxied interface</param>
		/// <param name="args">Method parameters</param>
		/// <returns>Result from the method call</returns>
		protected override object Invoke (MethodInfo targetMethod, object[] args) {
			// prepare parameters for RPC call
			var rpcBuilder = new RpcBuilder ();
			var targetClassName = targetMethod.DeclaringType.Name;
			var rpcCallParams = rpcBuilder.CreateCallParams (targetClassName, targetMethod);
			rpcCallParams.SetValues (args);
			var requestData = JsonConvert.SerializeObject (rpcCallParams);
			// RPC call
			var response = RpcClient.Call (targetClassName, targetMethod.Name, requestData);
			// Get result from RPC call		
			var rpcResultType = rpcBuilder.CreateResultParamsType (targetClassName, targetMethod);
			var rpcResult = (RpcCallParameters) JsonConvert.DeserializeObject (response, rpcResultType);
			return rpcResult.GetResult ();
		}
	}
}