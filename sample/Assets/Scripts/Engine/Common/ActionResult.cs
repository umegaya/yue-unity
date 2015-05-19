using UnityEngine;
using System.Collections.Generic;
using ScriptEngine;

namespace ScriptEngine {
	public class ActionResult {
		public int Type { get; set; }
		public string Name { get; set; }
		public object[] Args { get; set; }
		public List<ActionResult> ComboData { get; set; }
		public void InitComboData() {
			this.ComboData = new List<ActionResult>();
		}
		public ActionResult(int type, string name, object[] args) {
			this.Type = type;
			this.Name = name;
			this.Args = args;
		}
		public int IntArg(int idx) {
			return (int)this.Args[idx];
		}
		public string StrArg(int idx) {
			return (string)this.Args[idx];
		}
		public SkillBase SkillArg(int idx) {
			return (SkillBase)this.Args[idx];
		}
		public ObjectBase ObjectArg(int idx) {
			return (ObjectBase)this.Args[idx];			
		}
	}
	public class ActionResultFactory {
		static public ActionResult Create(int type, string name, params object[] args) {
			return new ActionResult(type, name, args);
		}
	}
}
