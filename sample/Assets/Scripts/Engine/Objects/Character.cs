using UnityEngine;
using System.Collections.Generic;
using ScriptEngine;

namespace ScriptEngine {
	public class CharacterType : ObjectTypeBase {	
		public int MaxHp { get; set; }
		public List<string> Skills { get; set; }
	}
	public class Character : ObjectBase {
		public List<SkillBase> Skills { get; set; }
		public List<SkillBase> Effects { get; set; }
		public bool IsDead { get { return Hp <= 0; } }
		public int MaxHp { get; set; }
		public int Hp { get; set; }
		
		//ctor
		public Character() : base() {
			this.Skills = new List<SkillBase>();
			this.Effects = new List<SkillBase>();
		}
	}
}
