using UnityEngine;
using ScriptEngine;
using NLua;

namespace ScriptEngine {
	class ScriptLoader {
		static public string SearchPath = Application.streamingAssetsPath+"/LuaRoot/";
		static public void Load(Lua env, string file) {
			string code;
#if UNITY_EDITOR
			code = System.IO.File.ReadAllText(SearchPath+file);
#else
			Debug.LogError("TODO: load script from remote server");	
#endif
			env.DoString(code, file);
		}
	}
}
