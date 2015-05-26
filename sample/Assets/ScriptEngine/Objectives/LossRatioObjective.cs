using UnityEngine;
using System.Collections.Generic;
using ScriptEngine;

namespace ScriptEngine {
	public class LossRatioObjectiveType : ObjectiveTypeBase {
		public int LossRatio { get; set; } //loss ratio threshold (0~100 percentage)
		public string TeamId { get; set; } //for which team?
	}
}
