using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

using Neutron;

namespace Test {
	class Program {
		static Random Rnd = new Random();

		static string GetString(int Len) {
			string Ret = "";
			for (int i = 0; i < Len; i++)
				Ret += (char)Rnd.Next((int)'A', (int)'Z');
			return Ret;
		}

		static void Main(string[] args) {
			Console.Title = "NeutronVM Test";

			Assembler Asm = new Assembler()
				.OpCode(Opcode.PUSHINT64).AddressOf("1")
				.OpCode(Opcode.PUSHINT32).Constant(1)
				.OpCode(Opcode.REGINT)
				.OpCode(Opcode.PUSHINT32).Constant(0)
				.OpCode(Opcode.PUSHINT64).AddressOf("Main")
				.OpCode(Opcode.CALL)
				.OpCode(Opcode.RELJMP).Constant((long)-1)
				.Label("Main")
				.OpCode(Opcode.PUSHSTRING).Constant("Hello World!")
				.OpCode(Opcode.PUSHINT32).Constant(1)
				.OpCode(Opcode.LOAD).Constant("print".GetHashCode())
				.OpCode(Opcode.CALL)
				.OpCode(Opcode.RET)
				.Label("1")
				.OpCode(Opcode.PUSHSTRING).Constant("Interrupt #1")
				.OpCode(Opcode.PUSHINT32).Constant(1)
				.OpCode(Opcode.LOAD).Constant("print".GetHashCode())
				.OpCode(Opcode.CALL)
				.OpCode(Opcode.TERMINATE)
				.OpCode(Opcode.RET);

			VM V = new VM(Asm.ToBytecode());
			V.Store("getstring".GetHashCode(), new Func<int, string>(GetString));
			V.Store("print".GetHashCode(), new Action<object>(Console.WriteLine));

			new Thread(() => {
				Thread.Sleep(1000);
				V.Interrupt(1);
			}).Start();

			while (V.Executing)
				V.Run();

			Console.WriteLine("Done!");
			Console.ReadLine();
		}
	}
}