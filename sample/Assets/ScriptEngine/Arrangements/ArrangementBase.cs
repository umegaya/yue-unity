using UnityEngine;
using System.Collections.Generic;
using ScriptEngine;

namespace ScriptEngine {
	//pattern of enemy which put in one cell
	public class GroupTypeBase : Util.FixData {
		public List<string> RandomList { get; set; }
		public List<string> FixedList { get; set; }
		public int Size { get; set; }
	}
	public class GroupBase {
		public GroupTypeBase Type { get; set; }
	}
	public class GroupFactory : Util.Factory<GroupTypeBase, GroupBase> {}
	
	//group pattern container for logical arrangement unit (eg. normal stage for single play or place ment for multi play mode)
	public class ArrangementTypeBase : Util.FixData {
		public Dictionary<string, List<string>> TeamMemberLists { get; set; }
	}
	public class ArrangementBase {
		public ArrangementTypeBase Type { get; set; }
	}
	public class ArrangementFactory : Util.Factory<ArrangementTypeBase, ArrangementBase> {}
}
