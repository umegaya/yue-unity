using UnityEngine;
using System.Collections.Generic;
using ScriptEngine;
using MiniJSON;
using NLua;
using Yue;

public class GameField {
	//constants
	//login progress state for remote mode
	const int NOT_ACTIVE = -4;
	const int ON_REQUEST = -3;
	const int CONFIGURE_DATA = -2;
	const int OPEN_FIELD = -1;
	const int REQUEST_OTP = 0;
	const int REQUEST_ENTER = 1;
	const int LOGIN = 2;

	//static variables	
	static public float update_latency = 0.0f;
	static string _game_fix_data = null;
	
	//instance variables common
	bool _debug = false;
	string _user_id = null;
	//instance variables for local mode
	Lua _env = null;			//for local execution
	float _last_update = 0.0f;	//last time when script Update called
	//instance variables for remote mode
	string _rt_url, _web_url;  //generate full field URL with created field id
	string _field_id; 			//field id to enter
	string _otp;				//current otp (one-time-password) for entering field
	Actor _actor, _login_actor;	//for remote execution
	int _login_state = NOT_ACTIVE;	//login progress state
	object _field_data = null;	//if not null, try to create field  
	object _user_data = null; 	//used only when _field_data is not null

	
	//delegate which receive script method's result
	public delegate void ScriptResultDelegate(object[] result, object e);
	void DefaultScriptResultDelegate(object[] result, object e) {
		if (e != null) {
			throw (System.Exception)e;
		}
	}
	
	//static method
	static public void Initialize(Dictionary<string, Dictionary<string, Dictionary<string, object>>> datas) {
		_game_fix_data = Json.Serialize(datas);
	}
	static public Lua NewVM(bool debug) {
		var env = new Lua();
		env.LoadCLRPackage(); //enable .net objects
		env.RegisterFunction("print", typeof(GameField).GetMethod("print"));
		env["DEBUG"] = debug; //on/off debug mode
		//add our script search path (as 1st priority)
		env.DoString(string.Format(@"package.path='{0}?.lua;'..package.path", ScriptLoader.SearchPath));
		return env;
	}
	
	//ctor/dtor
	public GameField(bool debug = false) {
		_debug = debug;
	}
	public void CleanUp() {
		if (_env != null) {
			_env.Dispose();
			_env = null;
		}
		if (_actor != null) {
			_actor.Destroy();
			_actor = null;
		}
		if (_login_actor != null) {
			_login_actor.Destroy();
			_login_actor = null;
		}
	}
	
	//initialization
	public void InitLocal(object field_data) {
		CleanUp();
		_env = NewVM(_debug);		
		ScriptLoader.Load(_env, "startup.lua");
		Call("InitFixData", null, _game_fix_data);
		Call("Initialize", null, Json.Serialize(field_data));
	}
	public void InitRemote_Debug(string rt_url, string web_url, object field_data) {
		CleanUp();
		Debug.Log("InitRemote:" + rt_url + "|" + web_url);
		_user_id = "10000";
		_field_data = field_data;
		_rt_url = rt_url;
		_web_url = web_url;
		_login_actor = NetworkManager.instance.NewActor(web_url);
	}
	public void InitRemote(string rt_url, string web_url, string user_id, string field_id) {
		CleanUp();
		Debug.Log("InitRemote:" + rt_url + "|" + web_url);
		_user_id = user_id;
		_field_id = field_id;
		_rt_url = rt_url;
		_web_url = web_url;
		_login_actor = NetworkManager.instance.NewActor(web_url);
	}
	
	//method
	public void SendCommand(ScriptResultDelegate d, object command) {
		if (_env != null) {
			Call("SendCommand", d, System.Convert.ToInt32(_user_id), Json.Serialize(command));
		}
		else {
			// TODO : call actor
			_actor.Call((resp, err) => {
				if (resp != null) {
					d(resp.ArgsList, null);
				}
				else {
					if (err is System.Exception) {
						d(null, err);
					}
					else {
						d(null, new System.Exception(err.ToString()));
					}
				}
				return null;
			}, "SendCommand", _user_id, Json.Serialize(command));
		}
	}
	
	public void Update(double dt) {
		if (_env != null) {
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
			switch (_login_state) {
			case NOT_ACTIVE:
			case ON_REQUEST:
			case LOGIN:
				return;
			case CONFIGURE_DATA:
				Debug.Log("CONFIGURE_DATA");
				ActorCall(_login_actor, (resp) => {
					_login_state = OPEN_FIELD;
				}, "configure", Json.Deserialize(_game_fix_data));
				break;
			case OPEN_FIELD:
				Debug.Log("OPEN_FIELD");
				ActorCall(_login_actor, (resp) => {
					_field_id = ((string)resp.Args(0));
					_login_state = REQUEST_OTP;
				}, "open_field", _field_data);
				break;
			case REQUEST_OTP:
				Debug.Log("REQUEST_OTP");
				ActorCall(_login_actor, (resp) => {
					_otp = ((string)resp.Args(0));
					_actor = NetworkManager.instance.NewActor(_rt_url + _field_id);
					Debug.Log("generate otp:"+_otp+"@"+_field_id);
					_login_state = REQUEST_ENTER;
				}, "otp", _field_id, _user_id, _user_data);
				break;
			case REQUEST_ENTER:
				Debug.Log("REQUEST_ENTER");
				ActorCall(_actor, (resp) => {
					_login_state = LOGIN;
				}, "login", _otp);
				break;
			}
			_login_state = ON_REQUEST;
		}			
	}
	
	public void Enter(object r, object user_data) {
		if (_env != null) {
			Call("Enter", delegate (object []rvs, object err) {
				if (rvs != null) {
					this._user_id = ((double)rvs[0]).ToString();
				}
			}, "hoge", r, Json.Serialize(user_data));
		}
		else {
			NetworkManager.instance.Register("/player", r);
			_user_data = user_data;
			Debug.Log("_field_data:" + _field_data + "|" + (_field_data != null));
			_login_state = (_field_data != null ? CONFIGURE_DATA : REQUEST_OTP);
		}
	}
	
	//helpers
	void Call(string function, ScriptResultDelegate d, params object[] args) {
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
	delegate void SuccessResponseDelegate(Response r);
	void ActorCall(Actor a, SuccessResponseDelegate d, string func, params object[] args) {
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
	
	static public int TableLength(LuaTable t) {
		if (t[1] == null) {
			return 0;
		}
		var i = 1;
		while (true) {
			i++;
			if (t[i] == null) {
				break;
			}
		}
		return i - 1;
	}
	static public object[] ToList(LuaTable t) {
		var len = TableLength(t);
		var i = 0;
		var l = new object[len];
		while (true) {
			i++;
			var e = t[i];
			if (e == null) {
				break;
			}
			if (i > 10000) {
				Debug.LogError("too much iteration");
				break;
			}
			if (e is LuaTable) {
				var tt = e as LuaTable;
				var lf = tt["__IsList__"];
				if ((lf != null && ((bool)lf) == true) || TableLength(tt) > 0) {
					l[i - 1] = ToList(tt);	
				}
				else {
					l[i - 1] = ToDictionary(tt);
				}
			}
			else {
				l[i - 1] = e;
			}
		}
		return l;
	}
	static public Dictionary<object, object> ToDictionary(LuaTable t) {
		var d = new Dictionary<object, object>();
		foreach (KeyValuePair<object, object> e in t) {
			if (e.Value is LuaTable) {
				var tt = e.Value as LuaTable;
				var lf = tt["__IsList__"];
				if ((lf != null && ((bool)lf) == true) || TableLength(tt) > 0) {
					d[e.Key] = ToList(tt);
				}
				else {
					d[e.Key] = ToDictionary(tt);
				}
			}
			else {
				d[e.Key] = e.Value;
			}
		}
		return d;
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
	public static void print(params object[] args) {
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
