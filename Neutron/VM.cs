using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Neutron {
	public enum Opcode : byte {
		TERMINATE = 0,
		NOP,
		PUSHDBL, // PUSHDBL double; pushes double onto stack
		PUSHINT32, // PUSHINT32 int; pushes int onto stack
		PUSHINT64, // PUSHINT64 long; pushes long onto stack
		STORE, // STORE int; pops object from stack and stores into register [int]
		LOAD, // LOAD int; loads register [int] and pushes the object onto stack
		CALL, // CALL; pops a callable object and the number of arguments from stack before calling
		RET, // RET; returns from a CALL instruction
	}

	public class VM {
		public bool Executing { get; private set; }
		Bytecode Bytecode;
		long IP;

		public Stack<Obj> Stack;
		public Stack<CallFrame> CallStack;
		public Dictionary<int, Obj> Regs;

		public VM(Bytecode Code, long IP = 0) {
			this.IP = IP;
			Bytecode = Code;
			Executing = true;
			Stack = new Stack<Obj>();
			Regs = new Dictionary<int, Obj>();
			CallStack = new Stack<CallFrame>();
		}

		public void Push(Obj O) {
			Stack.Push(O);
		}

		public Obj Pop() {
			if (Stack.Count > 0)
				return Stack.Pop();
			return Obj.NULL;
		}

		public T Pop<T>() {
			Obj O = Pop();
			if (O.GetValueType() == typeof(T))
				return (T)O.Value;
			throw new InvalidOperationException(string.Format("Cannot pop {0} as {1}", O.GetValueTypeName(), typeof(T).Name));
		}

		public Obj Load(int K) {
			if (!Regs.ContainsKey(K))
				return Obj.NULL;
			return Regs[K];
		}

		public void Store(int K, Obj V) {
			Regs[K] = V;
		}

		public void Run(int InstrCount = 16) {
			if (!(IP < Bytecode.Length))
				Executing = false;

			for (int Iteration = 0; (Iteration < InstrCount) && (IP < Bytecode.Length) && Executing; Iteration++) {
				Opcode Code = Bytecode.GetOpcode(ref IP);
				switch (Code) {
					case Opcode.TERMINATE:
						Executing = false;
						break;
					case Opcode.NOP:
						break;
					case Opcode.PUSHDBL:
						Push(Bytecode.GetDouble(ref IP));
						break;
					case Opcode.PUSHINT32:
						Push(Bytecode.GetInt32(ref IP));
						break;
					case Opcode.PUSHINT64:
						Push(Bytecode.GetInt64(ref IP));
						break;
					case Opcode.STORE:
						Store(Bytecode.GetInt32(ref IP), Stack.Pop());
						break;
					case Opcode.LOAD:
						Push(Load(Bytecode.GetInt32(ref IP)));
						break;
					case Opcode.CALL: {
							Obj Fnc = Pop();

							int ArgCnt = Pop<int>();
							Obj[] Args = new Obj[ArgCnt];
							for (int i = 0; i < ArgCnt; i++)
								Args[i] = Pop();

							CallStack.Push(new CallFrame(Args, IP));

							if (typeof(Delegate).IsAssignableFrom(Fnc.GetValueType())) {
								Obj Ret = Fnc.Invoke(Args);
								CallFrame ReturnFrame = CallStack.Pop();
								IP = ReturnFrame.ReturnIP;
								if (Ret != null)
									Push(Ret);
							} else if (Fnc.GetValueType() == typeof(long)) {
								IP = (long)Fnc.Value;
							} else
								throw new InvalidOperationException();

							break;
						}
					case Opcode.RET:
						IP = CallStack.Pop().ReturnIP;
						break;
					default:
						throw new NotImplementedException("Not implemented opcode: " + Code);
				}
			}
		}
	}
}