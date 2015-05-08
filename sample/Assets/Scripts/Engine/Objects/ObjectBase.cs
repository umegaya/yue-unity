using UnityEngine;
using System.Collections.Generic;
using ScriptEngine;

namespace ScriptEngine {
	namespace ObjectTypes {
		public class Base : Util.FixData {	
			//where to display this object? "user" : "user side", "enemy" : "enemy side"
			public string DisplayPosition { get; set; }
		}
	}
	namespace Objects {
		public class Base {
			public ObjectTypes.Base Type { get; set; }
			public Teams.Base Team { get; set; }
			public Cells.Base Cell { get; set; }
			public Objectives.Base Objective { get; set; }
			public List<Skills.Base> Skills { get; set; }
			public int Hp { get; set; }
			public int x { get; set; }
			public int y { get; set; }
			
			//ctor
			public Base(ObjectTypes.Base t) {//, string team_id, string objective_id, List<string> skill_ids) {
				this.Type = t;
				this.Skills = new List<Skills.Base>();
			}
		}
		public class Factory : Util.Factory<ObjectTypes.Base, Base> {}
	}
}
