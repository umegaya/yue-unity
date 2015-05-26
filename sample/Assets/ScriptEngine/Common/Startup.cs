using UnityEngine;
using System.Collections.Generic;
using System.Reflection;
using ScriptEngine;

namespace ScriptEngine {
	//have reponsibity for starting up script engine
	class ScriptStarter {
		static public void InitFixData(Dictionary<string, Dictionary<string, Dictionary<string, object>>> datas) {
			CellFactory.Initialize(datas["Cells"]);
			EventFactory.Initialize(datas["Events"]);
			ObjectiveFactory.Initialize(datas["Objectives"]);
			SkillFactory.Initialize(datas["Skills"]);
			TeamFactory.Initialize(datas["Teams"]);
			GroupFactory.Initialize(datas["Groups"]);
			ArrangementFactory.Initialize(datas["Arrangements"]);
			ObjectFactory.Initialize(datas["Objects"]);
		}
	}
}
