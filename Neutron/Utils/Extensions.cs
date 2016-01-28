using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neutron {
	static class Extensions {
		public static T[] Append<T>(this T[] Arr, params T[] Args) {
			if (Args == null || Args.Length == 0)
				throw new InvalidOperationException();
			T[] New = new T[Arr.Length + Args.Length];
			Array.Copy(Arr, New, Arr.Length);
			Array.Copy(Args, 0, New, Arr.Length, Args.Length);
			return New;
		}
	}
}