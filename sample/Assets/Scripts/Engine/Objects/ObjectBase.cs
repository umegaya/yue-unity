using UnityEngine;
using System.Collections.Generic;
using ScriptEngine;

namespace ScriptEngine {
	public class ObjectTypeBase : Util.FixData {	
		//where to display this object? "user" : "user side", "enemy" : "enemy side"
		public string DisplayPosition { get; set; }
	}
	public class ObjectBase {
		public ObjectTypeBase Type { get; set; }
		public TeamBase Team { get; set; }
		public CellBase Cell { get; set; }
		public ObjectiveBase Objective { get; set; }
		public List<SkillBase> Skills { get; set; }
		public int Hp { get; set; }
		public int x { get; set; }
		public int y { get; set; }
		
		//ctor
		public ObjectBase(ObjectTypeBase t) {
			this.Type = t;
			this.Skills = new List<SkillBase>();
		}
	}
	public class ObjectFactory : Util.Factory<ObjectTypeBase, ObjectBase> {}
}
