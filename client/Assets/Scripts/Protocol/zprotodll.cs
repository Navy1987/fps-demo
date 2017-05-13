using System;
using System.Runtime.InteropServices;
using System.Collections;
using System.Collections.Generic;

namespace zprotobuf
{
	public class dll
	{
		private const string DLLNAME = "zproto";
		public const int OOM = -1;
		public const int NOFIELD = -2;
		public const int ERROR = -3;
		public struct args
		{
			public int tag;
			public int type;
			public int idx;
			public int len;
			public System.IntPtr ud;
			public int maptag;
			public System.IntPtr name;
			public System.IntPtr mapname;
			public System.IntPtr buff;
			public int buffsz;
			public System.IntPtr sttype;
			public args(int tag) {
				this.tag = tag;
				type = 0;
				idx = 0;
				len = 0;
				ud = System.IntPtr.Zero;
				maptag = 0;
				name = System.IntPtr.Zero;
				mapname = System.IntPtr.Zero;
				buff = System.IntPtr.Zero;
				buffsz = 0;
				sttype = System.IntPtr.Zero;
			}
		};

		public delegate int zproto_cb_t(ref args arg);

		[DllImport(DLLNAME, EntryPoint = "csload", CallingConvention = CallingConvention.Cdecl)]
		public extern static System.IntPtr load(string name);

		[DllImport(DLLNAME, EntryPoint = "csparse", CallingConvention = CallingConvention.Cdecl)]
		public extern static System.IntPtr parse(string content);

		[DllImport(DLLNAME, EntryPoint = "csfree", CallingConvention = CallingConvention.Cdecl)]
		public extern static void free(System.IntPtr z);

		[DllImport(DLLNAME, EntryPoint = "csquery", CallingConvention = CallingConvention.Cdecl)]
		public extern static System.IntPtr query(System.IntPtr z, string name);

		[DllImport(DLLNAME, EntryPoint = "cstag", CallingConvention = CallingConvention.Cdecl)]
		public extern static int tag(System.IntPtr st);

		[DllImport(DLLNAME, EntryPoint = "csquerytag", CallingConvention = CallingConvention.Cdecl)]
		public extern static System.IntPtr querytag(System.IntPtr z, int tag);

		[DllImport(DLLNAME, EntryPoint = "csencode", CallingConvention = CallingConvention.Cdecl)]
		public extern static int encode(System.IntPtr st, System.IntPtr ptr, int len, zproto_cb_t cb, IntPtr obj);

		[DllImport(DLLNAME, EntryPoint = "csdecode", CallingConvention = CallingConvention.Cdecl)]
		public extern static int decode(System.IntPtr st, System.IntPtr ptr, int len, zproto_cb_t cb, IntPtr obj);
	}
}
