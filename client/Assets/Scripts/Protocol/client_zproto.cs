using System;
using System.Text;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Diagnostics;
using zprotobuf;
namespace client_zproto {
public abstract class wirep:wire {
	public int _serialize(out byte[] dat) {
		return serializer.instance().encode(this, out dat);
	}
	public int _parse(byte[] dat) {
		return serializer.instance().decode(this, dat);
	}
}

public class foo:wirep {
	public int hello;
	public string world;

	public override string _name() {
		return "foo";
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

	private const string def = "\x66\x6f\x6f\x20\x30\x78\x30\x31\x20\x7b\xa\x9\x2e\x68\x65\x6c\x6c\x6f\x3a\x69\x6e\x74\x65\x67\x65\x72\x20\x31\xa\x9\x2e\x77\x6f\x72\x6c\x64\x3a\x73\x74\x72\x69\x6e\x67\x20\x32\xa\x7d\xa\xa";
	private serializer():base(def) {

	}

public static serializer instance() {

	if (inst == null)
		inst = new serializer();
	return inst;
}


}
}
