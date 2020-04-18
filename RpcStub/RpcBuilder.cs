using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace RpcStub {

	internal class RpcBuilder {

		private ModuleBuilder _moduleBuilder;
		private IRpcClient _rpcClient;

		private Dictionary<string, Type> _typeCache;

		public RpcBuilder () {
			var assemblyName = new AssemblyName ("RpcStub");
			var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly (assemblyName, AssemblyBuilderAccess.Run);

			var moduleBuilder = assemblyBuilder.DefineDynamicModule ("RpcStubDynamicModule");
			_moduleBuilder = moduleBuilder;
			_typeCache = new Dictionary<string, Type> ();
		}

		public RpcBuilder (IRpcClient rpcClient) : this () {
			_rpcClient = rpcClient;
		}

		private void BuildPropertyFromParam (TypeBuilder tb, string name, Type type) {
			var property = tb.DefineProperty (name, PropertyAttributes.None, type, null);

			var propertyBackField = tb.DefineField (
				$"_{name}",
				type,
				FieldAttributes.Private);

			MethodAttributes getSetAttr = MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig;

			// Define the "get" accessor method for Number. The method returns
			// an integer and has no arguments. (Note that null could be 
			// used instead of Types.EmptyTypes)
			var propertyGetMethod = tb.DefineMethod (
				$"get_{name}",
				getSetAttr,
				type,
				Type.EmptyTypes);

			var getIL = propertyGetMethod.GetILGenerator ();
			// For an instance property, argument zero is the instance. Load the 
			// instance, then load the private field and return, leaving the
			// field value on the stack.
			getIL.Emit (OpCodes.Ldarg_0);
			getIL.Emit (OpCodes.Ldfld, propertyBackField);
			getIL.Emit (OpCodes.Ret);

			// Define the "set" accessor method for Number, which has no return
			// type and takes one argument of type int (Int32).
			var propertySetMethod = tb.DefineMethod (
				$"set_{name}",
				getSetAttr,
				null,
				new Type[] { type });

			var setIL = propertySetMethod.GetILGenerator ();
			// Load the instance and then the value argument of the set method, then store the
			// argument in the field.
			setIL.Emit (OpCodes.Ldarg_0);
			setIL.Emit (OpCodes.Ldarg_1);
			setIL.Emit (OpCodes.Stfld, propertyBackField);
			setIL.Emit (OpCodes.Ret);

			// Last, map the "get" and "set" accessor methods to the 
			// PropertyBuilder. The property is now complete. 
			property.SetGetMethod (propertyGetMethod);
			property.SetSetMethod (propertySetMethod);
		}

		public RpcCallParameters CreateCallParams (string className, MethodInfo targetMethod) {
			return Activator.CreateInstance (CreateCallParamsType (className, targetMethod)) as RpcCallParameters;
		}

		public Type CreateCallParamsType (string className, MethodInfo targetMethod) {
			var typeName = $"__Rpc{className}_{targetMethod.Name}";
			if (!_typeCache.ContainsKey (typeName)) {
				var tb = _moduleBuilder.DefineType (typeName, TypeAttributes.Public | TypeAttributes.Class, typeof (RpcCallParameters));
				var paramInfos = targetMethod.GetParameters ();
				foreach (var paramInfo in paramInfos) {
					BuildPropertyFromParam (tb, paramInfo.Name, paramInfo.ParameterType);
				}
				_typeCache.Add (typeName, tb.CreateType ());
			}
			return _typeCache[typeName];
		}

		public Type CreateResultParamsType (string className, MethodInfo targetMethod) {
			var typeName = $"__Rpc{className}_{targetMethod.Name}_MethodResult";
			if (!_typeCache.ContainsKey (typeName)) {
				var tb = _moduleBuilder.DefineType (typeName, TypeAttributes.Public | TypeAttributes.Class, typeof (RpcCallParameters));
				if (targetMethod.ReturnType != typeof (void)) {
					BuildPropertyFromParam (tb, "__MethodResult", targetMethod.ReturnType);
				}
				_typeCache.Add (typeName, tb.CreateType ());
			}
			return _typeCache[typeName];
		}

		public RpcCallParameters CreateResultParams (string className, MethodInfo targetMethod) {
			return Activator.CreateInstance (CreateResultParamsType (className, targetMethod)) as RpcCallParameters;
		}

		public TInterface CreateProxyFromInterface<TInterface> () {
			var interfaceType = typeof (TInterface);

			if (!interfaceType.IsInterface) {
				throw new ArgumentException ("Type must be an interface!");
			}

			var res = DispatchProxy.Create<TInterface, RpcDispatchProxy> ();
			var proxy = res as RpcDispatchProxy;
			if (proxy != null) {
				proxy.RpcClient = _rpcClient;
			}
			return res;
		}

	}
}