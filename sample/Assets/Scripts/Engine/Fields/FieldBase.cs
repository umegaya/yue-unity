using UnityEngine;
using System.Collections.Generic;
using ScriptEngine;

namespace ScriptEngine {
	namespace Fields {
		public class Base {
			//collection of all cells in fields
			public CellBase[,] Cells { get; set; }
			//x, y of cell size
			public int SizeX { get; set; }
			public int SizeY { get; set; }
			//all of each team member
			public Dictionary<string, TeamBase> Teams { get; set; }
			//all objectives
			public List<ObjectiveBase> Objectives { get; set; }
			//all events
			public List<EventBase> Events { get; set; }
			
			//constructor
			public Base() {
				this.Teams = new Dictionary<string, TeamBase>();
				this.Objectives = new List<ObjectiveBase>();
				this.Events = new List<EventBase>();
			}
		}
	}
}
