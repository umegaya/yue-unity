using UnityEngine;
using System.Collections.Generic;
using ScriptEngine;

namespace ScriptEngine {
	public class CharacterType : ObjectTypeBase {	
		public int MaxHp { get; set; }
		public int MaxWp { get; set; }
		public int Attack { get; set; }
		public int Defense { get; set; }
		public List<string> Skills { get; set; }
	}
	public class Character : ObjectBase {
		public List<SkillBase> Skills { get; set; }
		public List<SkillBase> Effects { get; set; }
		public List<ActionResult> ActionQueue { get; set; }
		public List<ActionResult> ComboChain { get; set; }
		public bool IsDead { get { return Hp <= 0; } }
		public int MaxHp { get; set; }
		public int Hp { get; set; }
		public int MaxWp { get; set; }
		public int Wp { get; set; }
		public int Attack { get; set; }
		public int Defense { get; set; }
		public int LastUpdateQueue { get; set; }
		
		//ctor
		public Character() : base() {
			this.Skills = new List<SkillBase>();
			this.Effects = new List<SkillBase>();
			this.ActionQueue = new List<ActionResult>();
			this.ComboChain = new List<ActionResult>();
			this.LastUpdateQueue = 0;
		}
	}
}
