using System;
using System.Text;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Diagnostics;
using zprotobuf;
namespace client_zproto {
public abstract class wirep:wire {
	public override int _serialize(out byte[] dat) {
		return serializer.instance().encode(this, out dat);
	}
	public override int _parse(byte[] dat, int size) {
		return serializer.instance().decode(this, dat, size);
	}
	public override int _tag() {
		return serializer.instance().tag(_name());
	}
}

public class vector3:wirep {
	public int x;
	public int y;
	public int z;

	public override string _name() {
		return "vector3";
	}
	protected override int _encode_field(ref dll.args args)  {
		switch (args.tag) {
		case 1:
			return write(ref args, x);
		case 2:
			return write(ref args, y);
		case 3:
			return write(ref args, z);
		default:
			return dll.ERROR;
		}
	}
	protected override int _decode_field(ref dll.args args)  {
		switch (args.tag) {
		case 1:
			return read(ref args, out x);
		case 2:
			return read(ref args, out y);
		case 3:
			return read(ref args, out z);
		default:
			return dll.ERROR;
		}
	}
}
public class rotation:wirep {
	public int x;
	public int y;
	public int z;
	public int w;

	public override string _name() {
		return "rotation";
	}
	protected override int _encode_field(ref dll.args args)  {
		switch (args.tag) {
		case 1:
			return write(ref args, x);
		case 2:
			return write(ref args, y);
		case 3:
			return write(ref args, z);
		case 4:
			return write(ref args, w);
		default:
			return dll.ERROR;
		}
	}
	protected override int _decode_field(ref dll.args args)  {
		switch (args.tag) {
		case 1:
			return read(ref args, out x);
		case 2:
			return read(ref args, out y);
		case 3:
			return read(ref args, out z);
		case 4:
			return read(ref args, out w);
		default:
			return dll.ERROR;
		}
	}
}
public class error:wirep {
	public int cmd;
	public int err;

	public override string _name() {
		return "error";
	}
	protected override int _encode_field(ref dll.args args)  {
		switch (args.tag) {
		case 1:
			return write(ref args, cmd);
		case 2:
			return write(ref args, err);
		default:
			return dll.ERROR;
		}
	}
	protected override int _decode_field(ref dll.args args)  {
		switch (args.tag) {
		case 1:
			return read(ref args, out cmd);
		case 2:
			return read(ref args, out err);
		default:
			return dll.ERROR;
		}
	}
}
public class r_login:wirep {
	public int gateid;
	public byte[] user;
	public byte[] passwd;

	public override string _name() {
		return "r_login";
	}
	protected override int _encode_field(ref dll.args args)  {
		switch (args.tag) {
		case 1:
			return write(ref args, gateid);
		case 2:
			return write(ref args, user);
		case 3:
			return write(ref args, passwd);
		default:
			return dll.ERROR;
		}
	}
	protected override int _decode_field(ref dll.args args)  {
		switch (args.tag) {
		case 1:
			return read(ref args, out gateid);
		case 2:
			return read(ref args, out user);
		case 3:
			return read(ref args, out passwd);
		default:
			return dll.ERROR;
		}
	}
}
public class a_login:wirep {
	public int uid;
	public int session;

	public override string _name() {
		return "a_login";
	}
	protected override int _encode_field(ref dll.args args)  {
		switch (args.tag) {
		case 1:
			return write(ref args, uid);
		case 2:
			return write(ref args, session);
		default:
			return dll.ERROR;
		}
	}
	protected override int _decode_field(ref dll.args args)  {
		switch (args.tag) {
		case 1:
			return read(ref args, out uid);
		case 2:
			return read(ref args, out session);
		default:
			return dll.ERROR;
		}
	}
}
public class r_login_gate:wirep {
	public int uid;
	public int session;

	public override string _name() {
		return "r_login_gate";
	}
	protected override int _encode_field(ref dll.args args)  {
		switch (args.tag) {
		case 1:
			return write(ref args, uid);
		case 2:
			return write(ref args, session);
		default:
			return dll.ERROR;
		}
	}
	protected override int _decode_field(ref dll.args args)  {
		switch (args.tag) {
		case 1:
			return read(ref args, out uid);
		case 2:
			return read(ref args, out session);
		default:
			return dll.ERROR;
		}
	}
}
public class a_login_gate:wirep {

	public override string _name() {
		return "a_login_gate";
	}
	protected override int _encode_field(ref dll.args args)  {
		switch (args.tag) {
		default:
			return dll.ERROR;
		}
	}
	protected override int _decode_field(ref dll.args args)  {
		switch (args.tag) {
		default:
			return dll.ERROR;
		}
	}
}
public class r_create:wirep {
	public byte[] user;
	public byte[] passwd;

	public override string _name() {
		return "r_create";
	}
	protected override int _encode_field(ref dll.args args)  {
		switch (args.tag) {
		case 1:
			return write(ref args, user);
		case 2:
			return write(ref args, passwd);
		default:
			return dll.ERROR;
		}
	}
	protected override int _decode_field(ref dll.args args)  {
		switch (args.tag) {
		case 1:
			return read(ref args, out user);
		case 2:
			return read(ref args, out passwd);
		default:
			return dll.ERROR;
		}
	}
}
public class a_create:wirep {
	public int uid;

	public override string _name() {
		return "a_create";
	}
	protected override int _encode_field(ref dll.args args)  {
		switch (args.tag) {
		case 1:
			return write(ref args, uid);
		default:
			return dll.ERROR;
		}
	}
	protected override int _decode_field(ref dll.args args)  {
		switch (args.tag) {
		case 1:
			return read(ref args, out uid);
		default:
			return dll.ERROR;
		}
	}
}
public class r_challenge:wirep {

	public override string _name() {
		return "r_challenge";
	}
	protected override int _encode_field(ref dll.args args)  {
		switch (args.tag) {
		default:
			return dll.ERROR;
		}
	}
	protected override int _decode_field(ref dll.args args)  {
		switch (args.tag) {
		default:
			return dll.ERROR;
		}
	}
}
public class a_challenge:wirep {
	public byte[] randomkey;

	public override string _name() {
		return "a_challenge";
	}
	protected override int _encode_field(ref dll.args args)  {
		switch (args.tag) {
		case 1:
			return write(ref args, randomkey);
		default:
			return dll.ERROR;
		}
	}
	protected override int _decode_field(ref dll.args args)  {
		switch (args.tag) {
		case 1:
			return read(ref args, out randomkey);
		default:
			return dll.ERROR;
		}
	}
}
public class r_join:wirep {
	public int join;

	public override string _name() {
		return "r_join";
	}
	protected override int _encode_field(ref dll.args args)  {
		switch (args.tag) {
		case 1:
			return write(ref args, join);
		default:
			return dll.ERROR;
		}
	}
	protected override int _decode_field(ref dll.args args)  {
		switch (args.tag) {
		case 1:
			return read(ref args, out join);
		default:
			return dll.ERROR;
		}
	}
}
public class a_join:wirep {
	public int join;
	public int uid;

	public override string _name() {
		return "a_join";
	}
	protected override int _encode_field(ref dll.args args)  {
		switch (args.tag) {
		case 1:
			return write(ref args, join);
		case 2:
			return write(ref args, uid);
		default:
			return dll.ERROR;
		}
	}
	protected override int _decode_field(ref dll.args args)  {
		switch (args.tag) {
		case 1:
			return read(ref args, out join);
		case 2:
			return read(ref args, out uid);
		default:
			return dll.ERROR;
		}
	}
}
public class r_battleinfo:wirep {

	public override string _name() {
		return "r_battleinfo";
	}
	protected override int _encode_field(ref dll.args args)  {
		switch (args.tag) {
		default:
			return dll.ERROR;
		}
	}
	protected override int _decode_field(ref dll.args args)  {
		switch (args.tag) {
		default:
			return dll.ERROR;
		}
	}
}
public class a_battleinfo:wirep {
	public int[] uid;

	public override string _name() {
		return "a_battleinfo";
	}
	protected override int _encode_field(ref dll.args args)  {
		switch (args.tag) {
		case 1:
			Debug.Assert(args.idx >= 0);
			if (args.idx >= (int)uid.Length) {
				args.len = args.idx;
				return dll.NOFIELD;
			}
			return write(ref args, uid[args.idx]);
		default:
			return dll.ERROR;
		}
	}
	protected override int _decode_field(ref dll.args args)  {
		switch (args.tag) {
		case 1:
			if (args.len == 0)
				return 0;
			if (args.idx == 0)
				uid = new int[args.len];
			return read(ref args, out uid[args.idx]);
		default:
			return dll.ERROR;
		}
	}
}
public class r_sync:wirep {
	public vector3 pos;
	public rotation rot;

	public override string _name() {
		return "r_sync";
	}
	protected override int _encode_field(ref dll.args args)  {
		switch (args.tag) {
		case 1:
			return pos._encode(args.buff, args.buffsz, args.sttype);
		case 2:
			return rot._encode(args.buff, args.buffsz, args.sttype);
		default:
			return dll.ERROR;
		}
	}
	protected override int _decode_field(ref dll.args args)  {
		switch (args.tag) {
		case 1:
			pos = new vector3();
			return pos._decode(args.buff, args.buffsz, args.sttype);
		case 2:
			rot = new rotation();
			return rot._decode(args.buff, args.buffsz, args.sttype);
		default:
			return dll.ERROR;
		}
	}
}
public class a_sync:wirep {
	public int uid;
	public vector3 pos;
	public rotation rot;

	public override string _name() {
		return "a_sync";
	}
	protected override int _encode_field(ref dll.args args)  {
		switch (args.tag) {
		case 1:
			return write(ref args, uid);
		case 2:
			return pos._encode(args.buff, args.buffsz, args.sttype);
		case 3:
			return rot._encode(args.buff, args.buffsz, args.sttype);
		default:
			return dll.ERROR;
		}
	}
	protected override int _decode_field(ref dll.args args)  {
		switch (args.tag) {
		case 1:
			return read(ref args, out uid);
		case 2:
			pos = new vector3();
			return pos._decode(args.buff, args.buffsz, args.sttype);
		case 3:
			rot = new rotation();
			return rot._decode(args.buff, args.buffsz, args.sttype);
		default:
			return dll.ERROR;
		}
	}
}
public class serializer:wiretree {

	private static serializer inst = null;

	private const string def = "\x23\x23\x23\x23\x23\x23\x23\x23\x23\x23\x23\x23\x23\x23\x23\x23\x23\x61\x63\x63\x6f\x75\x6e\x74\xa\xa\x76\x65\x63\x74\x6f\x72\x33\x20\x7b\xa\x9\x2e\x78\x3a\x69\x6e\x74\x65\x67\x65\x72\x20\x31\xa\x9\x2e\x79\x3a\x69\x6e\x74\x65\x67\x65\x72\x20\x32\xa\x9\x2e\x7a\x3a\x69\x6e\x74\x65\x67\x65\x72\x20\x33\xa\x7d\xa\xa\x72\x6f\x74\x61\x74\x69\x6f\x6e\x20\x7b\xa\x9\x2e\x78\x3a\x69\x6e\x74\x65\x67\x65\x72\x20\x31\xa\x9\x2e\x79\x3a\x69\x6e\x74\x65\x67\x65\x72\x20\x32\xa\x9\x2e\x7a\x3a\x69\x6e\x74\x65\x67\x65\x72\x20\x33\xa\x9\x2e\x77\x3a\x69\x6e\x74\x65\x67\x65\x72\x20\x34\xa\x7d\xa\xa\x65\x72\x72\x6f\x72\x20\x30\x78\x31\x30\x30\x20\x7b\xa\x9\x2e\x63\x6d\x64\x3a\x69\x6e\x74\x65\x67\x65\x72\x20\x31\xa\x9\x2e\x65\x72\x72\x3a\x69\x6e\x74\x65\x67\x65\x72\x20\x32\xa\x7d\xa\xa\x72\x5f\x6c\x6f\x67\x69\x6e\x20\x30\x78\x31\x30\x31\x20\x7b\xa\x9\x2e\x67\x61\x74\x65\x69\x64\x3a\x69\x6e\x74\x65\x67\x65\x72\x20\x31\xa\x9\x2e\x75\x73\x65\x72\x3a\x73\x74\x72\x69\x6e\x67\x20\x32\xa\x9\x2e\x70\x61\x73\x73\x77\x64\x3a\x73\x74\x72\x69\x6e\x67\x20\x33\xa\x7d\xa\xa\x61\x5f\x6c\x6f\x67\x69\x6e\x20\x30\x78\x31\x30\x32\x20\x7b\xa\x9\x2e\x75\x69\x64\x3a\x69\x6e\x74\x65\x67\x65\x72\x20\x31\xa\x9\x2e\x73\x65\x73\x73\x69\x6f\x6e\x3a\x69\x6e\x74\x65\x67\x65\x72\x20\x32\xa\x7d\xa\xa\x72\x5f\x6c\x6f\x67\x69\x6e\x5f\x67\x61\x74\x65\x20\x30\x78\x31\x30\x33\x20\x7b\xa\x9\x2e\x75\x69\x64\x3a\x69\x6e\x74\x65\x67\x65\x72\x20\x31\xa\x9\x2e\x73\x65\x73\x73\x69\x6f\x6e\x3a\x69\x6e\x74\x65\x67\x65\x72\x20\x32\xa\x7d\xa\xa\x61\x5f\x6c\x6f\x67\x69\x6e\x5f\x67\x61\x74\x65\x20\x30\x78\x31\x30\x34\x20\x7b\xa\xa\x7d\xa\xa\x23\x23\x23\x23\x23\x23\x23\x23\x23\x23\x23\x23\x23\x23\x23\x23\x23\x23\x23\x23\xa\x72\x5f\x63\x72\x65\x61\x74\x65\x20\x30\x78\x31\x31\x30\x20\x7b\xa\x9\x2e\x75\x73\x65\x72\x3a\x73\x74\x72\x69\x6e\x67\x20\x31\xa\x9\x2e\x70\x61\x73\x73\x77\x64\x3a\x73\x74\x72\x69\x6e\x67\x20\x32\xa\x7d\xa\xa\x61\x5f\x63\x72\x65\x61\x74\x65\x20\x30\x78\x31\x31\x31\x20\x7b\xa\x9\x2e\x75\x69\x64\x3a\x69\x6e\x74\x65\x67\x65\x72\x20\x31\xa\x7d\xa\xa\x72\x5f\x63\x68\x61\x6c\x6c\x65\x6e\x67\x65\x20\x30\x78\x31\x31\x32\x20\x7b\xa\xa\x7d\xa\xa\x61\x5f\x63\x68\x61\x6c\x6c\x65\x6e\x67\x65\x20\x30\x78\x31\x31\x33\x20\x7b\xa\x9\x2e\x72\x61\x6e\x64\x6f\x6d\x6b\x65\x79\x3a\x73\x74\x72\x69\x6e\x67\x20\x31\xa\x7d\xa\xa\x23\x23\x23\x23\x23\x23\x23\xa\x72\x5f\x6a\x6f\x69\x6e\x20\x30\x78\x31\x31\x34\x20\x7b\xa\x9\x2e\x6a\x6f\x69\x6e\x3a\x69\x6e\x74\x65\x67\x65\x72\x20\x31\xa\x7d\xa\xa\x61\x5f\x6a\x6f\x69\x6e\x20\x30\x78\x31\x31\x35\x20\x7b\xa\x9\x2e\x6a\x6f\x69\x6e\x3a\x69\x6e\x74\x65\x67\x65\x72\x20\x31\xa\x9\x2e\x75\x69\x64\x3a\x69\x6e\x74\x65\x67\x65\x72\x20\x32\xa\x7d\xa\xa\x72\x5f\x62\x61\x74\x74\x6c\x65\x69\x6e\x66\x6f\x20\x30\x78\x31\x31\x36\x20\x7b\xa\xa\x7d\xa\xa\x61\x5f\x62\x61\x74\x74\x6c\x65\x69\x6e\x66\x6f\x20\x30\x78\x31\x31\x37\x20\x7b\xa\x9\x2e\x75\x69\x64\x3a\x69\x6e\x74\x65\x67\x65\x72\x5b\x5d\x20\x31\xa\x7d\xa\xa\x72\x5f\x73\x79\x6e\x63\x20\x30\x78\x31\x31\x38\x20\x7b\xa\x9\x2e\x70\x6f\x73\x3a\x76\x65\x63\x74\x6f\x72\x33\x20\x31\xa\x9\x2e\x72\x6f\x74\x3a\x72\x6f\x74\x61\x74\x69\x6f\x6e\x20\x32\xa\x7d\xa\xa\x61\x5f\x73\x79\x6e\x63\x20\x30\x78\x31\x31\x39\x20\x7b\xa\x9\x2e\x75\x69\x64\x3a\x69\x6e\x74\x65\x67\x65\x72\x20\x31\xa\x9\x2e\x70\x6f\x73\x3a\x76\x65\x63\x74\x6f\x72\x33\x20\x32\xa\x9\x2e\x72\x6f\x74\x3a\x72\x6f\x74\x61\x74\x69\x6f\x6e\x20\x33\xa\x7d\xa\xa";
	private serializer():base(def) {

	}

public static serializer instance() {

	if (inst == null)
		inst = new serializer();
	return inst;
}


}
}
