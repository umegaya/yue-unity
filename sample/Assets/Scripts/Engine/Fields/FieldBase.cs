using UnityEngine;
using System.Collections.Generic;
using ScriptEngine;

namespace ScriptEngine {
	namespace Fields {
		public class Base {
			//collection of all cells in fields
			public Cells.Base[,] Cells { get; set; }
			//x, y of cell size
			public int SizeX { get; set; }
			public int SizeY { get; set; }
			//all of each team member
			public Dictionary<string, Teams.Base> Teams { get; set; }
			//all objectives
			public List<Objectives.Base> Objectives { get; set; }
			//all events
			public List<Events.Base> Events { get; set; }
			
			//constructor
			public Base() {
				this.Teams = new Dictionary<string, Teams.Base>();
				this.Objectives = new List<Objectives.Base>();
				this.Events = new List<Events.Base>();
			}
		}
	}
}
