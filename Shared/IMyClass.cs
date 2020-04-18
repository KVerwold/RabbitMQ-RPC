using System.Collections.Generic;

namespace Shared {
	public interface IMyClass {
		int TestInt (int value);
		string TestMessage (string message);
		int Min (int a, int b);
		int Max (int a, int b);
		string StringJoin (ICollection<string> strings);
	}
}