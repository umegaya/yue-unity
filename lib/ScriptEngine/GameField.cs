using UnityEngine;
using System.Collections.Generic;
using MiniJSON;
using NLua;
using Yue;

namespace Yue {
	public class GameField : MonoBehaviour {
		//constants
		//login progress state for remote mode
		public enum State {
			ON_REQUEST = -2,
			NOT_START = -1,
			OPENED = 0,

			REMOTE_CONFIGURE_DATA = -10,
			REMOTE_OPEN_FIELD = -11,

			MATCHING_CONFIGURE_DATA = -20,
		};
		public enum Method {
			LOCAL = 1,
			MATCHING = 2,
			REMOTE = 3,
		};

		//static variables	
		static public float update_latency = 0.0f;
		
		//instance variables
		//for local mode
		Lua _env = null;			//for local execution
		float _last_update = 0.0f;	//last time when script Update called
		//for remote mode
		string _field_id = null; 	//field id to enter
		Actor _login_actor = null;	//for remote execution
		string _game_fix_data = null;//fixed game data as json string
		string _field_data = null;	//if not null, try to create field  
		//for matching mode
		string _queue_name = null; //queue name to enter
		Dictionary<string, object> _queue_settings = null; //if queue is not created yet, queue will be created with this setting.

		public State _boot_state = State.NOT_START;	//login progress state
		Method _login_method = Method.LOCAL;	//login method for session
	
		//delegate and its default
		public delegate void SuccessResponseDelegate(Response r);
		public delegate void ScriptResultDelegate(object[] result, object e);
		void DefaultScriptResultDelegate(object[] result, object e) {
			if (e != null) {
				throw (System.Exception)e;
			}
		}
		
		//static method
		static public Lua NewVM(bool debug_flag) {
			var env = new Lua();
			env.LoadCLRPackage(); //enable .net objects
			env.RegisterFunction("print", typeof(GameField).GetMethod("print"));
			env["DEBUG"] = debug_flag; //on/off debug mode
			env["__SearchPath"] = ScriptLoader.SearchPath; //additional script search path
			return env;
		}
		
		//ctor
		void Start() {
		}
		
		//attribute
		public bool LocalMode { 
			get { return _env != null; }
		}
		public bool UseDummyWebsv {
			get { return _field_data != null; }
		}
		public Actor LoginActor {
			get { return _login_actor; }
		}
		public string FieldId {
			get { return _field_id; }
			set { _field_id = value; }
		}
		public bool Ready {
			get { return _boot_state == State.OPENED; }
		}
		public Method LoginMethod {
			get { return _login_method; }
		}
		public string QueueName {
			get { return _queue_name; }
		}
		public Dictionary<string, object> QueueSettings {
			get { return _queue_settings; }
		}
		
		//initialization
		public void InitLocal(object game_fix_data, object field_data, bool debug) {
			CleanUp();
			_env = NewVM(debug);
			ScriptLoader.Load(_env, "startup.lua");
			_game_fix_data = ToJson(game_fix_data);
			Call("InitFixData", null, _game_fix_data);
			Call("Initialize", null, Json.Serialize(field_data));
			_boot_state = State.OPENED;
			_login_method = Method.LOCAL;
			//*/
		}
		public void InitRemote(string login_url, string field_id) {
			CleanUp();
			_field_id = field_id;
			_login_actor = NetworkManager.instance.NewActor(login_url);
			_boot_state = State.OPENED;
			_login_method = Method.REMOTE;
		}
		public void InitRemoteWithCreateField(string login_url, object game_fix_data, object field_data) {
			CleanUp();
			_field_data = ToJson(field_data);
			_game_fix_data = ToJson(game_fix_data);
			_login_actor = NetworkManager.instance.NewActor(login_url);
			_boot_state = State.REMOTE_CONFIGURE_DATA;
			_login_method = Method.REMOTE;
		}
		public void InitMatching(string login_url, string queue_name, int group_size, object field_data) {
			InitRemote(login_url, null);
			_login_method = Method.MATCHING;
			_queue_name = queue_name;
			_queue_settings = new Dictionary<string, object> {
				{ "field_data", field_data },
				{ "group_size", group_size }
			};
		}
		public void InitMatchingWithConfigure(string login_url, string queue_name, int group_size, object game_fix_data, object field_data) {
			InitRemoteWithCreateField(login_url, game_fix_data, field_data);
			_login_method = Method.MATCHING;
			_boot_state = State.MATCHING_CONFIGURE_DATA;
			_queue_name = queue_name;
			_queue_settings = new Dictionary<string, object> {
				{ "field_data", field_data },
				{ "group_size", group_size }
			};
		}
		void CleanUp() {
			if (_env != null) {
				_env.Dispose();
				_env = null;
			}
			if (_login_actor != null) {
				_login_actor.Destroy();
				_login_actor = null;
			}
		}
		string ToJson(object data) {
			if (data.GetType() == typeof(string)) {
				return (string)data;
			}
			else {
				return Json.Serialize(data);
			}
		}
		
		void Update() {
			if (this.LocalMode) {
				var now = Time.time;
				if ((now - _last_update) > 0.1) {
					var ts = Time.realtimeSinceStartup;
					Call("Update", null, now - _last_update);
					var et = Time.realtimeSinceStartup;
					update_latency = (et - ts);
					//Debug.Log("Update takes:"+ (et - ts) + "|" + ts + "|" + et);
					_last_update = now;
				}
			}
			else {
				switch (_boot_state) {
				case State.NOT_START:
				case State.ON_REQUEST:
				case State.OPENED:
					return;
				//for remote field creation (debug)
				case State.REMOTE_CONFIGURE_DATA:
					Debug.Log("REMOTE_CONFIGURE_DATA");
					ActorCall(_login_actor, (resp) => {
						_boot_state = State.REMOTE_OPEN_FIELD;
					}, "configure", Json.Deserialize(_game_fix_data));
					break;
				case State.REMOTE_OPEN_FIELD:
					Debug.Log("REMOTE_OPEN_FIELD");
					ActorCall(_login_actor, (resp) => {
						_field_id = ((string)resp.Args(0));
						_boot_state = State.OPENED;
					}, "open_field", 5, Json.Deserialize(_field_data));
					break;
				//for matching field creation (debug)
				case State.MATCHING_CONFIGURE_DATA:
					Debug.Log("MATCHING_CONFIGURE_DATA");
					ActorCall(_login_actor, (resp) => {
						_boot_state = State.OPENED;
					}, "configure", Json.Deserialize(_game_fix_data));
					break;
				}
				_boot_state = State.ON_REQUEST;
			}			
		}
			
		//helpers
		public void Call(string function, ScriptResultDelegate d, params object[] args) {
			if (d == null) {
				d = DefaultScriptResultDelegate;
			}
			object[] result = new object[0];
			if(_env == null) {
				d(null, new System.Exception("script VM not initialized"));
				return;
			}
			LuaFunction lf = _env.GetFunction(function);
			if(lf == null) {
				d(null, new System.Exception("function not found:"+function));
				return;
			}
			try {
				// Note: calling a function that does not 
				// exist does not throw an exception.
				if(args != null) {
					result = lf.Call(args);
				} else {
					result = lf.Call();
				}
			} catch(NLua.Exceptions.LuaException e) {
				d(null, e);
			}
			d(result, null);
		}
		public void ActorCall(Actor a, SuccessResponseDelegate d, string func, params object[] args) {
			a.Call((resp, err) => {
				if (resp != null) {
					try {
						d(resp);
					}
					catch (System.Exception e) {
						Debug.Log("response delegate fails:" + e);	
					}
				}
				else if (err is ServerException) {
					Debug.Log((err as ServerException).Message);
				}
				else {
					Debug.Log("other exception:" + err);
				}
				return null;
			}, func, args);
		}
			
		static string DumpTrace(Lua st, int upto) {
			string ret = "";
			for (int i = upto; i >= 0; i--) {
				KeraLua.LuaDebug d = new KeraLua.LuaDebug();
				if (st.GetStack(i, ref d) == 1) {
					ret += (d.source + ":" + d.currentline);
				}
			}
			return ret;
		}
		public static void print(params object[] args) { //public is required to call from VM
			string text = "";
			if (args != null) {
				for (int i=0; i < args.Length; i++) {
					if (args[i] != null) {
						text += (args[i].ToString() + "\t");
					}
					else {
						text += "null\t";
					}
				}
			}
		    Debug.Log(text);
		}	
	}
}
