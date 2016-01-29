using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
				.OpCode(Opcode.PUSHINT32).Constant(42)
				.OpCode(Opcode.PUSHINT32).Constant(1)
				.OpCode(Opcode.LOAD).Constant("getstring".GetHashCode())
				.OpCode(Opcode.CALL)
				.OpCode(Opcode.PUSHINT32).Constant(1)
				.OpCode(Opcode.LOAD).Constant("print".GetHashCode())
				.OpCode(Opcode.CALL);

			VM V = new VM(Asm.ToBytecode());
			V.Store("getstring".GetHashCode(), new Func<int, string>(GetString));
			V.Store("print".GetHashCode(), new Action<object>(Console.WriteLine));

			while (V.Executing)
				V.Run();

			Console.WriteLine("Done!");
			Console.ReadLine();
		}
	}
}