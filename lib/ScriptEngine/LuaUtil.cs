using UnityEngine;
using NLua;
using System.Collections.Generic;

namespace Yue {
	class LuaUtil {
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
	}
}
