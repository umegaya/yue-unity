using System;
using System.IO;
using System.Net;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Reflection;
using MsgPack;
using MiniJSON;
using Serde = MsgPack.BoxingPacker;
using UnityEngine;

namespace Yue
{
	//sub class
	class YieldContext {
		public ResponseDelegate d;
		public float timeout_at;
	};
	class WebYieldContext : YieldContext {
		Curl _www;
		IEnumerator _reader;
		public WebYieldContext(CallAttr attr, ResponseDelegate d, string url, 
			object[] args, Dictionary<string, object> headers) {
			var str = Json.Serialize(args);
			byte[] body = System.Text.Encoding.ASCII.GetBytes(str);
			this.d = d;
			this.timeout_at = Time.time + attr.timeout;
			//Debug.Log("WWW parm:" + url + "|" + str + "|" + headers);
			_www = new Curl(url, body, headers);
			_reader = Start();
		}
		IEnumerator Start() {
			while (!_www.isDone) {
				yield return _www;
			}
			if (!string.IsNullOrEmpty(_www.error)) {
				Debug.Log("http error:" + _www.error);
				throw new HttpRequestException(_www.error);
			}
		}
		public object ReadStream() {
			if (_reader.MoveNext()) {
				return null;
			}
			var bs = System.Text.Encoding.ASCII.GetString(_www.bytes);
			if (bs == null) {
				Debug.Log("get ascii string fails");
			}
			Debug.Log("response:" + bs);
			return Json.Deserialize(bs);
		}
	}
	public class CallAttr {
		public bool notify;
		public bool async;
		public float timeout;
	};
	public class Response {
		protected object _obj;
		public Response() {}
		public Response(object o) {
			_obj = o;
			/*
			int count = 0;
			foreach (var ob in Data) {
				Debug.Log("Data:"+count+"|"+ob);
				count++;
			}//*/
		}
		public object[] Data {
			get {
				return ((object[])_obj);
			}
		}
		public virtual uint Kind {
			get {
				//Debug.Log("Kind:" + ((object[])_obj)[0].GetType());
				return (uint)(double)Data[0];
			}
		}
		public bool ServerCall {
			get {
				var k = (Kind & 3);
				return (k == Connection.KIND_CALL) || (k == Connection.KIND_SEND);
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
				return (string)Data[MethodIndex];
			}
		}
		public virtual ServerException Error(Serde sr) {
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
		int MethodIndex {
			get {
				return ServerNotify ? 2 : 3;
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
	public class WebResponse : Response {
		public WebResponse(object o) : base() {
			if (o.GetType().GetGenericTypeDefinition() == typeof(List<>)) {
				var l = o as List<object>;
				_obj = l.ToArray();
			}
		}
		public override uint Kind {
			get {
				return (uint)(System.Int64)Data[0];
			}
		}
		public override ServerException Error(Serde sr) {
			if (Success) {
				return null;
			}
			var err = Args<Dictionary<string, object>>(0);
			object name = err.TryGetValue("name", out name) ? name : "";
			return new ServerException((string)name, (string)err["bt"], (List<object>)err["args"]);
		}		
	}
	public delegate object ResponseDelegate(Response resp, Exception e);
	public delegate void ConnectionStateDelegate(string url, bool opened); //true if connection opened, otherwise closed.

	// exceptions;
	public class RPCTimeoutException : Exception {
		public float timeout_at;
		public RPCTimeoutException(float at) {
			timeout_at = at;
		}
	};
	public class ServerException : Exception {
		string _name;
		string _bt;
		object[] _args;
		public ServerException(string name, string bt, List<object> args) {
			_name = string.IsNullOrEmpty(name) && (args[0] is string) ? ((string)args[0]) : name;
			_bt = bt;
			_args = args.ToArray();			
		}
		public ServerException(string name, string bt, object[] args) {
			_name = name;
			_bt = bt;
			_args = args;			
		}
		public override string Message {
			get {
				return _name + _bt + (_args.Length > 0 ? "\n" + _args[0] : "");
			}
		}
	};
	public class HttpRequestException : Exception {
		string _error;
		public HttpRequestException(string err) {
			_error = err;
		}
		public override string Message {
			get {
				return _error;
			}
		}
	}

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
		static List<Connection> closed = new List<Connection>();
		static Dictionary<uint, YieldContext> dispatchers = new Dictionary<uint, YieldContext>();
		static List<WebYieldContext> web_dispatchers = new List<WebYieldContext>();
		static List<WebYieldContext> web_closed = new List<WebYieldContext>();
		static Dictionary<string, object> delegates = new Dictionary<string, object>();
		static Dictionary<string, ConnectionStateDelegate> watchers = new Dictionary<string, ConnectionStateDelegate>();
		static List<Socket> sockets = new List<Socket>();
		static Serde serde = new Serde();
		static public Connection Get(string url) {
			Connection c;
			if (connections.TryGetValue(url, out c)) {
				return c;
			}
			c = Connection.New(url);
			connections[url] = c;
			PublishConnectionState(url, true);
			return c;
		}
		static public void AddWatcher(string url, ConnectionStateDelegate d) {
			ConnectionStateDelegate tmp;
			if (watchers.TryGetValue(url, out tmp)) {
				tmp += d;
			}
			else {
				watchers.Add(url, d);
			}
		}
		static public void RemoveWatcher(string url, ConnectionStateDelegate d) {
			ConnectionStateDelegate tmp;
			if (watchers.TryGetValue(url, out tmp)) {
				tmp -= d;
			}
		}
		static public void DispatchViaHttp(CallAttr attr, ResponseDelegate d, string url, 
			object[] args = null, Dictionary<string, object> headers = null) {
			var ctx = new WebYieldContext(attr, d, url, args, headers);
			web_dispatchers.Add(ctx);
		}
		static public uint NewMsgId() {
			seed++;
			return seed;
		}
		static public void Remove(Connection c) {
			if (connections.ContainsKey(c.URL)) {
				connections.Remove(c.URL);
				PublishConnectionState(c.URL, false);
			}
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
									object o;
									try {
										if (delegates.TryGetValue(resp.UUID, out o)) {
											var body = CallMethodOnTheFly(o, resp.Method, resp.ArgsList);
											if (!resp.ServerNotify) {
												c.WriteStream(MakeResponse(resp, body, null));
											}
										}
										else {
											Debug.Log("UUID not found:" + resp.UUID);
										}
									}
									catch (Exception e) {
										if (!resp.ServerNotify) {
											c.WriteStream(MakeResponse(resp, null, e.Message + " " + e.StackTrace));
										}										
									}
								}
							}
							catch (MsgPack.BuffShortException) {
							}
							catch (IOException e) {
								Debug.Log("socket may be closed:"+e);
								closed.Add(c);
							}
							catch (Exception e) {
								Debug.Log("unexpected error:"+e);
							}
						}
					}
				}
			}
			//remove closed connection so that next RPC with same url can create new connection
			if (closed.Count > 0) {
				foreach (Connection c in closed) {
					c.Close();
				}
				closed.Clear();
			}
			//check timeout and remove too old RPC entry
			var now = Time.time;
			foreach (KeyValuePair<uint, YieldContext> pair in dispatchers) {
				var ctx = pair.Value;
				if (ctx.timeout_at < now) {
					dispatchers.Remove(pair.Key);
					ctx.d(null, new RPCTimeoutException(ctx.timeout_at));
				}
			}
			//web request polling
			foreach (WebYieldContext ctx in web_dispatchers) {
				if (ctx.timeout_at < now) {
					web_dispatchers.Remove(ctx);
					ctx.d(null, new RPCTimeoutException(ctx.timeout_at));
					web_closed.Add(ctx);
				}
				else {
					try {
						var obj = ctx.ReadStream();
						if (obj != null) {
							var r = new WebResponse(obj);
							var err = r.Error(serde);
							if (err != null) {
								ctx.d(null, err);
							}
							else {
								ctx.d(r, null);
							}
							web_closed.Add(ctx);
						}
						//obj == null is not error. it shows request not finished yet.
					}
					catch (Exception e) {
						ctx.d(null, e);
						web_closed.Add(ctx);
					}
				}
			}
			//delete closing web requests.
			if (web_closed.Count > 0) {
				foreach (WebYieldContext c in web_closed) {
					web_dispatchers.Remove(c);
				}
				web_closed.Clear();				
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
		static void PublishConnectionState(string url, bool opened) {
			ConnectionStateDelegate tmp;
			if (watchers.TryGetValue(url, out tmp)) {
				tmp(url, opened);
			}			
		}
	}
	public class Actor {
		static protected CallAttr attr = new CallAttr();
		public ConnectionStateDelegate _watcher = null;
		public void DefaultConnectionWatcher(string url, bool opened) {
			if (opened) {
				Debug.Log("connection open:" + url);
			}
			else {
				Debug.Log("connection close:" + url);			
			}			
		}
		public Actor() {}
		public Actor(string url, ConnectionStateDelegate d = null) {
			Parse(url);
			if (d == null) {
				d = DefaultConnectionWatcher;
			}
			_watcher = d;
			TransportManager.AddWatcher(URL, _watcher);
		}
		public void Destroy() {
			if (_watcher != null) {
				TransportManager.RemoveWatcher(URL, _watcher);
			}	
		}
		public string Id { get; set; }
		public string Host { get; set; }
		public string Proto { get; set; }
		public string URL { get { return Proto+"://"+Host; } }
		public virtual void Call(ResponseDelegate d, string method, params object[] args) {
			try {
				var c = TransportManager.Get(URL);
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
			catch (Exception e) {
				d(null, e);
			}
		}
		protected string ParseMethodName(string method, object[] args, ref CallAttr attr) {
			attr.notify = false;
			attr.async = false;
			attr.timeout = 5000;
			return method;
		}
		protected void Parse(string url) {
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
	public class WebActor : Actor {
		public WebActor(string url) : base() {
			Parse(url);			
		}
		public override void Call(ResponseDelegate d, string method, params object[] args) {
			try {
				string real_method = ParseMethodName(method, args, ref attr);
				TransportManager.DispatchViaHttp(attr, d, URL + Id + "/" + real_method, args);
			}
			catch (Exception e) {
				d(null, e);
			}
		}
	}
}
