using UnityEngine;
using System.Collections.Generic;
using ScriptEngine;
using MiniJSON;
using NLua;
using Yue;

public class GameField {
	//initialize static data
	static object _game_fix_data = null;
	static public void Initialize(Dictionary<string, Dictionary<string, Dictionary<string, object>>> datas) {
		_game_fix_data = Json.Serialize(datas);
		ScriptStarter.InitFixData(datas);
	}
	
	//variables
	bool _debug = false;
	Lua _env = null;		//for local execution
	string _user_id = null;
	float _last_update = 0.0f;
	Actor _actor;		//for remote execution

	//static variables	
	static public float update_latency = 0.0f;
	
	//delegate
	public delegate void ScriptResultDelegate(object[] result, object e);
	void DefaultScriptResultDelegate(object[] result, object e) {
		if (e != null) {
			throw (System.Exception)e;
		}
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
	}
	
	public Lua NewVM(bool debug) {
		var env = new Lua();
		env.LoadCLRPackage(); //enable .net objects
		env.RegisterFunction("print", typeof(GameField).GetMethod("print"));
		env["DEBUG"] = debug; //on/off debug mode
		//add our script search path (as 1st priority)
		env.DoString(string.Format(@"package.path='{0}?.lua;'..package.path", ScriptLoader.SearchPath));
		return env;
	}

	//initialization
	public void InitLocal(object field_data) {
		CleanUp();
		_env = NewVM(_debug);		
		ScriptLoader.Load(_env, "startup.lua");
		Call("InitFixData", null, _game_fix_data);
		Call("Initialize", null, Json.Serialize(field_data));
	}
	public void InitRemote(string url) {
		CleanUp();
		_actor = NetworkManager.instance.NewActor(url);
	}
	
	//method
	public void SendCommand(ScriptResultDelegate d, object command) {
		if (_env != null) {
			Call("SendCommand", d, System.Convert.ToInt32(_user_id), Json.Serialize(command));
		}
		else {
			// TODO : call actor
		}
	}
	
	public void Update(double dt) {
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
	
	public void Enter(object otp, object r, object user_data = null) {
		if (_env != null) {
			Call("Enter", delegate (object []rvs, object err) {
				if (rvs != null) {
					this._user_id = ((double)rvs[0]).ToString();
				}
			}, otp, r, Json.Serialize(user_data));
		}
		else {
			//TODO : register renderer to networkmanager / call actor method Enter
		}
	}
	
	//helper
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
	
	//its just for debugging purpose (for dumping as JSON string). 
	//LuaTable already mostly act like Dictionary, so most of case you don't need this.
	static public Dictionary<object, object> ToDictionary(LuaTable t) {
		var d = new Dictionary<object, object>();
		foreach (KeyValuePair<object, object> e in t) {
			if (e.Value is LuaTable) {
				d[e.Key] = ToDictionary(e.Value as LuaTable);
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
