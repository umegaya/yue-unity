using UnityEngine;
using System.Collections.Generic;
using ScriptEngine;

namespace ScriptEngine {
	public class User : ObjectBase {
		public object Peer { get; set; }
		
		public User() : base() {}
	}
}
