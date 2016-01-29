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
		PUSHSTRING, // PUSHSTRING string; pushes string onto stack
		STORE, // STORE int; pops object from stack and stores into register [int]
		LOAD, // LOAD int; loads register [int] and pushes the object onto stack
		STORELOCAL, // STORELOCAL byte int; pops object from stack and stores into local variable [int] at level [byte]
		LOADLOCAL, // LOADLOCAL byte int; loads local variable [int] at level [byte] and pushes onto the stack
		CALL, // CALL; pops a callable object and the number of arguments from stack before calling
		RET, // RET; returns from a CALL instruction
		REGINT, // REGINT; pops int num and object and registers as interrupt handler
		INT, // INT int; interrupts
		RELJMP, // RELJMP long; relative jump
		LOCNUM, // LOCNUM int; specifies local variable count
	}

	public class VM {
		public bool Executing { get; private set; }
		Bytecode Bytecode;
		long IP;

		Stack<Obj> Stack;
		Stack<CallFrame> CallStack;
		Dictionary<int, Obj> Regs;
		Queue<int> InterruptQueue;
		Dictionary<int, Obj> InterruptHandlers;

		bool InsideInterrupt;

		public VM(Bytecode Code, long IP = 0) {
			this.IP = IP;
			Bytecode = Code;
			Executing = true;
			Stack = new Stack<Obj>();
			Regs = new Dictionary<int, Obj>();
			CallStack = new Stack<CallFrame>();
			InterruptQueue = new Queue<int>();
			InterruptHandlers = new Dictionary<int, Obj>();

			InsideInterrupt = false;
			CallStack.Push(new CallFrame(new Obj[0], 0, -1, null, null));
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

		public Obj LoadLocal(int Level, int K) {
			return CallStack.Peek().GetLocalsArray(Level)[K];
		}

		public void StoreLocal(int Level, int K, Obj V) {
			CallStack.Peek().GetLocalsArray(Level)[K] = V;
		}

		public void Call(Obj Fnc, long ReturnIP, Action OnReturn = null) {
			int ArgCnt = Pop<int>();
			Obj[] Args = new Obj[ArgCnt];
			for (int i = 0; i < ArgCnt; i++)
				Args[i] = Pop();

			CallStack.Push(new CallFrame(Args, 0, ReturnIP, CallStack.Peek(), OnReturn));

			if (typeof(Delegate).IsAssignableFrom(Fnc.GetValueType())) {
				Obj Ret = Fnc.Invoke(Args);
				CallFrame ReturnFrame = CallStack.Pop();
				IP = ReturnFrame.Return();
				if (Ret != null)
					Push(Ret);
			} else if (Fnc.GetValueType() == typeof(long)) {
				IP = (long)Fnc.Value;
			} else
				throw new InvalidOperationException();
		}

		public void Interrupt(int I) {
			InterruptQueue.Enqueue(I);
		}

		public void Run(int InstrCount = 16) {
			if (!(IP < Bytecode.Length))
				Executing = false;

			for (int Iteration = 0; (Iteration < InstrCount) && (IP < Bytecode.Length) && Executing; Iteration++) {
				if (!InsideInterrupt && InterruptQueue.Count > 0) {
					int I = InterruptQueue.Peek();
					if (InterruptHandlers.ContainsKey(I)) {
						InterruptQueue.Dequeue();
						InsideInterrupt = true;
						Push(0);
						Call(InterruptHandlers[I], IP, () => InsideInterrupt = false);
					}
				}

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
					case Opcode.PUSHSTRING:
						Push(Bytecode.GetString(ref IP));
						break;
					case Opcode.STORE:
						Store(Bytecode.GetInt32(ref IP), Stack.Pop());
						break;
					case Opcode.LOAD:
						Push(Load(Bytecode.GetInt32(ref IP)));
						break;
					case Opcode.STORELOCAL: {
							int Level = Bytecode.GetByte(ref IP);
							StoreLocal(Level, Bytecode.GetInt32(ref IP), Stack.Pop());
							break;
						}
					case Opcode.LOADLOCAL: {
							int Level = Bytecode.GetByte(ref IP);
							Push(LoadLocal(Level, Bytecode.GetInt32(ref IP)));
							break;
						}
					case Opcode.CALL:
						Call(Pop(), IP);
						break;
					case Opcode.RET:
						IP = CallStack.Pop().Return();
						break;
					case Opcode.REGINT: {
							int I = Pop<int>();
							InterruptHandlers[I] = Pop();
							break;
						}
					case Opcode.INT:
						Interrupt(Bytecode.GetInt32(ref IP));
						break;
					case Opcode.RELJMP: {
							long NewIP = IP;
							NewIP += Bytecode.GetInt64(ref IP);
							IP = NewIP;
							break;
						}
					case Opcode.LOCNUM:
						CallStack.Peek().SetLocalsCount(Bytecode.GetInt32(ref IP));
						break;
					default:
						throw new NotImplementedException("Not implemented opcode: " + Code);
				}
			}
		}
	}
}