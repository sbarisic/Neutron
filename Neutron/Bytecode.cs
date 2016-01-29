using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Neutron {
	public abstract class Bytecode {
		public virtual long Length
		{
			get
			{
				return -1;
			}
		}

		public abstract byte GetByte(ref long IP);

		public virtual byte[] GetBytes(ref long IP, int Len) {
			byte[] Ret = new byte[Len];
			for (int i = 0; i < Len; i++)
				Ret[i] = GetByte(ref IP);
			return Ret;
		}

		public virtual Opcode GetOpcode(ref long IP) {
			return (Opcode)GetByte(ref IP);
		}

		public virtual int GetInt32(ref long IP) {
			return BitConverter.ToInt32(GetBytes(ref IP, sizeof(int)), 0);
		}

		public virtual long GetInt64(ref long IP) {
			return BitConverter.ToInt64(GetBytes(ref IP, sizeof(long)), 0);
		}

		public virtual double GetDouble(ref long IP) {
			return BitConverter.ToDouble(GetBytes(ref IP, sizeof(double)), 0);
		}
	}

	public class BytecodeArray : Bytecode {
		byte[] TehArray;

		public override long Length
		{
			get
			{
				return TehArray.LongLength;
			}
		}

		public BytecodeArray(byte[] Arr) {
			TehArray = new byte[Arr.Length];
			Arr.CopyTo(TehArray, 0);
		}

		public override byte GetByte(ref long IP) {
			return TehArray[IP++];
		}

		public static implicit operator BytecodeArray(byte[] Arr) {
			return new BytecodeArray(Arr);
		}
	}

	public class BytecodeStream : Bytecode {
		Stream TehStream;

		public override long Length
		{
			get
			{
				return TehStream.Length;
			}
		}

		public BytecodeStream(Stream Str) {
			TehStream = Str;
		}

		public override byte GetByte(ref long IP) {
			TehStream.Seek(IP, SeekOrigin.Begin);
			return (byte)TehStream.ReadByte();
		}

		public override byte[] GetBytes(ref long IP, int Len) {
			TehStream.Seek(IP, SeekOrigin.Begin);
			byte[] Ret = new byte[Len];
			TehStream.Read(Ret, 0, Len);
			return Ret;
		}

		public static implicit operator BytecodeStream(Stream Str) {
			return new BytecodeStream(Str);
		}
	}
}