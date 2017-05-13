using System;
using System.IO;
using System.Net;
using System.Collections;
using System.Net.Sockets;
using UnityEngine;

class Stream {
	private byte[] buffer = null;
	private int length = 0;
	public int Length {
		get {
			return length;
		}
	}

	public byte[] GetBuffer() {
		return buffer;
	}

	public int Write(byte[]dat, int offset, int size) {
		int need = length + size;
		if (buffer == null)
			buffer = new byte[need];
		else if (buffer.Length < need) {
			byte[] old = buffer;
			buffer = new byte[need];
			Buffer.BlockCopy(old, 0, buffer, 0, length);
		}
		Buffer.BlockCopy(dat, offset, buffer, length, size);
		length = need;
		return size;
	}

	public int Read(byte[] dat, int offset, int size) {
		if (length < size)
			return 0;
		Buffer.BlockCopy(buffer, 0, dat, offset, size);
		length -= size;
		Buffer.BlockCopy(buffer, size, buffer, 0, length);
		return size;
	}

}

public class NetSocket {
	public const int CONNECTED = 1;
	public const int DISCONNECT = 2;

	private Socket s = null;
	private int status = DISCONNECT;
	private Queue sendq = new Queue();
	private byte[] buffer = new byte[128];
	private Stream readstream = new Stream();


	private static void SendCB(IAsyncResult ar) {
		NetSocket obj = (NetSocket) ar.AsyncState;
		if (obj.sendq.Count == 0)
			return ;
		Debug.Log("SendCB:" + obj.sendq.Count);
		byte[] a = (byte[]) obj.sendq.Dequeue();
		SocketError err = obj.doSend(a);
		Debug.Log("SendCB Result:" + err);
		return ;
	}
	private static void RecvCB(IAsyncResult ar) {
		Debug.Log("RecvCB");
		NetSocket obj = (NetSocket) ar.AsyncState;
		int read = obj.s.EndReceive(ar);
		if (read > 0) {
			obj.readstream.Write(obj.buffer, 0, read);
			obj.doRecv();
		} else {
			Debug.Log("RecvCB: Disconnect");
			obj.status = DISCONNECT;
			obj.s.Close();
		}
	}

	private static void ConnectCB(IAsyncResult ar) {
		NetSocket obj = (NetSocket) ar.AsyncState;
		obj.status = CONNECTED;
		Debug.Log("Connect:" + obj.s.Connected);
		obj.doRecv();
		return ;
	}

	private void doRecv() {
		s.BeginReceive(buffer, 0, buffer.Length,
			SocketFlags.None,
			new AsyncCallback(RecvCB), this);
		return ;
	}
	private SocketError doSend(byte[] data) {
		SocketError err;
		Debug.Log("doSend:" + data.Length);
		s.BeginSend(data, 0, data.Length,
			SocketFlags.None,
			out err,
			new AsyncCallback(SendCB), this);
		return err;
	}

	public void Connect(string addr, int port) {
		s = new Socket(AddressFamily.InterNetwork,
				SocketType.Stream, ProtocolType.Tcp);
		s.BeginConnect(addr, port,
				new AsyncCallback(ConnectCB), this);
		return ;
	}

	public void Close() {
		s.Close();
		status = DISCONNECT;
	}

	public int Status {
		get {
			return status;
		}
	}

	public SocketError Send(byte[] data) {
		if (sendq.Count != 0) {
			sendq.Enqueue(data);
			return SocketError.Success;
		}
		return doSend(data);
	}

	public int Length {
		get {
			return (int)readstream.Length;
		}
	}

	public int Read(byte[] data, int sz) {
		if (readstream.Length < sz)
			return 0;
		return readstream.Read(data, 0, sz);
	}
}
