using UnityEngine;
using System.Collections.Generic;
using ScriptEngine;

namespace ScriptEngine {
	public class NPCObjectType : CharacterType {
		public int GainExp { get; set; }
		public int GainMoney { get; set; }
		public float WaitSec { get; set; }
	}
	public class NPCObject : Character {
		public float Cooldown { get; set; }		
	}
}
