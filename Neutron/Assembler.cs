using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Neutron {
	public class Assembler {
		List<byte> Bytes;

		public Assembler() {
			Bytes = new List<byte>();
		}

		public Assembler OpCode(Opcode Op) {
			Bytes.Add((byte)Op);
			return this;
		}

		public Assembler Constant(object Obj) {
			if (Obj == null)
				throw new InvalidOperationException();

			MethodInfo GetBytesInfo = typeof(BitConverter).GetMethod("GetBytes", new[] { Obj.GetType() });
			if (GetBytesInfo == null)
				throw new Exception("Cannot convert type to bytes: " + Obj.GetType());
			Bytes.AddRange((byte[])GetBytesInfo.Invoke(null, new object[] { Obj }));

			return this;
		}

		public byte[] ToByteArray() {
			return Bytes.ToArray();
		}

		public Bytecode ToBytecode() {
			return new BytecodeArray(ToByteArray());
		}
	}
}
