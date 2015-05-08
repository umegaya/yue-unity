using UnityEngine;
using System.Collections.Generic;
using ScriptEngine;

namespace ScriptEngine {
	public class ObjectiveTypeBase : Util.FixData {
	}
	public class ObjectiveBase {
		public ObjectiveTypeBase Type { get; set; }

		public ObjectiveBase(ObjectiveTypeBase t) {
			this.Type = t;
		}
	}
	public class ObjectiveFactory : Util.Factory<ObjectiveTypeBase, ObjectiveBase> {}
}
