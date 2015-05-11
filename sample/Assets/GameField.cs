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

	//initialization
	public void InitLocal(object field_data, object event_player) {
		CleanUp();
		_env = new Lua();
		_env.LoadCLRPackage(); //enable .net objects
		_env.RegisterFunction("print", typeof(GameField).GetMethod("print"));
		_env["DEBUG"] = _debug;
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
		Call("Update", dt);
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
