using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neutron {
	public class Obj {
		public static readonly Obj NULL = new Obj(null);

		public object Value;
		Type ValueType;

		public Obj(object Val) {
			Value = Val;
			if (Value != null)
				ValueType = Value.GetType();
		}

		public Type GetValueType() {
			return ValueType;
		}

		public string GetValueTypeName() {
			Type T = GetValueType();
			if (T != null)
				return T.Name;
			return "null";
		}

		public Obj Invoke(params Obj[] Args) {
			if (Value is Delegate) {
				object[] ObjArgs = new object[Args.Length];
				for (int i = 0; i < Args.Length; i++)
					ObjArgs[i] = Args[i].Value;
				object RetVal = ((Delegate)Value).DynamicInvoke(ObjArgs);
				if (RetVal == null)
					return null;
				return new Obj(RetVal);
			}

			if (Value == null)
				throw new InvalidOperationException("Cannot invoke null");
			throw new InvalidOperationException("Cannot invoke " + Value.GetType());
		}

		public override string ToString() {
			if (Value == null)
				return "null";
			return Value.ToString();
		}

		public static implicit operator Obj(double D) {
			return new Obj(D);
		}

		public static implicit operator Obj(int I) {
			return new Obj(I);
		}

		public static implicit operator Obj(long L) {
			return new Obj(L);
		}

		public static implicit operator Obj(Delegate Del) {
			return new Obj(Del);
		}

		public static implicit operator Obj(string Str) {
			return new Obj(Str);
		}
	}
}