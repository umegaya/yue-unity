using UnityEngine;
using System.Collections.Generic;
using ScriptEngine;

namespace ScriptEngine {
	namespace TeamTypes {
		public class Base : Util.FixData {
			//friendly team id
			public List<string> FriendlyTeams { get; set; }
			//hostile team id
			public List<string> HostileTeams { get; set; }
		}
	}
	namespace Teams {
		public class Base {
			//team type
			public TeamTypes.Base Type { get; set; }
			//objects belongs to this team
			public List<Objects.Base> BelongsTo { get; set; }
			
			public Base(TeamTypes.Base t) {
				this.Type = t;
				this.BelongsTo = new List<Objects.Base>();
			}
		}
		public class Factory : Util.Factory<TeamTypes.Base, Base> {}
	}
}
