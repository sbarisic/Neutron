using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neutron {
	public class CallFrame {
		public Obj[] Arguments;
		public long ReturnIP;

		public CallFrame(Obj[] Args, long RetIP) {
			Arguments = Args;
			ReturnIP = RetIP;
		}
	}
}
