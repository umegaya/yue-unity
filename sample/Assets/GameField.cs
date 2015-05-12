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
	ScriptEngine.Fields.Base _field;
	Actor _actor;		//for remote execution
	
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
	public void InitLocal(object field_data, object event_player) {
		CleanUp();
		_env = NewVM(_debug);
		_field = new ScriptEngine.Fields.Base();
		
		ScriptLoader.Load(_env, "startup.lua");
		Call("Initialize", _field, field_data);
	}
	public void InitRemote(string url, object event_player) {
		CleanUp();
		_actor = NetworkManager.instance.NewActor(url);
	}
	
	//method
	public void SendCommand(object command) {
		Call("SendCommand", command);
	}
	
	public void Update(double dt) {
		//var ts = Time.realtimeSinceStartup;
		Call("Update", dt);
		//var et = Time.realtimeSinceStartup;
		//Debug.Log("Update takes:"+ (et - ts) + "|" + ts + "|" + et);
	}
	
	public void Enter(Renderer r, object user_data = null) {
		Call("Enter", r.Id, r, user_data);
	}
	
	//helper
	object[] Call(string function, params object[] args) {
		object[] result = new object[0];
		if(_env == null) return result;
		LuaFunction lf = _env.GetFunction(function);
		if(lf == null) return result;
		try {
			// Note: calling a function that does not 
			// exist does not throw an exception.
			if(args != null) {
				result = lf.Call(args);
			} else {
				result = lf.Call();
			}
		} catch(NLua.Exceptions.LuaException e) {
			Debug.LogError(e);
			throw e;
		}
		return result;		
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
