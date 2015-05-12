using UnityEngine;
using System.Collections.Generic;
using ScriptEngine;

namespace ScriptEngine {
	public class ObjectiveTypeBase : Util.FixData {
		public string AssignedTeam { get; set; }
		public string Group { get; set; }
	}
	public class ObjectiveBase {
		public ObjectiveTypeBase Type { get; set; }
		public ObjectiveBase() {
			
		}
	}
	public class ObjectiveFactory : Util.Factory<ObjectiveTypeBase, ObjectiveBase> {}
}
