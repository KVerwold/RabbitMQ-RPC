using System;
using System.Collections.Generic;
using System.Linq;
using Shared;

namespace RpcServer {

	public class MyClass : IMyClass {
		public int TestInt (int value) {
			return value * 2;
		}
		public string TestMessage (string message) {
			return $"You have sent {message}";
		}

		public int Min (int a, int b) {
			Console.WriteLine ($"a:{a} b:{b}");
			if (a < b) {
				return a;
			}
			return b;
		}

		public int Max (int a, int b) {
			Console.WriteLine ($"a:{a} b:{b}");
			if (a > b) {
				return a;
			}
			return b;
		}

		public string StringJoin (ICollection<string> strings) {
			return string.Join (" ", strings.ToArray ());
		}

	}
}