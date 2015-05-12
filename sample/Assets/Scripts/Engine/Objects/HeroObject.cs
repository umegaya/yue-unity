using UnityEngine;
using System.Collections.Generic;
using ScriptEngine;

namespace ScriptEngine {
	public class HeroObjectType : CharacterType {
		public int OwnerId { get; set; } // user id who has this object
		public int Exp { get; set; }
		public int Level { get; set; }
	}
}
