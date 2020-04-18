using System;
using System.Linq;

namespace RpcStub {

	public class RpcCallParameters {
		public object[] GetValues () {
			return this.GetType ().GetProperties ().Select (c => c.GetValue (this)).ToArray ();
		}

		public void SetValue (string name, object value) {
			var propInfo = this.GetType ().GetProperties ().FirstOrDefault (c => c.Name == name);
			if (propInfo != null) {
				propInfo.SetValue (this, value);
			}
		}
		public void SetValue (int position, object value) {
			var propInfo = this.GetType ().GetProperties ().Skip (position).Take (1).FirstOrDefault ();
			if (propInfo != null) {
				propInfo.SetValue (this, value);
			}
		}

		public void SetValues (object[] args) {
			var propInfos = this.GetType ().GetProperties ().ToList ();
			if (propInfos.Count != args.Length) {
				throw new ArgumentException ("Amount of values does not match Parameters count");
			}
			for (var i = 0; i < propInfos.Count; i++) {
				var propInfo = propInfos[i];
				propInfo.SetValue (this, args[i]);
			}
		}

		/// <summary>
		/// Get the result from the call
		/// </summary>
		/// <returns></returns>
		public object GetResult () {
			return this.GetType ().GetProperties ().FirstOrDefault (c => c.Name == "__MethodResult")?.GetValue (this);
		}

		/// <summary>
		/// Set the result parameter
		/// </summary>
		/// <param name="value">Result value</param>
		public void SetResult (object value) {
			var propInfo = this.GetType ().GetProperties ().FirstOrDefault (c => c.Name == "__MethodResult");
			if (propInfo != null) {
				propInfo.SetValue (this, value, null);
			}
		}

		/// <summary>
		/// Validates, if the call returned a result
		/// </summary>
		/// <returns>True, if the result parameter is set</returns>
		public bool HasResult () {
			return this.GetType ().GetProperties ().Any (c => c.Name == "__MethodResult" && c.DeclaringType != typeof (void));
		}
	}

}