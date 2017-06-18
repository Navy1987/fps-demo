using System;
using System.Text;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace zprotobuf
{
	public abstract class wire {
		private static int oid = 0;
		private static Dictionary<IntPtr, wire> uds = new Dictionary<IntPtr, wire>();
		private static int encode_cb(ref dll.args arg) {
			wire obj = uds[arg.ud];
			return obj._encode_field(ref arg);
		}

		private static int decode_cb(ref dll.args arg) {
			wire obj = uds[arg.ud];
			return obj._decode_field(ref arg);
		}

		private IntPtr udbegin() {
			IntPtr id = (IntPtr)(++oid);
			uds[id] = this;
			return id;
		}
		private void udend(IntPtr id) {
			uds[id] = null;
		}

		protected int write(ref dll.args arg, byte[] src) {
			if (src.Length > arg.buffsz)
				return dll.OOM;
			Marshal.Copy(src, 0, arg.buff, src.Length);
			return src.Length;
		}

		protected int write(ref dll.args arg, bool val) {
			byte[] src = BitConverter.GetBytes(val);
 			return write(ref arg, src);
		}

		protected int write(ref dll.args arg, int val) {
			byte[] src = BitConverter.GetBytes(val);
			return write(ref arg, src);
		}

		private byte[] read(ref dll.args arg) {
			byte[] ret = new byte[arg.buffsz];
			Marshal.Copy(arg.buff, ret, 0, ret.Length);
			return ret;
		}
		protected int read(ref dll.args arg, out bool val) {
			val = false;
			if (arg.buffsz < 1)
				return dll.ERROR;
			val = BitConverter.ToBoolean(read(ref arg), 0);
			return arg.buffsz;
		}
		protected int read(ref dll.args arg, out int val) {
			val = 0;
			if (arg.buffsz < sizeof(int))
				return dll.ERROR;
			val = BitConverter.ToInt32(read(ref arg), 0);
			return arg.buffsz;
		}
		protected int read(ref dll.args arg, out byte[] val) {
			val = read(ref arg);
			return arg.buffsz;
		}

		public int _encode(IntPtr buff, int len, IntPtr st) {
			IntPtr id = udbegin();
			int err = dll.encode(st, buff, len, encode_cb, id);
			udend(id);
			return err;
		}

		public int _decode(IntPtr buff, int len, IntPtr st) {
			IntPtr id = udbegin();
			int err = dll.decode(st, buff, len, decode_cb, id);
			udend(id);
			return err;
		}

		public virtual int _serialize(out byte[] dat) {
			dat = null;
			Debug.Assert("NotImplement" == null);
			return 0;
		}
		public virtual int _parse(byte[] dat, int size) {
			Debug.Assert("NotImplement" == null);
			return 0;
		}
		public virtual int _tag() {
			Debug.Assert("NotImplement" == null);
			return 0;
		}
		//abstract function
		public abstract string _name();
		protected abstract int _encode_field(ref dll.args arg);
		protected abstract int _decode_field(ref dll.args arg);
	}

	public class wiretree {
		private string protodef = "";
		private IntPtr Z = IntPtr.Zero;
		private IntPtr buff = IntPtr.Zero;
		private int bufflen = 0;
		private Dictionary<string, IntPtr> cache = new Dictionary<string,IntPtr>();
		public wiretree(string def) {
			bufflen = 64;
			buff = Marshal. AllocHGlobal(bufflen);
			protodef = def;
			Z = dll.parse(protodef);
		}
		~wiretree() {
			Marshal.FreeHGlobal(buff);
		}

		private IntPtr query(string name) {
			if (cache.ContainsKey(name))
				return cache[name];
			IntPtr st = dll.query(Z, name);
			cache[name] = st;
			return st;
		}

		public int tag(string name) {
			IntPtr st = query(name);
			return dll.tag(st);
		}

		private void expand() {
			bufflen *= 2;
			IntPtr sz = (IntPtr)bufflen;
			buff = Marshal.ReAllocHGlobal(buff, sz);
			return ;
		}

		private int encodecheck(ref wire obj) {
			int sz;
			IntPtr st = query(obj._name());
			for (;;) {
				sz = obj._encode(buff, bufflen, st);
				if (sz == dll.ERROR)
					return sz;
				if (sz == dll.OOM) {
					expand();
					continue;
				}
				return sz;
			}
		}
		public int encode(wire obj, out byte[] data) {
			int sz;
			data = null;
			sz = encodecheck(ref obj);
			if (sz > 0) {
				data = new byte[sz];
				Marshal.Copy(buff, data, 0, sz);
			}
			return sz;
		}
		public int decode(wire obj, byte[] data, int size) {
			int len = bufflen;
			IntPtr st = query(obj._name());
			while (bufflen < size)
				bufflen *= 2;
			if (bufflen != len)
				buff = Marshal.ReAllocHGlobal(buff, (IntPtr)bufflen);
			Marshal.Copy(data, 0, buff, size);
			return obj._decode(buff, size, st);
		}
	}
}
