using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Neutron {
	public enum Opcode : byte {
		NOP = 0,
		CALL,
		TAILCALL,
		RET
	}

	public class VM {
		public bool Executing { get; private set; }
		Bytecode Bytecode;
		Stack<CallFrame> CallStack;
		long IP;

		CallFrame CurrentFrame
		{
			get
			{
				return CallStack.Peek();
			}
		}

		public VM(Bytecode Code, long IP = 0) {
			this.IP = IP;
			CallStack = new Stack<CallFrame>();
			Bytecode = Code;
			Executing = true;

			CallStack.Push(new CallFrame());
		}

		public void Run(int InstrCount = 16) {
			for (int i = 0; i < InstrCount; i++) {
				Opcode Code = Bytecode.GetOpcode(ref IP);
				switch (Code) {
					case Opcode.NOP:
						break;
					case Opcode.CALL: {
							long NewIP = Bytecode.GetInt64(ref this.IP);

							CallFrame NewFrame = new CallFrame(NewIP);
							CallStack.Push(NewFrame);
							this.IP = NewIP;
							break;
						}
					case Opcode.TAILCALL:
						this.IP = CurrentFrame.IP;
						break;
					case Opcode.RET:
						break;
					default:
						throw new NotImplementedException("Not implemented opcode: " + Code);
				}
			}
		}
	}
}