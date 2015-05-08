using UnityEngine;
using System.Collections.Generic;
using ScriptEngine;

namespace ScriptEngine {
	namespace CellTypes {
		public class Base : Util.FixData {
		}
	}
	namespace Cells {
		public class Base {
			// cell type
			public CellTypes.Base Type { get; set; }
			// team_id => all objects belongs to team_id
			public Dictionary<string, List<Objects.Base>> Teams { get; set; }
			
			public Base(CellTypes.Base t) {
				this.Type = t;
				this.Teams = new Dictionary<string, List<Objects.Base>>();
			}
		}
		public class Factory : Util.Factory<CellTypes.Base, Base> {}
	}
}
