using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Neutron;

namespace Test {
	class Program {
		static void Main(string[] args) {
			Console.Title = "NeutronVM Test";

			Bytecode BCode = null;

			VM V = new VM(BCode);
			while (V.Executing)
				V.Run();

			Console.WriteLine("Done!");
			Console.ReadLine();
		}
	}
}