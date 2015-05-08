using UnityEngine;
using System.Collections.Generic;
using System.Reflection;
using ScriptEngine;

namespace ScriptEngine {
	//have reponsibity for starting up script engine
	class ScriptStarter {
		static public void InitFixData(Dictionary<string, Dictionary<string, Dictionary<string, object>>> datas) {
			Cells.Factory.Initialize(datas["CellTypes"]);
			Events.Factory.Initialize(datas["Events"]);
			Objectives.Factory.Initialize(datas["Objectives"]);
			Skills.Factory.Initialize(datas["Skills"]);
			Teams.Factory.Initialize(datas["Teams"]);
			Objects.Factory.Initialize(datas["Objects"]);
		}
	}
}
