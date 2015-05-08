using UnityEngine;
using System.Collections.Generic;
using System.Reflection;
using ScriptEngine;

namespace ScriptEngine {
	namespace Util {
		public class RawFixData<K> {
			public K Id { get; set; }
			public string Name { get; set; }
			public string Class { get; set; }
			public string TypeClass { get; set; }
			public RawFixData() {}
		}
		public class FixData : RawFixData<string> {}
		public class RawVault<K, T> where T : RawFixData<K>, new() {
			static Dictionary<K, T> map;
			static RawVault() {
				map = new Dictionary<K, T>();
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
				T t = Get(k);
				bool update = true;
				if (t == null) {
					System.Type type;
					object typeclass;
					if (d.TryGetValue("TypeClass", out typeclass) && typeclass is string) {
						type = System.Type.GetType((string)typeclass);				
					}
					else {
						type = typeof(T);
					}
					if (type.IsSubclassOf(typeof(T))) {
						ConstructorInfo ctor = type.GetConstructor(System.Type.EmptyTypes);
						t = (T)ctor.Invoke(new object[] {});
						update = false;
					}
					else {
						Debug.LogError("data error: "+typeclass+" is not subclass of "+typeof(T));
						return;
					}
				}
				SetData(ref t, d);
				t.Id = k;
				if (!update) {
					map.Add(k, t);
				}
			}
			static void SetData(ref T t, Dictionary<string, object> d) {
				System.Type type = typeof(T);
				foreach (KeyValuePair<string, object> e in d) {
					try {
						object[] args = new object[1] { e.Value };
						type.InvokeMember(e.Key,
						    BindingFlags.Instance | BindingFlags.Public | BindingFlags.SetProperty,
						    System.Type.DefaultBinder, t, args);	
					}
					catch (System.Exception error) {
						Debug.LogError("fail to set property:"+error);
					}
				}
			}
		}
		public class Vault<T> : RawVault<string, T> where T : FixData, new() {}
		
		public class RawFactory<K, T, O> : RawVault<K, T> where T : RawFixData<K>, new() {
			static public O Create(K k) {
				T t = Get(k);
				if (t != null) {
					System.Type type = System.Type.GetType(t.Class);
					if (type.IsSubclassOf(typeof(O))) {
						ConstructorInfo ctor = type.GetConstructor(new[] { typeof(T) });
						return (O)ctor.Invoke(new object[] { t });
					}
					Debug.LogError("data error: "+t.Class+" is not subclass of "+typeof(O));
				}
				return default(O);
			}
		}
		public class Factory<T, O> : RawFactory<string, T, O> where T : FixData, new() {}
	}
}
