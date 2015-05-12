using UnityEngine;
using System.Collections;
using ScriptEngine;

namespace ScriptEngine {
	public class SkillTypeBase : Util.FixData {
	}
	public class SkillBase {
		public SkillTypeBase Type { get; set; }
		
		public SkillBase() {
		}
	}
	public class SkillFactory : Util.Factory<SkillTypeBase, SkillBase> {}
}
