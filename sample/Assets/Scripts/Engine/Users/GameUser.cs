using UnityEngine;
using System.Collections.Generic;
using ScriptEngine;

namespace ScriptEngine {
	public class GameUserType : ObjectTypeBase {
		public float WaitSec { get; set; }
	}
	public class GameUser : User {
		public ObjectiveBase Objective { get; set; }
		public List<ObjectBase> Heroes { get; set; }
		public float LastCmdTime { get; set; }
		public float WaitSec { get; set; }
		public GameUser() : base() {
			this.Heroes = new List<ObjectBase>();
		}
	}
}
