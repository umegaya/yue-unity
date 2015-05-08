using UnityEngine;
using System.Collections.Generic;
using ScriptEngine;

namespace ScriptEngine {
	namespace EventTypes {
		public class Base : Util.FixData {
		}
	}
	namespace Events {
		public class Base {
			public EventTypes.Base Type { get; set; }
			
			public Base(EventTypes.Base t) {
				this.Type = t;
			}
		}
		public class Factory : Util.Factory<EventTypes.Base, Base> {}
	}
}
