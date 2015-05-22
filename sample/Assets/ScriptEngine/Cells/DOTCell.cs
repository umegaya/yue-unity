using UnityEngine;
using System.Collections.Generic;
using ScriptEngine;

namespace ScriptEngine {
	public class DOTCellType : CellTypeBase {
		public string DamageName { get; set; }
		public int DamagePerTick { get; set; }
	}
}
