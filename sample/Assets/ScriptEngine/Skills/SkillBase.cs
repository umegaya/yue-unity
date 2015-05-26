using UnityEngine;
using System.Collections.Generic;
using ScriptEngine;

namespace ScriptEngine {
	public class SkillTypeBase : Util.FixData {
		public string Group { set; get; }
		public string Prefix { set; get; }
		public string Postfix { set; get; }
		public int Wp { set; get; }
		public List<string> AcceptGroups { set; get; }
		public string Range { set; get; } //group size which this skill applied to
		public string Scope { set; get; } //group attribute (friend/enemy) which this skill applied to
		public int Duration { set; get; }
	}
	public class SkillBase : System.ICloneable {
		public SkillTypeBase Type { get; set; }
		public int Duration { get; set; }
		public SkillBase() {}
		public object Clone() {
			return this.MemberwiseClone();
		}
	}
	public class SkillFactory : Util.Factory<SkillTypeBase, SkillBase> {}
}
