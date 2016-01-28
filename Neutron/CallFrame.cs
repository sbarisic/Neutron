using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neutron {
	class CallFrame {
		public Obj[] Args;
		public long IP;

		public CallFrame(long IP = 0) {
			Args = new Obj[] { };
			this.IP = IP;
		}
	}
}
