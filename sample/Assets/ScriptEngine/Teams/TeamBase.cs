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
		public Dictionary<int, ObjectBase> BelongsTo { get; set; }
		//user belongs to this team
		public Dictionary<int, User> UserBelongsTo { get; set; }
		//current score
		public int Score { get; set; }
		
		public TeamBase() {
			this.BelongsTo = new Dictionary<int, ObjectBase>();
			this.UserBelongsTo = new Dictionary<int, User>();
			this.Score = 0;
		}
	}
	public class TeamFactory : Util.Factory<TeamTypeBase, TeamBase> {}
}
