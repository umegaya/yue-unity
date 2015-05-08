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
	bool debug = false;
	Lua env = null;		//for local execution
	ScriptEngine.Fields.Base fld;
	Actor actor;		//for remote execution
	
	//ctor/dtor
	public GameField(bool debug = false) {
		this.debug = debug;
	}
	public void CleanUp() {
		if (env != null) {
			env.Dispose();
			env = null;
		}
		fld = null;
		actor = null;
	}

	//initialization
	public void InitLocal(object field_data, object event_player) {
		CleanUp();
		env = new Lua();
		env.LoadCLRPackage(); //enable .net objects
		env.RegisterFunction("print", typeof(GameField).GetMethod("print"));
		env["DEBUG"] = this.debug;
		fld = new ScriptEngine.Fields.Base();
		
		ScriptLoader.Load(env, "startup.lua");
		Call("Initialize", fld, field_data);
	}
	public void InitRemote(string url, object event_player) {
		CleanUp();
		actor = NetworkManager.instance.NewActor(url);
	}
	
	//method
	public void SendCommand(object command) {
		Call("SendCommand", command);
	}
	
	public void Update(double dt) {
		Call("Update", dt);
	}
	
	public void Enter(object user_data = null) {
		Call("Enter", user_data);
	}
	
	//helper
	object[] Call(string function, params object[] args) {
		object[] result = new object[0];
		if(env == null) return result;
		LuaFunction lf = env.GetFunction(function);
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
		for (int i=0; i < args.Length; i++) {
			text += (args[i].ToString() + "\t");
		}
	    Debug.Log(text);
	}
}
