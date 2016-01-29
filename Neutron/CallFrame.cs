using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neutron {
	public class CallFrame {
		public CallFrame ParentFrame;
		public Obj[] Arguments;
		public Obj[] Locals;

		long ReturnIP;
		Action ReturnAction;

		public CallFrame(Obj[] Args, int LocalCount, long RetIP, CallFrame PFrame, Action OnReturn) {
			Locals = new Obj[LocalCount];
			ParentFrame = PFrame;
			Arguments = Args;
			ReturnIP = RetIP;
			ReturnAction = OnReturn;
		}

		public Obj[] GetLocalsArray(int Level) {
			if (Level == 0)
				return Locals;
			else
				return ParentFrame.GetLocalsArray(Level - 1);
		}

		public void SetLocalsCount(int Num) {
			Obj[] NewLocals = new Obj[Num];
			if (Locals.Length > 0 || Num > 0)
				Array.Copy(Locals, NewLocals, Locals.Length < Num ? Locals.Length : Num);
			Locals = NewLocals;
		}

		public long Return() {
			if (ReturnAction != null)
				ReturnAction();
			return ReturnIP;
		}
	}
}
