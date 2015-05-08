using UnityEngine;
using System.Collections.Generic;
using ScriptEngine;

namespace ScriptEngine {
	public class TeamTypeBase : Util.FixData {
		//friendly team id
		public List<string> FriendlyTeams { get; set; }
		//hostile team id
		public List<string> HostileTeams { get; set; }
	}
	public class TeamBase {
		//team type
		public TeamTypeBase Type { get; set; }
		//objects belongs to this team
		public List<ObjectBase> BelongsTo { get; set; }
		
		public TeamBase(TeamTypeBase t) {
			this.Type = t;
			this.BelongsTo = new List<ObjectBase>();
		}
	}
	public class TeamFactory : Util.Factory<TeamTypeBase, TeamBase> {}
}
