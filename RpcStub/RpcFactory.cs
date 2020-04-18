using System;
using System.Reflection;

namespace RpcStub {

	public static class RpcFactory {

		public static TInterface CreateProxy<TInterface> (IRpcClient rpcClient) {
			var interfaceType = typeof (TInterface);

			if (!interfaceType.IsInterface) {
				throw new ArgumentException ("Generic type must be an interface!");
			}

			var res = DispatchProxy.Create<TInterface, RpcDispatchProxy> ();
			var proxy = res as RpcDispatchProxy;
			if (proxy != null) {
				proxy.RpcClient = rpcClient;
			}
			return res;
		}

	}

}