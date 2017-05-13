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
public class r_foo:wirep {
	public int hello;
	public string world;

	public override string _name() {
		return "r_foo";
	}
	protected override int _encode_field(ref dll.args args)  {
		switch (args.tag) {
		case 1:
			return write(ref args, hello);
		case 2:
			return write(ref args, world);
		default:
			return dll.ERROR;
		}
	}
	protected override int _decode_field(ref dll.args args)  {
		switch (args.tag) {
		case 1:
			return read(ref args, out hello);
		case 2:
			return read(ref args, out world);
		default:
			return dll.ERROR;
		}
	}
}
public class a_foo:wirep {
	public int hello;
	public int world;

	public override string _name() {
		return "a_foo";
	}
	protected override int _encode_field(ref dll.args args)  {
		switch (args.tag) {
		case 1:
			return write(ref args, hello);
		case 2:
			return write(ref args, world);
		default:
			return dll.ERROR;
		}
	}
	protected override int _decode_field(ref dll.args args)  {
		switch (args.tag) {
		case 1:
			return read(ref args, out hello);
		case 2:
			return read(ref args, out world);
		default:
			return dll.ERROR;
		}
	}
}
public class serializer:wiretree {

	private static serializer inst = null;

	private const string def = "\xa\x65\x72\x72\x6f\x72\x20\x30\x78\x30\x31\x20\x7b\xa\x9\x2e\x63\x6d\x64\x3a\x69\x6e\x74\x65\x67\x65\x72\x20\x31\xa\x9\x2e\x65\x72\x72\x3a\x69\x6e\x74\x65\x67\x65\x72\x20\x32\xa\x7d\xa\xa\x72\x5f\x66\x6f\x6f\x20\x30\x78\x30\x32\x20\x7b\xa\x9\x2e\x68\x65\x6c\x6c\x6f\x3a\x69\x6e\x74\x65\x67\x65\x72\x20\x31\xa\x9\x2e\x77\x6f\x72\x6c\x64\x3a\x73\x74\x72\x69\x6e\x67\x20\x32\xa\x7d\xa\xa\x61\x5f\x66\x6f\x6f\x20\x30\x78\x30\x33\x20\x7b\xa\x9\x2e\x68\x65\x6c\x6c\x6f\x3a\x69\x6e\x74\x65\x67\x65\x72\x20\x31\xa\x9\x2e\x77\x6f\x72\x6c\x64\x3a\x69\x6e\x74\x65\x67\x65\x72\x20\x32\xa\x7d\xa\xa\xa\xa\xa";
	private serializer():base(def) {

	}

public static serializer instance() {

	if (inst == null)
		inst = new serializer();
	return inst;
}


}
}
