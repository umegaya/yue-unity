using UnityEngine;
using System.Collections.Generic;
using ScriptEngine;

namespace ScriptEngine {
	public class GameUser : User {
		public ObjectiveBase Objective { get; set; }
		public List<ObjectBase> Heroes { get; set; }
		public GameUser() : base() {
			this.Heroes = new List<ObjectBase>();
		}
	}
}
