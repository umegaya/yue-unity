using UnityEngine;
using System.Collections.Generic;
using ScriptEngine;

namespace ScriptEngine {
	public class FieldBase {
		//collection of all cells in fields
		public List<List<CellBase>> Cells { get; set; }
		//x, y of cell size
		public int SizeX { get; set; }
		public int SizeY { get; set; }
		//all of each team member
		public Dictionary<string, TeamBase> Teams { get; set; }
		//all objectives
		public List<ObjectiveBase> Objectives { get; set; }
		//all events
		public List<EventBase> Events { get; set; }
		//object arrangement
		public ArrangementBase Arrangement { get; set; }
		//last update
		public float LastUpdate { get; set; }
		//this field finished?
		public bool Finished { get; set; }
		//id - object mapping
		public Dictionary<int, ObjectBase> ObjectMap { get; set; }

		
		//constructor
		public FieldBase() {
			this.Teams = new Dictionary<string, TeamBase>();
			this.Objectives = new List<ObjectiveBase>();
			this.Events = new List<EventBase>();
			this.ObjectMap = new Dictionary<int, ObjectBase>();
			this.Finished = false;
		}
		
		//initialize field cells
		public void InitCells(List<List<string>> ids) {
			this.SizeX = ids[0].Count;
			this.SizeY = ids.Count;
			this.Cells = new List<List<CellBase>>(this.SizeY);
			for (int i = 0; i < this.SizeY; i++) {//y
				var rows = new List<CellBase>(this.SizeX);
				for (int j = 0; j < this.SizeX; j++) {//x
					rows.Add(CellFactory.Create(ids[j][i]));
				}
				this.Cells.Add(rows);
			}	
		}
		
		//find object from id
		public ObjectBase FindObject(int id) { 
			ObjectBase o;
			return this.ObjectMap.TryGetValue(id, out o) ? o : null;
		}
		
		//get team
		public TeamBase GetTeam(string id) {
			return this.Teams[id];
		}
		
		//get cell
		public CellBase CellAt(int x, int y) {
			if (x < this.SizeX && y < this.SizeY) {
				return this.Cells[x][y];
			}
			return null;
		}			
	}
}
