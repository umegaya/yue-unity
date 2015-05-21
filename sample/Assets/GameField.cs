using UnityEngine;
using System.Collections.Generic;
using ScriptEngine;
using NLua;
using Yue;

public class GameField {
	//initialize static data
	static public void Initialize(Dictionary<string, Dictionary<string, Dictionary<string, object>>> datas) {
		ScriptStarter.InitFixData(datas);
	}
	
	//variables
	bool _debug = false;
	Lua _env = null;		//for local execution
	string _user_id = null;
	ScriptEngine.FieldBase _field;
	Actor _actor;		//for remote execution

	//static variables	
	static public float update_latency = 0.0f;

	//static function
	static public int NewLocalUserId() {
		return ObjectBase.NewId();
	}
	
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
		_field = null;
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
		_field = new ScriptEngine.FieldBase();
		
		ScriptLoader.Load(_env, "startup.lua");
		Call("Initialize", null, _field, field_data);
	}
	public void InitRemote(string url) {
		CleanUp();
		_actor = NetworkManager.instance.NewActor(url);
	}
	
	//method
	public void SendCommand(ScriptResultDelegate d, object command) {
		if (_env != null) {
			Call("SendCommand", d, System.Convert.ToInt32(_user_id), command);
		}
		else {
			// TODO : call actor
		}
	}
	
	public void Update(double dt) {
		var now = Time.time;
		if ((now - _field.LastUpdate) > 0.2) {
			var ts = Time.realtimeSinceStartup;
			Call("Update", null, now - _field.LastUpdate);
			var et = Time.realtimeSinceStartup;
			update_latency = (et - ts);
			//Debug.Log("Update takes:"+ (et - ts) + "|" + ts + "|" + et);
			_field.LastUpdate = now;
		}			
	}
	
	public void Enter(object r, object user_data = null) {
		if (_env != null) {
			int id = NewLocalUserId();
			Call("Enter", null, id, r, user_data);
			_user_id = id.ToString();
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
