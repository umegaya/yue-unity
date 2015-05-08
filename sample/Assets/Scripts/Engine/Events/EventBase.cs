using UnityEngine;
using System.Collections.Generic;
using ScriptEngine;

namespace ScriptEngine {
	public class EventTypeBase : Util.FixData {
	}
	public class EventBase {
		public EventTypeBase Type { get; set; }
		
		public EventBase(EventTypeBase t) {
			this.Type = t;
		}
	}
	public class EventFactory : Util.Factory<EventTypeBase, EventBase> {}
}
