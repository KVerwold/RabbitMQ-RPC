namespace RpcStub {

	public interface IRpcClient {
		string Call (string className, string method, string message);
	}

}