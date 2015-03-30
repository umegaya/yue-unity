using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Reflection;
using MsgPack;
using Serde = MsgPack.BoxingPacker;
using UnityEngine;

namespace Yue
{
	//sub class
	class YieldContext {
		public ResponseDelegate d;
		public float timeout_at;
	};
	public class CallAttr {
		public bool notify;
		public bool async;
		public float timeout;
	};
	public class Response {
		object _obj;
		public Response(object o) {
			_obj = o;
			/*int count = 0;
			foreach (var ob in Data) {
				Debug.Log("Data:"+count+"|"+ob);
				count++;
			}*/
		}
		public object[] Data {
			get {
				return ((object[])_obj);
			}
		}
		public uint Kind {
			get {
				//Debug.Log("Kind:" + ((object[])_obj)[0].GetType());
				return (uint)(double)Data[0];
			}
		}
		public bool ServerCall {
			get {
				return (Kind == Connection.KIND_CALL) || (Kind == Connection.KIND_SEND);
			}
		}
		public bool ServerResponse {
			get {
				return Kind == Connection.KIND_RESPONSE;
			}
		}
		public bool ServerNotify {
			get {
				return (Kind & 4) != 0;
			}
		}
		public string Method {
			get {
				return (string)Data[3];
			}
		}
		public ServerException Error(Serde sr) {
			if (Success) {
				return null;
			}
			var ext = Args<Ext>(0);
			var m = new MemoryStream(ext.Data);
			//run iterator and skip first yield return
			var it = sr.Unpack(m); it.MoveNext(); it.MoveNext();
			string name = (string)it.Current; it.MoveNext();
			string bt = (string)it.Current; it.MoveNext();
			object[] args = (object[])it.Current;
			return new ServerException(name, bt, args);
		}
		public string UUID {
			get {
				return (string)Data[1];
			}
		}
		public uint Msgid {
			get {
				return (uint)(double)Data[MsgidIndex];
			}
		}
		int ArgsIndex {
			get {
				//ServerResponse has result(boolean) as its first argument. so skip it.
				return ServerResponse ? 3 : (ServerNotify ? 3 : (ServerCall ? 5 : -1));
			}
		}
		int MsgidIndex {
			get {
				return ServerResponse ? 1 : (ServerCall ? 2 : -1);
			}			
		}
		public bool Success {
			get {
				//see ArgsIndex's comment. 
				return Convert.ToBoolean(Data[ArgsIndex - 1]);
			}
		}
		public object Args(uint idx) {
			//skip first args (is result)
			return Data[idx + ArgsIndex];
		}
		public T Args<T>(uint idx) {
			return (T)Data[idx + ArgsIndex];
		}
		public object[] ArgsList {
			get {
				var sz = Data.Length - ArgsIndex;
				if (sz > 0) {
					object[] list = new object[sz];
					for (uint i = 0; i < sz; i++) {
						list[i] = Args(i);
					}
					return list;
				}
				return null;
			}
		}
	};
	public delegate object ResponseDelegate(Response resp, Exception e);

	// exceptions;
	public class RPCTimeoutException : Exception {
		public float timeout_at;
		public RPCTimeoutException(float at) {
			timeout_at = at;
		}
	};
	public class ServerException : Exception {
		public string _name;
		public string _bt;
		public object[] _args;
		public ServerException(string name, string bt, object[] args) {
			_name = name;
			_bt = bt;
			_args = args;			
		}
		public string Message {
			get {
				return _name + "\n" + _bt;
			}
		}
	};

	//connection
	public class Connection {
		//constant
		public const byte KIND_CALL = 1;
		public const byte KIND_SEND = 2;
		public const byte KIND_RESPONSE = 3;
		public const byte KIND_NOTIFY_CALL = 5;
		public const byte KIND_NOTIFY_SEND = 6;

		//static methods
		static public Connection New(string url) {
			return new Connection(url);
		}

		//instance vars
		string _url;
		Serde _serde;
		Socket _sock;
		NetworkStream _stream;
		MemoryStream _sendbuf;
		IEnumerator _reader;

		//instance methods
		public Connection(string url) {
			_url = url;
			_sock = this.NewSocket(url);
			_serde = this.NewSerde();
			_stream = new NetworkStream(_sock);
			_stream.ReadTimeout = 10;
			_sendbuf = new MemoryStream();
			_reader = _serde.Unpack(_stream);
		}
		static object[] Merge(object[] dst, object[] src) {
			int i = dst.Length;
			Array.Resize<object> (ref dst, (int)(dst.Length + src.Length));
			foreach (var o in src) {
				dst[i++] = o;
			}
			return dst;
		}
		public void Send(Actor a, CallAttr attr, string method, object[] args, ResponseDelegate d) {
			var msgid = TransportManager.NewMsgId();
			Dispatch(Merge(new object[5] {
				KIND_SEND,
				a.Id, 
				msgid,
				method,
				null
			}, args), attr, msgid, d);
		}
		public void Notify(Actor a, CallAttr attr, string method, object[] args) {
			WriteStream(Merge(new object[3] {
				KIND_NOTIFY_SEND,
				a.Id, 
				method
			}, args));
		}
		public Socket Socket {
			get {
				return _sock;
			}
		}
		public string URL {
			get {
				return _url;
			}
		}
		public void Close() {
			TransportManager.Remove(this);
			_sock.Close(1);
			_stream.Close();
		}
		public object ReadStream() {
			_reader.MoveNext();
			//Debug.Log("ReadStream:" + _reader.Current);
			if (_reader.Current.GetType() == typeof(MsgPack.BuffShortException)) {
				throw (MsgPack.BuffShortException)_reader.Current;
			}
			return _reader.Current;
		}
		public void WriteStream(object o) {
			_serde.Pack(_sendbuf, o);
			_sendbuf.Seek(0, SeekOrigin.Begin);
			_sendbuf.CopyTo(_stream);
			//	DumpWriteStream();
			// because if there is remained buffer size ( == sz), 
			// it is not sure that SetLength(sz) keep remained buffer, 
			// only when buffer is fully written to network, we reduce length.
			if (_sendbuf.Position == _sendbuf.Length) {
				_sendbuf.SetLength(0);
			}
		}

		void DumpWriteStream() {
			var buf = _sendbuf.GetBuffer();
			string dump = "";
			for (int i = 0; i < _sendbuf.Length; i++) {
				dump += String.Format(":{0:X}", buf[i]);
			}
			Debug.Log("buf:"+_sendbuf.Length+dump);
		}
		void Dispatch(object[] req, CallAttr attr, uint msgid, ResponseDelegate d) {
			WriteStream(req);
			TransportManager.Yield(msgid, attr, d);
		}
		Serde NewSerde() {
			return new Serde();
		}
		Socket NewSocket(string url) {
			var m = Regex.Match(url, @"(.+?)://([^/:]+):?([0-9]*)");
			if (m.Success) {
				var proto = m.Groups[1].Value;
				var hostname = m.Groups[2].Value;
				var port = m.Groups[3].Value;
				Socket s;
				switch (proto) {
				case "tcp":
					s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
					break;
				case "udp":
					s = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
					break;
				default:
					throw new FormatException();
				}
				s.Connect(hostname, int.Parse(port));
				Debug.Log("Connect To:" + proto + "|" + hostname + "|" + port);
				return s;
			}
			else {
				throw new FormatException();
			}
		}
	}
	public class TransportManager {
		static uint seed = 0;
		static Dictionary<string, Connection> connections = new Dictionary<string, Connection>();
		static Dictionary<uint, YieldContext> dispatchers = new Dictionary<uint, YieldContext>();
		static Dictionary<string, object> delegates = new Dictionary<string, object>();
		static List<Socket> sockets = new List<Socket>();
		static Serde serde = new Serde();
		static public Connection Get(string url) {
			Connection c;
			if (connections.TryGetValue(url, out c)) {
				return c;
			}
			c = Connection.New(url);
			connections[url] = c;
			return c;
		}
		static public uint NewMsgId() {
			seed++;
			return seed;
		}
		static public void Remove(Connection c) {
			connections.Remove(c.URL);
		}
		static public void Yield(uint msgid, CallAttr attr, ResponseDelegate d) {
			dispatchers[msgid] = new YieldContext { d = d, timeout_at = Time.time + attr.timeout };
		}
		static public void Register(string name, object o) {
			delegates[name] = o;
		}
		static public object CallMethodOnTheFly(object o, string method, object[] args) {
			Type t = o.GetType();
			MethodInfo m = t.GetMethod(method);
			if (m != null) {
				return m.Invoke(o, args);
			}
			return null;
		}
		static public void Poll() {
			sockets.Clear();
			foreach (KeyValuePair<string, Connection> pair in connections) {
				sockets.Add(pair.Value.Socket);
			}
			if (sockets.Count > 0) {
				Socket.Select(sockets, null, null, 1000);
				foreach (Socket s in sockets) {
					foreach (KeyValuePair<string, Connection> pair in connections) {
						var c = pair.Value;
						if (s == c.Socket) {
							try {
								YieldContext ctx;
								var obj = c.ReadStream();
								var resp = new Response(obj);
								if (resp.ServerResponse) {
									if (dispatchers.TryGetValue(resp.Msgid, out ctx)) {
										dispatchers.Remove(resp.Msgid);
										var err = resp.Error(serde);
										if (err != null) {
											ctx.d(null, err);
										}
										else {
											ctx.d(resp, null);
										}
									}
								}
								else if (resp.ServerCall) {
									// TODO : enable to register delegate, and dispatch call
									object o;
									try {
										if (delegates.TryGetValue(resp.UUID, out o)) {
											var body = CallMethodOnTheFly(o, resp.Method, resp.ArgsList);
											if (!resp.ServerNotify) {
												c.WriteStream(MakeResponse(resp, body, null));
											}
										}
									}
									catch (Exception e) {
										if (!resp.ServerNotify) {
											c.WriteStream(MakeResponse(resp, null, e.Message + " " + e.StackTrace));
										}										
									}
								}
							}
							catch (MsgPack.BuffShortException e) {
								//Debug.Log(e);
							}
							catch (Exception e) {
								Debug.Log("unexpected error:"+e);
							}
						}
					}
				}
			}
			var now = Time.time;
			foreach (KeyValuePair<uint, YieldContext> pair in dispatchers) {
				var ctx = pair.Value;
				if (ctx.timeout_at < now) {
					dispatchers.Remove(pair.Key);
					ctx.d(null, new RPCTimeoutException(ctx.timeout_at));
				}
			}
		}

		static object[] MakeResponse(Response resp, object body, string error) {
			return new object[] {
				Connection.KIND_RESPONSE,
				resp.Msgid,
				error == null,
				error == null ? body : error
			};
		}
	}
	public class Actor {
		static CallAttr attr = new CallAttr();
		public Actor(string url) {
			Parse(url);
		}
		public string Id { get; set; }
		public string Host { get; set; }
		public string Proto { get; set; }
		public void Call(ResponseDelegate d, string method, params object[] args) {
			var c = TransportManager.Get(Proto+"://"+Host);
			string real_method = ParseMethodName(method, args, ref attr);
			if (attr.notify) {
				c.Notify(this, attr, real_method, args);
				return;
			}
			if (attr.async) {
				//TODO: implement async
			}
			c.Send(this, attr, real_method, args, d);
		}
		string ParseMethodName(string method, object[] args, ref CallAttr attr) {
			attr.notify = false;
			attr.async = false;
			attr.timeout = 5000;
			return method;
		}
		void Parse(string url) {
			var m = Regex.Match(url, @"([^:/]+?)://([^/]+)(/.*)");
			if (m.Success) {
				Proto = m.Groups[1].Value;
				Host = m.Groups[2].Value;
				Id = m.Groups[3].Value;
			}
			else {
				throw new FormatException();
			}
		}
	}
}
