using UnityEngine;
using System.Collections.Generic;
using ScriptEngine;

namespace ScriptEngine {
	public class CellTypeBase : Util.FixData {
	}
	public class CellBase {
		// cell type
		public CellTypeBase Type { get; set; }
		// team_id => all objects belongs to team_id
		public Dictionary<string, List<ObjectBase>> Teams { get; set; }
		
		public CellBase(CellTypeBase t) {
			this.Type = t;
			this.Teams = new Dictionary<string, List<ObjectBase>>();
		}
	}
	public class CellFactory : Util.Factory<CellTypeBase, CellBase> {}
}
