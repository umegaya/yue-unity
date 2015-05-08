using UnityEngine;
using System.Collections;
using ScriptEngine;

namespace ScriptEngine {
	namespace SkillTypes {
		public class Base : Util.FixData {
		}
	}
	namespace Skills {
		public class Base {
			public SkillTypes.Base Type { get; set; }
			
			public Base(SkillTypes.Base t) {
				this.Type = t;
			}
		}
		public class Factory : Util.Factory<SkillTypes.Base, Base> {}
	}
}
