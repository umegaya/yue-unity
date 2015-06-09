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
			ON_REQUEST = -4,
			NOT_START = -3,
			CONFIGURE_DATA = -2,
			OPEN_FIELD = -1,
			OPENED = 0
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
		public State _boot_state = State.NOT_START;	//login progress state
		string _game_fix_data = null;//fixed game data as json string
		string _field_data = null;	//if not null, try to create field  
	
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
		public bool CreateRemoteField {
			get { return _field_data != null; }
		}
		public Actor LoginActor {
			get { return _login_actor; }
		}
		public string FieldId {
			get { return _field_id; }
		}
		public bool Ready {
			get { return _boot_state == State.OPENED; }
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
			//*/
		}
		public void InitRemoteWithCreateField(string login_url, object game_fix_data, object field_data) {
			CleanUp();
			_field_data = Json.Serialize(field_data);
			_game_fix_data = ToJson(game_fix_data);
			_login_actor = NetworkManager.instance.NewActor(login_url);
			_boot_state = State.CONFIGURE_DATA;
		}
		public void InitRemote(string login_url, string field_id) {
			CleanUp();
			_field_id = field_id;
			_login_actor = NetworkManager.instance.NewActor(login_url);
			_boot_state = State.OPENED;
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
				case State.CONFIGURE_DATA:
					Debug.Log("CONFIGURE_DATA");
					ActorCall(_login_actor, (resp) => {
						_boot_state = State.OPEN_FIELD;
					}, "configure", _game_fix_data);
					break;
				case State.OPEN_FIELD:
					Debug.Log("OPEN_FIELD");
					ActorCall(_login_actor, (resp) => {
						_field_id = ((string)resp.Args(0));
						_boot_state = State.OPENED;
					}, "open_field", _field_data);
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
