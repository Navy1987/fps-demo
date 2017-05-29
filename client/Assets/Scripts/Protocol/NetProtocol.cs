using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using client_zproto;

using zprotobuf;

public class NetProtocol {
	//delegate
	public delegate void cb_t(int err, wire obj);
	public delegate void event_cb_t();
	//socket
	private NetSocket socket = new NetSocket();
	private byte[] buffer = new byte[8];
	private short length_val = 0;
	//protocol
	private error error_response = new error();
	private Dictionary<int, wire> protocol_obj = new Dictionary<int, wire>();
	private Dictionary<int, cb_t> protocol_cb = new Dictionary<int, cb_t>();
	//event
	private int socket_status = NetSocket.CLOSE;
	private event_cb_t event_connect = null;
	private event_cb_t event_close = null;
	private string connect_addr;
	private int connect_port;

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

	public NetProtocol() {
		Register(error_response, error);
	}

	public void Close() {
		length_val = 0;
		socket.Close();
		socket_status = socket.Status;
		Debug.Log("[NetProtocol] Close");
		return ;
	}

	public void Connect(string addr, int port, event_cb_t open, event_cb_t close) {
		Close();
		Debug.Log("Connect:" + addr + ":" + port);
		connect_addr = addr;
		connect_port = port;
		event_connect = open;
		event_close = close;
		socket.Connect(addr, port);
		socket_status = socket.Status;
	}

	public void Reconnect() {
		if (socket.Status == NetSocket.CONNECTED)
			return ;
		socket.Connect(connect_addr, connect_port);
		socket_status = socket.Status;
	}

	public bool isConnected() {
		return socket.Status == NetSocket.CONNECTED;
	}

	public bool Send(wire obj) {
		if (!isConnected()) {
			Debug.Log("[NetProtocol] Send:" + obj._name() + " disconnect" + socket.Status);
			return false;
		}
		int cmd = obj._tag();
		Debug.Log("Send Cmd:" + cmd + " name:" + obj._name());
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
		Debug.Log("[NetProtocol] Register:" + obj._name() + " tag:" + cmd);
		Debug.Assert(!protocol_obj.ContainsKey(cmd));
		Debug.Assert(!protocol_cb.ContainsKey(cmd));
		protocol_obj[cmd] = obj;
		protocol_cb[cmd] = cb;
		return ;
	}

	public void Update() {
		if (socket_status == NetSocket.CLOSE)
			return ;
		if (socket.Status == NetSocket.DISCONNECT) {
			event_close();
			socket_status = NetSocket.DISCONNECT;
			Debug.Log("[NetProtocol] Reconnect addr " + connect_addr + ":" + connect_port);
			socket.Connect(connect_addr, connect_port);
		}
		switch (socket_status) {
		case NetSocket.DISCONNECT:
			if (socket.Status == NetSocket.CONNECTED) {
				socket_status = NetSocket.CONNECTED;
				event_connect();
			}
			break;
		}
		if (socket.Length < 2)
			return ;
		if (length_val == 0) {
			socket.Read(buffer, 2);
			length_val = BitConverter.ToInt16(buffer, 0);
			length_val = System.Net.IPAddress.NetworkToHostOrder(length_val);
		}
		if (socket.Length < length_val)
			return ;
		if (buffer.Length < length_val)
			buffer = new byte[length_val];
		socket.Read(buffer, 4);
		int cmd = BitConverter.ToInt32(buffer, 0);
		Debug.Assert(length_val > 4);
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

public class NetInstance {
	static public NetProtocol Login = new NetProtocol();
	static public NetProtocol Gate = new NetProtocol();
	static public void Update() {
		Login.Update();
		Gate.Update();
	}
}

