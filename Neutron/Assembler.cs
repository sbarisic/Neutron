using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Neutron {
	public class Assembler {
		MemoryStream Mem;
		Dictionary<string, long> Labels;
		Dictionary<string, Queue<long>> Addresses;

		public Assembler() {
			Mem = new MemoryStream();
			Labels = new Dictionary<string, long>();Addresses = new Dictionary<string, Queue<long>>();
		}

		public Assembler OpCode(Opcode Op) {
			Mem.WriteByte((byte)Op);
			return this;
		}

		public Assembler Constant(object Obj) {
			if (Obj == null)
				throw new InvalidOperationException();

			MethodInfo GetBytesInfo = typeof(BitConverter).GetMethod("GetBytes", new[] { Obj.GetType() });
			if (GetBytesInfo == null)
				throw new Exception("Cannot convert type to bytes: " + Obj.GetType());

			byte[] Bytes = (byte[])GetBytesInfo.Invoke(null, new object[] { Obj });
			Mem.Write(Bytes, 0, Bytes.Length);
			return this;
		}

		public Assembler Label(string Name) {
			Labels[Name] = Mem.Position;
			if (Addresses.ContainsKey(Name)) {
				Queue<long> Locations = Addresses[Name];
				while (Locations.Count > 0) {
					long OldPos = Mem.Position;
					Mem.Position = Locations.Dequeue();
					Constant(Labels[Name]);
					Mem.Position = OldPos;
				}
				Addresses.Remove(Name);
			}
			return this;
		}

		public Assembler AddressOf(string Name) {
			if (Labels.ContainsKey(Name))
				Constant(Labels[Name]);
			else {
				if (!Addresses.ContainsKey(Name))
					Addresses.Add(Name, new Queue<long>());
				Addresses[Name].Enqueue(Mem.Position);
				Constant((long)-1);
			}
			return this;
		}

		public byte[] ToByteArray() {
			if (Addresses.Count > 0)
				throw new Exception(string.Format("There are {0} unresolved AddressOf statements", Addresses.Count));
			byte[] Ret = Mem.ToArray();
			Mem.Dispose();
			return Ret;
		}

		public Bytecode ToBytecode() {
			return new BytecodeArray(ToByteArray());
		}
	}
}
