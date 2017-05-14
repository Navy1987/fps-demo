using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using client_zproto;

using zprotobuf;

public class NetProtocol {
	static private NetProtocol inst = null;
	public delegate void cb_t(int err, wire obj);
	private byte[] buffer = new byte[8];
	private short length_val = 0;
	private error error_response = new error();
	private NetSocket socket = new NetSocket();
	private Dictionary<int, wire> protocol_obj = new Dictionary<int, wire>();
	private Dictionary<int, cb_t> protocol_cb = new Dictionary<int, cb_t>();

	static public NetProtocol Instance {
		get {
			if (inst == null)
				inst = new NetProtocol();
			return inst;
		}
	}

	private void error(int err, wire obj) {
		error errobj = (error)obj;
		int cmd = errobj.cmd;
		int errno = errobj.err;
		if (!protocol_obj.ContainsKey(cmd)) {
			Debug.Log("[NetProtocol] can't has handler of cmd[" + cmd + "]");
			return ;
		}
		cb_t cb = protocol_cb[cmd];
		cb(errno, null);
		return ;
	}

	NetProtocol() {
		Register(error_response, error);
	}


	public void Connect(string addr, int port) {
		length_val = 0;
		socket.Connect(addr, port);
	}

	public void Close() {
		socket.Close();
		return ;
	}

	public bool isConnected() {
		return socket.Status != NetSocket.DISCONNECT;
	}

	public bool Send(wire obj) {
		if (socket.Status != NetSocket.CONNECTED)
			return false;
		int cmd = obj._tag();
		byte[] dat = null;
		obj._serialize(out dat);
		short len = (short)(4 + dat.Length);
		int need = len + 2;
		byte[] buffer = new byte[need];
		len = System.Net.IPAddress.HostToNetworkOrder(len);
		System.BitConverter.GetBytes(len).CopyTo(buffer, 0);
		System.BitConverter.GetBytes(cmd).CopyTo(buffer, 2);
		dat.CopyTo(buffer, 6);
		socket.Send(buffer);
		return true;
	}

	public void Register(wire obj, cb_t cb) {
		int cmd = obj._tag();
		Debug.Log("[NetProtocol] Register:" + obj._name() + "_TAG:" + cmd);
		Debug.Assert(!protocol_obj.ContainsKey(cmd));
		Debug.Assert(!protocol_cb.ContainsKey(cmd));
		protocol_obj[cmd] = obj;
		protocol_cb[cmd] = cb;
		return ;
	}

	public void Update() {
		if (socket.Length < 2)
			return ;
		if (length_val == 0) {
			socket.Read(buffer, 2);
			length_val = BitConverter.ToInt16(buffer, 0);
			length_val = System.Net.IPAddress.NetworkToHostOrder(length_val);
			//Debug.Log(":: Need:" + length_val);
		}
		if (socket.Length < length_val)
			return ;
		if (buffer.Length < length_val)
			buffer = new byte[length_val];
		socket.Read(buffer, 4);
		int cmd = BitConverter.ToInt32(buffer, 0);
		length_val -= sizeof(int);
		socket.Read(buffer, length_val);
		if (!protocol_obj.ContainsKey(cmd)) {
			Debug.Log("[NetProtocol] can't has handler of cmd[" + cmd + "]");
			return ;
		}
		wire obj = protocol_obj[cmd];
		int err = obj._parse(buffer, length_val);
		length_val = 0;
		//Debug.Log("[NetProtocol] Process cmd[" + obj._name() + "]Err:" + err);
		if (err < 0)
			return ;
		cb_t cb = protocol_cb[cmd];
		cb(0, obj);
		return ;
	}

}
