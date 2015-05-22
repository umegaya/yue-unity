using UnityEngine;
using System.Collections.Generic;
using ScriptEngine;

namespace ScriptEngine {
	public class ObjectTypeBase : Util.FixData {	
		//where to display this object? "user" : "user side", "enemy" : "enemy side"
		public string DisplaySide { get; set; }
	}
	public class ObjectBase {
		public int Id { get; set; }
		public ObjectTypeBase Type { get; set; }
		public Partition Partition { get; set; }
		public TeamBase Team { get; set; }
		public int X { get; set; }
		public int Y { get; set; }
		public bool IsDead { get { return false; } }
		
		//ctor
		public ObjectBase() {
			this.Id = NewId();
		}
				
		//idgen : TODO : move them to common code like Util.ObjectBase and inherit in here.
		static int g_seed = 0;
		static public int NewId() {
			g_seed++;
			if (g_seed > 2000000000) {
				g_seed = 1;
			}
			return g_seed;
		}
	}
	public class ObjectFactory : Util.Factory<ObjectTypeBase, ObjectBase> {}
}
