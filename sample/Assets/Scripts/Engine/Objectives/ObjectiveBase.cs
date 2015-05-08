using UnityEngine;
using System.Collections.Generic;
using ScriptEngine;

namespace ScriptEngine {
	namespace ObjectiveTypes {
		public class Base : Util.FixData {
		}
	}
	namespace Objectives {
		public class Base {
			public ObjectiveTypes.Base Type { get; set; }

			public Base(ObjectiveTypes.Base t) {
				this.Type = t;
			}
		}
		public class Factory : Util.Factory<ObjectiveTypes.Base, Base> {}
	}
}
