using UnityEngine;
using System.Collections.Generic;
using System.Reflection;
using NLua;
using ScriptEngine;

namespace ScriptEngine {
	public class ObjectWrapper {
		static LuaFunction wrapper;
		static public object Wrap(object o, string src = null) {
			if (src != null) {
				wrapper.Call(new object[2] {o, src});
			}
			else {
				wrapper.Call(new object[1] {o});				
			}
			return o;
		}
		static public void Initialize(LuaFunction f) {
			wrapper = f;
		}
	}
	namespace Util {
		public class RawFixData<K> {
			public K Id { get; set; }
			public string Name { get; set; }
			public string Class { get; set; }
			public string TypeClass { get; set; }
			public string Script { get ; set; }
			public RawFixData() {}
		}
		public class FixData : RawFixData<string> {}
		public class RawVault<K, T> where T : RawFixData<K>, new() {
			static Dictionary<K, T> map;
			static RawVault() {
				map = new Dictionary<K, T>();
			}
			static public string MakeFullClassName(string class_name) {
				return (string)("ScriptEngine."+class_name);
			}
			//get specified FixData from key k
			static public T Get(K k) {
				T t;
				return map.TryGetValue(k, out t) ? t : null;
			}
			//initialize this vault by given dictionary (typically converted from JSON/msgpack string)
			static public void Initialize(Dictionary<K, Dictionary<string, object>> data) {
				map.Clear();
				foreach (KeyValuePair<K, Dictionary<string, object>> e in data) {
					Put(e.Key, e.Value);
				}
			}
			static void Put(K k, Dictionary<string, object> d) {
				System.Type type = null;
				T t = Get(k);
				bool update = true;
				if (t == null) {
					object typeclass = null;
					if (d.TryGetValue("TypeClass", out typeclass) && typeclass is string) {
						type = System.Type.GetType(MakeFullClassName((string)typeclass));
						if (type == null) {
							Debug.LogError("data error: "+typeclass+" not found");
							return;
						}
					}
					else {
						type = typeof(T);
					}
					if (type == typeof(T) || type.IsSubclassOf(typeof(T))) {
						ConstructorInfo ctor = type.GetConstructor(System.Type.EmptyTypes);
						t = (T)ctor.Invoke(new object[] {});
						update = false;
					}
					else {
						Debug.LogError("data error: "+typeclass+" is not subclass of "+typeof(T));
						return;
					}
				}
				SetData(ref t, type, d);
				t.Id = k;
				if (!update) {
					map.Add(k, t);
				}
			}
			static void SetData(ref T t, System.Type type, Dictionary<string, object> d) {
				foreach (KeyValuePair<string, object> e in d) {
					try {
						object[] args = new object[1] { e.Value };
						type.InvokeMember(e.Key,
						    BindingFlags.Instance | BindingFlags.Public | BindingFlags.SetProperty,
						    System.Type.DefaultBinder, t, args);	

					}
					catch (System.Exception error) {
						Debug.LogError("fail to set property to "+type+":"+error);
					}
				}
			}
		}
		public class Vault<T> : RawVault<string, T> where T : FixData, new() {}
		
		public class RawFactory<K, T, O> : RawVault<K, T> where T : RawFixData<K>, new() {
			static public O Create(K k) {
				T t = Get(k);
				System.Type type;
				if (t != null) {
					if (t.Class == null) {
						type = typeof(O);
					}
					else {
						type = System.Type.GetType(MakeFullClassName(t.Class));
						if (type == null) {
							Debug.LogError("data error: "+t.Class+" not found");
							return default(O);
						}
						if (!type.IsSubclassOf(typeof(O))) {
							Debug.LogError("data error: "+t.Class+" is not subclass of "+typeof(O));
							return default(O);
						}
					}
					ConstructorInfo ctor = type.GetConstructor(new System.Type [] {});
					object o = ctor.Invoke(new object[] {});
					type.InvokeMember("Type",
						    BindingFlags.Instance | BindingFlags.Public | BindingFlags.SetProperty,
						    System.Type.DefaultBinder, o, new object[1] { t });	
					return (O)ObjectWrapper.Wrap(o);
				}
				return default(O);
			}
		}
		public class Factory<T, O> : RawFactory<string, T, O> where T : FixData, new() {}
	}
}
